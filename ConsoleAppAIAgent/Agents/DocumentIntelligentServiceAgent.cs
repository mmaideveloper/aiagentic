using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Azure;
using Microsoft.SemanticKernel.Agents;
using OpenAI.Assistants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0110

namespace ConsoleAppAIAgent.Agents
{
    internal class DocumentIntelligentServiceAgent
    {
        private readonly IConfiguration _configuration;
        public DocumentIntelligentServiceAgent(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Do()
        {
            var connectionString = "eastus2.api.azureml.ms;5c0078f8-78f1-49be-9917-bdcc35f5e831;rg-mmatonok-6104_ai;mmatonok-8994";
            string tenantId = "f3d3d42a-7c6d-4a44-aceb-ff9d7839f6d3";
            string clientId = "d877e829-4f9b-418d-b984-bff62020b4c8";
            string clientSecret = _configuration["AzureSettings:Secret"];

            KernelPlugin plugin = KernelPluginFactory.CreateFromType<LightsPlugin>();
            // Create credentials using ClientSecretCredential
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            PersistentAgentsClient agentsClient = new PersistentAgentsClient(connectionString, credential);

            // 1. Define an agent on the Azure AI agent service
            PersistentAgent definition = await agentsClient.Administration.CreateAgentAsync(
                "mmLocalDocumentItellignetAgent",
                name: "docIntAgent",
                description: "Connect to azure document intelligent service",
                instructions: "Connect to document intelligent service and from pdf file return fields and values in json format");

            // 2. Create a Semantic Kernel agent based on the agent definition
            AzureAIAgent agent = new(definition, agentsClient, plugins: [plugin]);
            AzureAIAgentThread agentThread = new(agent.Client);
            try
            {
                ChatMessageContent message = new(AuthorRole.User, "<your user input>");
                await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
                {
                    Console.WriteLine(response.Content);
                }
            }
            finally
            {
                //await agentThread.DeleteAsync();
                //await agent.Client.DeleteAgentAsync(agent.Id);
            }
        }
    }
}
