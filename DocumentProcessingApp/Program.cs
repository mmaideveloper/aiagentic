using DocumentProcessing.API.Services;
using DocumentProcessingApp.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Serilog;

IdentityModelEventSource.ShowPII = true;
//IdentityModelEventSource.LogCompleteSecurityArtifact = true;

var builder = WebApplication.CreateBuilder(args);


var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ""; // Defaults to Production


var config = builder.Configuration
   .SetBasePath(AppContext.BaseDirectory) // Ensure correct path
   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
   .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
   .Build();


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();


builder.Logging.AddConsole();
builder.Logging.AddSerilog();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

// Add services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//   .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            //context.ProtocolMessage.Prompt = "consent"; // Forces user to grant permissions
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            return Task.CompletedTask;
        },
        OnAccessDenied = context =>
        {
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            return Task.CompletedTask;
        }
    };
});

builder.AddServiceDefaults();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "3.0.1" });
});

builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<DocumentIntellignetService>();

var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}


app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/test", () => new
{
    Name = "Example Minimal API",
    Version = "0.0.1-preview",
    Status = "Running"
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Web Api has started successfully!");
logger.LogInformation($"Start API: {environment} dir:{AppContext.BaseDirectory}");


app.Run();

