using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Linq; // Add this for LINQ methods like .FirstOrDefault()

namespace DocumentQnA.Api.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _geminiApiKey;

        public GeminiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
            _geminiApiKey = _config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey", "Gemini API key is not configured.");
        }

        public async Task<(string answer, string sourceDocument)> GetAnswerWithDocumentAsync(string prompt)
        {
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt } 
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";

            var response = await _http.PostAsync(requestUri, content);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 60;
                throw new InvalidOperationException($"Gemini rate limit exceeded. Please wait {retryAfter} seconds.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errText = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Gemini API error ({(int)response.StatusCode}): {errText}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            string fullText;
            try
            {
                fullText = doc.RootElement
                                  .GetProperty("candidates")[0]
                                  .GetProperty("content")
                                  .GetProperty("parts")[0]
                                  .GetProperty("text")
                                  .GetString();
            }
            catch (KeyNotFoundException)
            {
                // Handle cases where the expected structure might be missing (e.g., empty response or different format)
                return ("Could not retrieve a valid response from Gemini.", "None");
            }
            catch (JsonException)
            {
                // Handle cases where JSON parsing fails unexpectedly
                return ("Error parsing Gemini response.", "None");
            }

            var lines = fullText?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var answer = "No answer";
            var sourceDocument = "None"; // Default to "None"

            // Find the answer line
            var answerLine = lines.FirstOrDefault(l => l.StartsWith("answer:", StringComparison.OrdinalIgnoreCase));
            if (answerLine != null)
            {
                answer = answerLine.Substring("answer:".Length).Trim();
            }
            else
            {
                // If "answer:" isn't found, assume the entire response is the answer
                // This is common for greetings or direct, non-document-based answers.
                answer = fullText.Trim();
            }

            // Find the sourceDocument line
            var sourceLine = lines.FirstOrDefault(l => l.StartsWith("sourceDocument:", StringComparison.OrdinalIgnoreCase));
            if (sourceLine != null)
            {
                string extractedSource = sourceLine.Substring("sourceDocument:".Length).Trim();
                // Even if a source line is present, if it's empty or a placeholder like "No document", set to "None"
                if (!string.IsNullOrWhiteSpace(extractedSource) && !extractedSource.Equals("No document", StringComparison.OrdinalIgnoreCase))
                {
                    sourceDocument = extractedSource;
                }
            }
            // If sourceLine is null or its content is "No document", sourceDocument remains "None" as initialized.

            return (answer, sourceDocument);
        }
    }
}