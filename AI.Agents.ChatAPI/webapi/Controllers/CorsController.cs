using Microsoft.AspNetCore.Mvc;

namespace CopilotChat.WebApi.Controllers
{
    /// <summary>
    /// Shows list of available origins 
    /// </summary>
    [ApiController]
    [Route("/cors")]
    public class CorsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CorsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult HandleOptions()
        {
            string[] allowedOrigins = _configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            return Ok(new
            {
                allowedOrigins = allowedOrigins
            });
        }
    }
}
