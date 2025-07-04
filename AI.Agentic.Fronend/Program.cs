using Microsoft.Extensions.FileProviders;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseDefaultFiles();

//app.MapDefaultEndpoints();
//app.UseForwardedHeaders();
//app.UseRouting();
//app.UseEndpoints(endpoints =>
//{
//endpoints.MapControllers();
//endpoints.MapFallbackToFile("index.html");
//});
// Serve React app from the "FrontendService/build" folder
/*
app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "src", "build")),
    RequestPath = "",
    EnableDirectoryBrowsing = false
});
*/



/*
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "src";
    if (env.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});
*/

app.Use(async (context, next) =>
{
    if (!context.Request.Path.Value.StartsWith("/api"))
    {
        context.Response.Redirect("index.html");
    }
    else
    {
        await next();
    }
});

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ""; // Defaults to Production

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("AI Chat WebApp has started successfully!");
logger.LogInformation($"Start App: {environment} dir:{AppContext.BaseDirectory}");



app.Run();
