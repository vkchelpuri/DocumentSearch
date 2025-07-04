namespace DocumentQnA.Api.Models
{
    public class DocumentText
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string RawText { get; set; }
        public DateTime UploadedAt { get; set; }
    }

}
