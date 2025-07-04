using Azure;
using DocumentProcessingApp.Modules;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.IO;

namespace DocumentProcessing.API.Services
{
    public class DocumentIntellignetService
    {
        private readonly IConfiguration _configuration;

        public DocumentIntellignetService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DocumentResponse> ProcessDocument(DocumentRequest request)
        {
            var key = _configuration["AzureDocumentIntelligenceService:Key"];
            var endpoint = _configuration["AzureDocumentIntelligenceService:Url"];

            AzureKeyCredential credential = new AzureKeyCredential(key);
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);
            Uri fileUri = new Uri(request.FilePath);
            var options = new AnalyzeDocumentOptions
            {
                Pages = { "1-5" }
            };
            //options.Features.Clear();
            //options.Features.Add(DocumentAnalysisFeature.KeyValuePairs);

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, request.Model, fileUri, options);

            AnalyzeResult result = operation.Value;

            var response = new DocumentResponse
            {
                Name = request.Name,
                Fields = new Dictionary<string, Field>(),
                DocumentConfidence = result.Documents.FirstOrDefault()?.Confidence ?? 0,
                TotalFields = result.Documents.FirstOrDefault()?.Fields?.Count()?? 0
            };

            foreach (var doc in result.Documents)
            {
                foreach (var field in doc.Fields.OrderBy(f=> f.Key))
                {
                    Console.WriteLine("Field:" + field.Key + "=" + field.Value.Content);

                    response.Fields.Add(field.Key, new Field
                    {
                        Name = field.Key,
                        Confidence = field.Value.Confidence.ToString()!,
                        Region = field.Value?.BoundingRegions?.FirstOrDefault() ?? new BoundingRegion(),
                        Value = field.Value?.Content ?? "unknown"
                    });
                }
            }

            return response;
        }

    }
}
