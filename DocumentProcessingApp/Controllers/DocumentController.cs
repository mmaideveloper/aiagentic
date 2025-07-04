using DocumentProcessing.API.Services;
using DocumentProcessingApp.Modules;
using DocumentProcessingApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentProcessingApp.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class DocumentController : ControllerBase
    {
        private readonly StorageService _storageService;
        private readonly DocumentIntellignetService _documentIntelligenceService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentController> _logger;
        public DocumentController(StorageService storageService, 
            ILogger<DocumentController> logger, DocumentIntellignetService documentIntellignetService, IConfiguration configuration)
        {
            _storageService = storageService;
            _documentIntelligenceService = documentIntellignetService;
            _configuration = configuration;
            _logger = logger;

            _logger.LogInformation("upload controller");
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("get");

            var containerPath = PrepareContainerPath();

            var fileName= $"test{DateTime.UtcNow.ToString("yyyyDDmmHHmmss")}.txt";

            var filePath = Path.Combine($"{containerPath}", fileName);

            _logger.LogInformation("Store to file test:" + filePath);

            System.IO.File.WriteAllText(filePath, "test01");

            return Ok();
        }

        private string PrepareContainerPath()
        {
            var containerPath = _configuration["AzureContainerApp:Path"] ?? "";
            if (!string.IsNullOrEmpty(containerPath))
            {
                containerPath = "/"+containerPath + "/uploads";
            }
            else
            {
                containerPath = "uploads";
            }

            if (!Directory.Exists(containerPath))
            {
                Directory.CreateDirectory(containerPath);
            }  
            
            return containerPath;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("secure")]
        public IActionResult GetSecure()
        {
            return Ok();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string model = "creditorInfo_us_bankruptcy_form_201_201512-v2")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var containerPath = PrepareContainerPath();
            var filePath = Path.Combine($"{containerPath}", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string localFilePath = Path.Combine(Environment.CurrentDirectory, filePath);
           
            //store to Azure Blob
            var storageResponse = await _storageService.StoreDocument(new Modules.StorageRequest
            {
                FilePath = localFilePath,
                Name = file.FileName
            });

            //connection to Document Intelligence Service
            var docResponse = await _documentIntelligenceService.ProcessDocument(new DocumentRequest
            {
                Name = file.FileName,
                FilePath = storageResponse.Url,
                Model = model
            });

            //store to Azure Blob
            //var jsonResult = await _storageService.StoreDocumentResult(new Modules.StorageRequest
            //{
            //    FilePath = localFilePath+".json",
            //    Name = file.FileName+".json"
            //});

            //send result
            return Ok(new DocumenProcessResponse { 
                Message = "File uploaded successfully", 
                FileName = file.FileName,
                Url = storageResponse.Url,
                DocumentResponse = docResponse,
                JsonUrl = null //jsonResult.Url
            });
        }
    }
}
