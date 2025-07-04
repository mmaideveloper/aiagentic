// Copyright (c) Microsoft. All rights reserved.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CopilotChat.WebApi.Controllers;
using CopilotChat.WebApi.Extensions;
using CopilotChat.WebApi.Hubs;
using CopilotChat.WebApi.Models.Storage;
using CopilotChat.WebApi.Options;
using CopilotChat.WebApi.Storage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Graph;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;

namespace CopilotChat.WebApi.Plugins.Chat;

/// <summary>
/// This class is a plugin that calls Custom Azure Document Inteliggence Service
/// </summary>
public sealed class CustomDocumentServicePlugin
{
    private readonly string _bearerToken;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tenantId;
    private readonly string _authority;
    private readonly int _responseTokenLimit = 128000;
    private readonly IConfiguration _configuration;
    private readonly BearerAuthenticationProvider _authenticationProvider;
    private readonly ChatMemorySourceRepository _sourceRepository;
    private readonly IHubContext<MessageRelayHub> _messageRelayHubContext;

    /// <summary>
    /// A kernel instance to create a completion function since each invocation
    /// of the <see cref="ChatAsync"/> function will generate a new prompt dynamically.
    /// </summary>
    private readonly Kernel _kernel;

    /// <summary>
    /// Client for the kernel memory service.
    /// </summary>
    private readonly IKernelMemory _memoryClient;

    private readonly ChatMessageRepository _messageRepository;
    ///// <summary>
    ///// A repository to save and retrieve chat messages.
    ///// </summary>
    //private readonly ChatMessageRepository _chatMessageRepository;

    ///// <summary>
    ///// A repository to save and retrieve chat sessions.
    ///// </summary>
    //private readonly ChatSessionRepository _chatSessionRepository;

    ///// <summary>
    ///// A SignalR hub context to broadcast updates of the execution.
    ///// </summary>
    //private readonly IHubContext<MessageRelayHub> _messageRelayHubContext;

    ///// <summary>
    ///// Settings containing prompt texts.
    ///// </summary>
    //private readonly PromptsOptions _promptOptions;

    ///// <summary>
    ///// A kernel memory retriever instance to query semantic memories.
    ///// </summary>
    //private readonly KernelMemoryRetriever _kernelMemoryRetriever;

    //
    // Summary:
    //     Initializes a new instance of the MsGraphOboPlugin to execute the API calls using the OBO Flow.
    //     class.
    //
    // Parameters:
    //   bearerToken:
    //     The bearer token to received by the WebAPI and used to obtain a new access token using the OBO Flow.
    //
    //   clientFactory:
    //     The factory to use to create HttpClient instances.
    //
    //   PlannerOptions.OboOptions:
    //     Configuration for the plugin defined in appsettings.json.
    public CustomDocumentServicePlugin(
        string bearerToken, 
        IHttpClientFactory clientFactory, 
        int responseTokenLimit, 
        ILogger logger, 
        IConfiguration configuration,
        BearerAuthenticationProvider authenticationProvider,
        Kernel kernel,
        IKernelMemory memoryClient,
        //ChatMessageRepository chatMessageRepository,
        //ChatSessionRepository chatSessionRepository,
        //IHubContext<MessageRelayHub> messageRelayHubContext,
        //IOptions<PromptsOptions> promptOptions,
        //IOptions<DocumentMemoryOptions> documentImportOptions,
        ChatMemorySourceRepository sourceRepository,
        IHubContext<MessageRelayHub> messageRelayHubContext,
        ChatMessageRepository messageRepository
        )
    {
        this._bearerToken = bearerToken ?? throw new ArgumentNullException(bearerToken);
        this._clientFactory = clientFactory;
        this._logger = logger;
        this._responseTokenLimit = responseTokenLimit;
        _configuration = configuration;

        //this._clientId = _configuration["Plugins:CustomDocumentService:ClientId"] ?? throw new ArgumentNullException("ClientId");
        //this._clientSecret = _configuration["Plugins:CustomDocumentService:ClientSecret"] ?? throw new ArgumentNullException("ClientSecret");
        //this._tenantId = _configuration["Plugins:CustomDocumentService:TenantId"] ?? throw new ArgumentNullException("TenantId");
        //this._authority = _configuration["Plugins:CustomDocumentService:Authority"] ?? throw new ArgumentNullException("Authority");

        this._authenticationProvider = authenticationProvider;

        this._kernel = kernel;
        this._memoryClient = memoryClient;
        //this._chatMessageRepository = chatMessageRepository;
        //this._chatSessionRepository = chatSessionRepository;
        //this._messageRelayHubContext = messageRelayHubContext;
        //// Clone the prompt options to avoid modifying the original prompt options.
        //this._promptOptions = promptOptions.Value.Copy();

        //this._kernelMemoryRetriever = new KernelMemoryRetriever(promptOptions, chatSessionRepository, memoryClient, logger);
        this._sourceRepository = sourceRepository;
        this._messageRelayHubContext = messageRelayHubContext;
        this._messageRepository = messageRepository;
    }

    [KernelFunction("demo"), Description("check if custom document service is enabled.")]
    public async Task<string> DemoCallApiTasksAsync(
        string chatId,
        CancellationToken cancellationToken = default)
    {
        return "This is test custom document service plugin.";
    }
        
    [KernelFunction("extract_fields_form_201_from_pdf_document"), 
        Description("Extracts structured fields from a PDF using the company's custom Document Intelligence model. Use this for all PDF field extraction.")]
    public async Task<string> CallApiTasksAsync(
        [Description("string with the action")] string action,
        [Description("string with the document local path or url needed to execute the API call")]
        string filePath,
        [Description("string with file name that was upload, or ask for upload a file for that action")] string fileName,
        [Description("string with id of file that was uploaded")] string fileId,
         [Description("Chat ID to extract history from")]
        string chatId,
        [Description("file stream of bytes of uploaded files represented as base64")] string fileStreamAsString = null,
        [Description("string with the LLM module name needed to execute the API call")] string module = null,
        CancellationToken cancellationToken = default)
    {
        await this._messageRepository.CreateAsync(CopilotChatMessage.CreateBotResponseMessage(chatId, "test", "prompt_test", null, null));

        var chatHistory = new ChatHistory();
       
        try {

            chatHistory.AddAssistantMessage($"Customer Document Intelligne Service for FORM 201 processing file: {fileName}");

        
        var token = await _authenticationProvider.GetToken();
        var plugins = _configuration.GetSection("Plugins").Get<List<Plugin>>() ?? new List<Plugin>();
        var apiUrl = plugins.FirstOrDefault(p => p.Name == "CustomDocumentService")?.ApiUrl;
        var apiToCall = $"{apiUrl}api/upload";

            chatHistory.AddAssistantMessage($"Customer Document Intelligne Service {fileName} loaded.");

            var graphResponseContent = string.Empty;
        var accessToken = token; // await this.GetAccessTokenAsync(cancellationToken);

            using (HttpClient client = this._clientFactory.CreateClient())
            {
                using var form = new MultipartFormDataContent();
                //using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var fileStream = await GetUploadedFile(filePath, fileName, fileId, fileStreamAsString, chatId);
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                form.Add(streamContent, "file", fileName );
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                chatHistory.AddAssistantMessage($"Customer Document Intelligne Service for FORM 201 processing file: {fileName}. Request send to {apiToCall}");

                // Send request to API
                var response = await client.PostAsync($"{apiToCall}?model={module}", form);
                var resultW = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    chatHistory.AddDeveloperMessage(resultW);

                    chatHistory.AddAssistantMessage($"Customer Document Intelligne Service for FORM 201 finalized file: {fileName}. Sucessfully.");
                    return resultW;
                }
                else
                {
                    chatHistory.AddDeveloperMessage($"Error from {apiToCall}.{resultW} Status code: {response.Version.ToString()}");

                    chatHistory.AddAssistantMessage($"Customer Document Intelligne Service for FORM 201 processing file: {fileName}. Failed.");

                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling CustomDocumentServicePlugin: {Message}", ex.Message);
            chatHistory.AddDeveloperMessage(ex.Message);
        }

        return string.Empty;
    }

    private async Task<FileStream> GetUploadedFile(string documentUrl, string fileName, string fileId, string fileStreamAsString, string chatId)
    {
        var plugins = _configuration.GetSection("Plugins").Get<List<Plugin>>();
        var myPlugin = plugins.FirstOrDefault(p => p.Name == "CustomDocumentService");
        var tmpPath = myPlugin.FilePath;

        if (!System.IO.Directory.Exists(tmpPath))
        {
            System.IO.Directory.CreateDirectory(tmpPath);
        }

        var filePath = Path.Combine(tmpPath, fileName);
        FileStream fileStream = null;
        //save to temp folder
        if( System.IO.File.Exists(filePath))
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
        }

        fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);

        if (!string.IsNullOrEmpty(fileStreamAsString))
        {
            byte[] data = Convert.FromBase64String(fileStreamAsString);
            fileStream.Write(data, 0, data.Length);
        }
        else
        {

            //IEnumerable<MemorySource> sources = await this._sourceRepository.FindByChatIdAsync(chatId);
            IEnumerable<MemorySource> sources = await this._sourceRepository.FindByNameAsync(fileName);
            var fileObj = sources.FirstOrDefault(s => s.SourceType == MemorySourceType.File && s.Name.IndexOf(fileName) >= 0);
            if (fileObj == null)
            {
                throw new Exception($"{documentUrl} does not exist.");
            }

            byte[] data = fileObj.BinaryContent;
            fileStream.Write(data, 0, data.Length);
            fileStream.Position = 0;        
        }

        return fileStream;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var oboToken = string.Empty;

        using (HttpClient client = this._clientFactory.CreateClient())
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, this._authority + "/" + this._tenantId + "/oauth2/v2.0/token"))
            {
                var keyValues = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                    new("client_id", this._clientId),
                    new("client_secret", this._clientSecret),
                    new("assertion", this._bearerToken),
                    new("scope", "access_as_user"),
                    new("requested_token_use", "on_behalf_of")
                };

                request.Content = new FormUrlEncodedContent(keyValues);
                var response = await client.SendAsync(request, cancellationToken);
                var responseContent = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                else
                {
                    throw new HttpRequestException($"Failed to get token: {response.StatusCode}");
                }

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("access_token", out JsonElement accessTokenElement))
                    {
                        oboToken = accessTokenElement.GetString();
                    }
                    else
                    {
                        throw new HttpRequestException("Failed to get access token");
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(oboToken))
        {
            throw new HttpRequestException("Failed to get access token");
        }

        return oboToken;
    }
}
