using Google.Protobuf.WellKnownTypes;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.DocumentProcessing_API>("documentprocessingapi")
    .WithExternalHttpEndpoints();

var web = builder.AddProject<Projects.DocumentIntelligenceWeb>("documentintelligenceweb")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

var apiChat = builder.AddProject<Projects.CopilotChatWebApi>("ai-chat-webapi")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.AI_Agentic_Fronend>("ai-chat-webapp")
    .WithExternalHttpEndpoints()
    .WithReference(apiChat)
    .WaitFor(apiChat);

/*
builder.AddNpmApp("ai-chat-webapp", "../AI.Agentic.Frondend")
    .WithReference(apiChat)
    .WaitFor(apiChat)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();
*/

var app = builder.Build();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ""; // Defaults to Production

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("AI Host has started successfully!");
logger.LogInformation($"Start Host Env: {environment} dir:{AppContext.BaseDirectory}");


app.Run();
