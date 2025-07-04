using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace DocumentProcessingApp.Modules
{
    public class DocumentResponse
    {
        public string Name { get;  set; }
        public Dictionary<string, Field> Fields { get;  set; }
        public float DocumentConfidence { get; internal set; }
        public int TotalFields { get; internal set; }
    }

    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; } = string.Empty;

        public string Confidence { get; set; }

        public BoundingRegion Region  { get; set; }
    }
}
