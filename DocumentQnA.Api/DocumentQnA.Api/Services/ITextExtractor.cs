namespace DocumentQnA.Api.Services
{
    public interface ITextExtractor
    {
        Task<string> ExtractTextAsync(string filePath);
    }

}
