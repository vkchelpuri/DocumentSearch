using System.Text.Json;
using System.Text;
using System.Net;

namespace DocumentQnA.Api.Services
{
    /// <summary>
    /// Provides services for interacting with the Gemini AI model, implementing IGeminiServices.
    /// </summary>
    public class GeminiServices : IGeminiServices
    {
        private readonly HttpClient _http;
        private readonly string _geminiApiKey;
        private readonly IConfiguration _config;

        // Constants for Gemini API models
        private const string GENERATIVE_MODEL_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        private const string EMBEDDING_MODEL_URL = "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent"; // Gemini Embedding Model

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiServices"/> class.
        /// This constructor is used at RUNTIME when HttpClient is available via dependency injection.
        /// </summary>
        public GeminiServices(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
            _geminiApiKey = _config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey", "Gemini API key is not configured.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiServices"/> class.
        /// This constructor is primarily for DESIGN-TIME operations (e.g., EF Core migrations)
        /// where HttpClient might not be resolvable by the service provider.
        /// </summary>
        public GeminiServices(IConfiguration config)
        {
            // For design-time, create a basic HttpClient. Not for actual runtime use.
            _http = new HttpClient();
            _config = config;
            _geminiApiKey = _config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey", "Gemini API key is not configured.");
        }

        /// <summary>
        /// Generates content (e.g., an answer) and potentially identifies a source document
        /// based on a given prompt using the Gemini AI model.
        /// </summary>
        public async Task<(string answer, string sourceDocument)> GetAnswerWithDocumentAsync(string prompt)
        {
            if (_http == null)
            {
                return ("Gemini service is not properly initialized (HttpClient is null).", "None");
            }

            if (string.IsNullOrEmpty(_geminiApiKey))
            {
                throw new InvalidOperationException("Gemini API Key is not configured.");
            }

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
            var requestUri = $"{GENERATIVE_MODEL_URL}?key={_geminiApiKey}";

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
                return ("Could not retrieve a valid response from Gemini.", "None");
            }
            catch (JsonException)
            {
                return ("Error parsing Gemini response.", "None");
            }

            var lines = fullText?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var answer = "No answer";
            var sourceDocument = "None";

            var answerLine = lines.FirstOrDefault(l => l.StartsWith("answer:", StringComparison.OrdinalIgnoreCase));
            if (answerLine != null)
            {
                answer = answerLine.Substring("answer:".Length).Trim();
            }
            else
            {
                answer = fullText.Trim();
            }

            var sourceLine = lines.FirstOrDefault(l => l.StartsWith("sourceDocument:", StringComparison.OrdinalIgnoreCase));
            if (sourceLine != null)
            {
                string extractedSource = sourceLine.Substring("sourceDocument:".Length).Trim();
                if (!string.IsNullOrWhiteSpace(extractedSource) && !extractedSource.Equals("No document", StringComparison.OrdinalIgnoreCase))
                {
                    sourceDocument = extractedSource;
                }
            }

            return (answer, sourceDocument);
        }

        /// <summary>
        /// Generates a vector embedding for a given text using the Gemini embedding model.
        /// </summary>
        public async Task<double[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text), "Text for embedding cannot be null or empty.");
            }

            if (_http == null)
            {
                throw new InvalidOperationException("Gemini service is not properly initialized (HttpClient is null).");
            }

            if (string.IsNullOrEmpty(_geminiApiKey))
            {
                throw new InvalidOperationException("Gemini API Key is not configured.");
            }

            // Payload structure for the embedContent endpoint
            var payload = new
            {
                model = "models/embedding-001", // Specify the embedding model
                content = new
                {
                    parts = new[]
                    {
                        new { text = text }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var requestUri = $"{EMBEDDING_MODEL_URL}?key={_geminiApiKey}";

            var response = await _http.PostAsync(requestUri, content);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 60;
                throw new InvalidOperationException($"Gemini embedding rate limit exceeded. Please wait {retryAfter} seconds.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errText = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Gemini embedding API error ({(int)response.StatusCode}): {errText}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            try
            {
                // Navigate to the embedding array in the JSON response
                var embeddingArray = doc.RootElement
                                        .GetProperty("embedding")
                                        .GetProperty("values")
                                        .EnumerateArray()
                                        .Select(v => v.GetDouble())
                                        .ToArray();

                return embeddingArray;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error parsing Gemini embedding response: {ex.Message}", ex);
            }
        }
    }
}
