using DocumentIntelligenceWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Polly.Timeout;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DocumentIntelligenceWeb.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    private readonly ILogger<HomeController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenAcquisition _tokenAcquisition;

    //private readonly IServiceProvider _serviceProvider;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration, 
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor
        ,ITokenAcquisition tokenAcquisition
        //, IServiceProvider serviceProvider
        )
    {
        _tokenAcquisition = tokenAcquisition;
        
        _httpContextAccessor = httpContextAccessor;
        
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;

        _httpContextAccessor = httpContextAccessor;

        //_serviceProvider = serviceProvider;
        _logger.LogInformation("Hoe controller");
    }

    public IActionResult Index()
    {

        _logger.LogInformation("home page");
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        return token;
    }

    private string PrepareContainerPath()
    {
        var containerPath = _configuration["AzureContainerApp:Path"] ?? "";
        if (!string.IsNullOrEmpty(containerPath))
        {
            containerPath = "/" + containerPath + "/uploads";
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

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file , string modul)
    {
        //var client = _httpClientFactory.CreateClient("documentprocessingapi");
        using var client = new HttpClient();
        var url = _configuration["Api"];
        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(1200);
        //if ( client == null)
        //{
        //    client = _httpClientFactory.CreateClient();
        //    var apiUrl = _configuration["Api-NoAspire"];
        //    client.BaseAddress = new Uri(apiUrl);
        //}

        _logger.LogInformation($"User Identity: {_httpContextAccessor.HttpContext?.User?.Identity?.Name}");   
        _logger.LogInformation($"Is Authenticated: {_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated}");
        var token = await GetAccessTokenAsync();


        if (token == null)
        {
            try
            {
                //var tokenAcquisition = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
                    new[] { "https://matonokB2C.onmicrosoft.com/6d231866-0947-4152-bc97-80ee8e3f3655/access_as_user" });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                token = this.HttpContext.Session.GetString("access_token");          
            }
        }

        if (token == null)
        {
            //return Redirect("/MicrosoftIdentity/Account/SignOut");
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        try
        {
            UploadRequest model = new UploadRequest
            {
                File = file,
                Model = modul
            };

            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            var folder = PrepareContainerPath();

            using var form = new MultipartFormDataContent();
            var filePath = Path.Combine($"{folder}", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(streamContent, "file", Path.GetFileName(filePath));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Send request to API
            var response = await client.PostAsync($"/api/upload?model={modul}", form);

            if (response.IsSuccessStatusCode)
            {
                var resultW = await response.Content.ReadAsStringAsync();
                var documentResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadResponse>(resultW);

                return View(documentResponse);
            }
            else
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = this.Request.HttpContext.TraceIdentifier,
                    Error = $"{response.StatusCode} {response.ReasonPhrase}"
                });
            }
        }
        catch (TimeoutRejectedException ex)
        {
            return View("Error", new ErrorViewModel
            {
                RequestId = this.Request.HttpContext.TraceIdentifier,
                Error = "Timeout"
            });
        }
        catch (Exception ex)
        {
            return View("Error", new ErrorViewModel
            {
                RequestId = this.Request.HttpContext.TraceIdentifier,
                Error = ex.Message
            });
        }

       
    }
}
