namespace DocumentProcessingApp.Modules
{
    public class DocumenProcessResponse
    {
        public string Message { get; internal set; }
        public string FileName { get; internal set; }
        public string Url { get; internal set; }
        public DocumentResponse DocumentResponse { get; internal set; }

        public string JsonUrl { get; internal set; }
    }
}
