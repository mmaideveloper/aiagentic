namespace DocumentIntelligenceWeb.Models
{
    public class UploadRequest
    {
        
        public string Model { get; set; } 

        public IFormFile File { get; set; }

        public string Response { get; set; }
    }
}
