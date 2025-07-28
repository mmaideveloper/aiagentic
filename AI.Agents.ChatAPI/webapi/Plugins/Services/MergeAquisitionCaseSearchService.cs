using CopilotChat.WebApi.Hubs;
using CopilotChat.WebApi.Plugins.Services;
using CopilotChat.WebApi.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace CustomPlugins;

public class PdfGeneratorAgent: BaseChatAgent
{
    public PdfGeneratorAgent(
            IHubContext<MessageRelayHub> messageRelayHubContext,
            IPluginAuthCredentialsService pluginAuthCredentialsService,
            KernelArguments variables,
            ILogger logger)
            : base(messageRelayHubContext, pluginAuthCredentialsService, variables, logger)
    {
        
    }
    [KernelFunction]
    [Description("Simulates search in external documentation for a given case number.")]
    public async Task<string> SearchDocumentation(string caseNumber)
    {
        await ShowMessage($"PDF document ready for case {caseNumber}");

        return $"📁 Document for case {caseNumber}:\n" +
               $"- https://docs.fake.gov/us-form201/{caseNumber}-results.pdf\n";
    }

    

}

