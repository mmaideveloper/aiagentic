using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using Polly;
using Polly.Timeout;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

IdentityModelEventSource.ShowPII = true;

// This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
// By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
// For instance, 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles' claim.
// This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development"; // Defaults to Production

var config = builder.Configuration
   .SetBasePath(AppContext.BaseDirectory) // Ensure correct path
   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
   .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
   .Build();

Log.Logger = new LoggerConfiguration()
.ReadFrom.Configuration(config)
.CreateLogger();

//builder.Host.UseSerilog();
// Enables console logging
//builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddSerilog();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1200))
    .WithPolicyKey("HttpTimeoutPolicy");

var retryPolicy = Policy
    .Handle<TimeoutRejectedException>()
    .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, 20 * retryAttempt)));

ThreadPool.SetMinThreads(100, 50);

//.net aspire
builder.Services.AddHttpClient("documentprocessingapi",
    client =>
    {
        var url = config.GetValue<string>("Api");
        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(2000);
        Log.Logger.Information("Set timeout to 120 sec");
    })
 .AddPolicyHandler(timeoutPolicy)
.AddServiceDiscovery()
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
})
.ConfigureHttpClient(client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan; // ⏳ Force unlimited timeout at HttpClient level
});
/*
.AddResilienceHandler("api", options =>
{
    options.AddTimeout(new TimeSpan(0, 20, 0));
});*/



builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
     .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
     .EnableTokenAcquisitionToCallDownstreamApi()
     .AddInMemoryTokenCaches();

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    //options.Scope.Clear();
    options.Scope.Add(config["AzureAd:Scopes"]);
});

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            //context.ProtocolMessage.Prompt = "consent"; // Forces user to grant permissions
            return Task.CompletedTask;
        },
        OnAccessDenied = context =>
        {
            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnUserInformationReceived = context =>
        {
            return Task.CompletedTask;
        },
        OnTokenResponseReceived = context =>
        {
            context.HttpContext.Session.SetString("access_token", context.TokenEndpointResponse?.AccessToken);
            Console.WriteLine($"Access token: {context.TokenEndpointResponse?.AccessToken}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSession();
builder.AddServiceDefaults();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment() &&
    app.Environment.EnvironmentName != "local")
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseStaticFiles();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Web App has started successfully!");
logger.LogInformation($"Start web env: {environment} dir:{AppContext.BaseDirectory}");

app.Run();
