using System.Net.Http;
using System.Text;
using System.Text.Json;

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

        public async Task<string> GetAnswerAsync(string prompt)
        {
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user", // Keep this, it's good practice for multimodal/chat models
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // --- THE NEW CHANGE ---
            // Update to the recommended model: gemini-1.5-flash
            var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";

            var response = await _http.PostAsync(requestUri, content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var answer = doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

            return answer ?? "No response from Gemini.";
        }
    }
}