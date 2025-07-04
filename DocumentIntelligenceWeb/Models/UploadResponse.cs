namespace DocumentIntelligenceWeb.Models
{
    public class UploadResponse
    {
        public string Response { get; set; }
        public string FileName { get; set; }

        public DocumentResponse DocumentResponse { get; set; }

        public string Url { get; set; }
        public string Message { get; set; }
    }

    public class DocumentResponse
    {
        public string Name { get; set; }
        public Dictionary<string, Field> Fields { get; set; }
        public float DocumentConfidence { get; set; }
        public int TotalFields { get; set; }
    }

    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; } = string.Empty;

        public string Confidence { get; set; }

        public BoundingRegion Region { get; set; }
    }

    public class BoundingRegion
    {
        public int PageNumber { get; set; }
        public List<BoundaryPolygon> BoundingPolygon { get;set;}

        public override string ToString()
        {
            var msg = $"Page:{PageNumber}";
            if (BoundingPolygon != null)
            {
                foreach (BoundaryPolygon polygon in BoundingPolygon)
                { msg += polygon.ToString() + ","; }
            }

            return msg;
        }
    }

    public class BoundaryPolygon
    {
        public double X { get; set; }
        public double Y { get; set; }

        public override string ToString()
        {
            return $"x:{X}, y:{Y}";
        }
    }
}
