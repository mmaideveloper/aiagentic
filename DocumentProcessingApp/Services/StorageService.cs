using Azure.Storage.Blobs;
using DocumentProcessingApp.Modules;

namespace DocumentProcessingApp.Services
{
    
    public class StorageService
    {
        private readonly IConfiguration _configuration;

        public StorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<StorageResponse> StoreDocument(StorageRequest request)
        {
            string uri = _configuration["AzureStorage:Url"];

            var blobServiceClient = new BlobServiceClient(new Uri(uri));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("applicationdocuments");

            // Open file stream properly
            using FileStream uploadFileStream = File.OpenRead(request.FilePath);
            var blobClient = blobContainerClient.GetBlobClient(request.Name);

            if (uploadFileStream == null)
            {
                throw new ArgumentNullException(nameof(uploadFileStream), "File stream cannot be null.");
            }

            // Upload using file stream
            await blobClient.UploadAsync(uploadFileStream, overwrite: true);
            uploadFileStream.Close();

            return new StorageResponse
            {
                Url = blobClient.Uri.AbsoluteUri,
                Name = blobClient.Name
            };
        }

        public async Task<StorageResponse> StoreDocumentResult(StorageRequest request)
        {
            string uri = _configuration["AzureStorage:UrlResults"];

            var blobServiceClient = new BlobServiceClient(new Uri(uri));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("analyzeddocumentdata");

            // Open file stream properly
            using FileStream uploadFileStream = File.OpenRead(request.FilePath);
            var blobClient = blobContainerClient.GetBlobClient(request.Name);

            if (uploadFileStream == null)
            {
                throw new ArgumentNullException(nameof(uploadFileStream), "File stream cannot be null.");
            }

            // Upload using file stream
            await blobClient.UploadAsync(uploadFileStream, overwrite: true);
            uploadFileStream.Close();

            return new StorageResponse
            {
                Url = blobClient.Uri.AbsoluteUri,
                Name = blobClient.Name
            };
        }
    }

    
}
