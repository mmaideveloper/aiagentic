namespace DocumentProcessingApp.Modules
{
    public class DocumentRequest
    {
        public string FilePath { get; internal set; }
        public string Name { get; internal set; }
        public string Model { get; internal set; }
    }
}
