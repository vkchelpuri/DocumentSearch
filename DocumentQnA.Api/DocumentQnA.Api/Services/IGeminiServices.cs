namespace DocumentQnA.Api.Services
{

    public interface IGeminiServices
    {
        Task<(string answer, string sourceDocument)> GetAnswerWithDocumentAsync(string prompt);
        Task<double[]> GenerateEmbeddingAsync(string text);
    }
}
