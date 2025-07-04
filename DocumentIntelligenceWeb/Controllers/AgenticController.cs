using DocumentIntelligenceWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DocumentIntelligenceWeb.Controllers;

[Authorize]
public class AgenticController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    private readonly ILogger<HomeController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenAcquisition _tokenAcquisition;

    //private readonly IServiceProvider _serviceProvider;

    public AgenticController(ILogger<HomeController> logger, IConfiguration configuration, 
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
        _logger.LogInformation("agentic page");
        return View();
    }
}
