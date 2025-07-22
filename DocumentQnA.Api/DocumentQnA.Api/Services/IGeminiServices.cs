// Services/IGeminiServices.cs
using System.Threading.Tasks;

namespace DocumentQnA.Api.Services
{
    /// <summary>
    /// Defines the contract for interacting with the Gemini AI service.
    /// </summary>
    public interface IGeminiServices
    {
        /// <summary>
        /// Generates content (e.g., an answer) and potentially identifies a source document
        /// based on a given prompt using the Gemini AI model.
        /// </summary>
        /// <param name="prompt">The input prompt or question for the Gemini model.</param>
        /// <returns>A tuple containing the generated answer (string) and the source document (string).</returns>
        Task<(string answer, string sourceDocument)> GetAnswerWithDocumentAsync(string prompt);

        /// <summary>
        /// Generates a vector embedding for a given text using the Gemini embedding model.
        /// </summary>
        /// <param name="text">The text for which to generate the embedding.</param>
        /// <returns>A double array representing the vector embedding.</returns>
        Task<double[]> GenerateEmbeddingAsync(string text); // NEW METHOD
    }
}
