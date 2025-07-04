namespace DocumentProcessingApp.Modules
{
    public class StorageRequest
    {
       public Stream StreamFile { get; set; }
       public string Name { get; set; }
        public string FilePath { get; internal set; }
    }
}
