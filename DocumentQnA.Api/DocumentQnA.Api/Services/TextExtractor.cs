using CsvHelper;
using System.Globalization;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using OfficeOpenXml;

namespace DocumentQnA.Api.Services
{
    public class TextExtractor : ITextExtractor
    {
        public async Task<string> ExtractTextAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();

            return extension switch
            {
                ".pdf" => ExtractPdf(filePath),
                ".csv" => await ExtractCsvAsync(filePath),
                ".xlsx" => ExtractExcel(filePath),
                ".docx" => ExtractDocx(filePath),
                ".txt" => await ExtractTxtAsync(filePath),
                _ => "Unsupported file format."
            };
        }

        private string ExtractPdf(string filePath)
        {
            var sb = new StringBuilder();
            using var document = PdfDocument.Open(filePath);
            foreach (Page page in document.GetPages())
                sb.AppendLine(page.Text);

            return sb.ToString();
        }

        private async Task<string> ExtractCsvAsync(string filePath)
        {
            var sb = new StringBuilder();
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<dynamic>();
            await foreach (var record in csv.GetRecordsAsync<dynamic>())
            {
                records.Add(record);
            }

            foreach (var record in records)
            {
                var row = (IDictionary<string, object>)record;
                sb.AppendLine(string.Join(" | ", row.Select(kv => $"{kv.Key}: {kv.Value}")));
            }

            return sb.ToString();
        }

        private string ExtractExcel(string filePath)
        {
            var sb = new StringBuilder();
            var fileInfo = new FileInfo(filePath);
            using var package = new ExcelPackage(fileInfo);

            foreach (var sheet in package.Workbook.Worksheets)
            {
                sb.AppendLine($"Sheet: {sheet.Name}");

                if (sheet.Dimension == null)
                {
                    sb.AppendLine("(Empty sheet)");
                    continue;
                }

                for (int row = sheet.Dimension.Start.Row; row <= sheet.Dimension.End.Row; row++)
                {
                    for (int col = sheet.Dimension.Start.Column; col <= sheet.Dimension.End.Column; col++)
                    {
                        var cell = sheet.Cells[row, col]?.Text ?? "";
                        sb.Append($"{cell}\t");
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string ExtractDocx(string filePath)
        {
            var sb = new StringBuilder();
            using var wordDoc = WordprocessingDocument.Open(filePath, false);
            var body = wordDoc.MainDocumentPart?.Document.Body;
            if (body != null)
                sb.AppendLine(body.InnerText);

            return sb.ToString();
        }

        private async Task<string> ExtractTxtAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }
    }
}
