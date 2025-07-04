using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppAIAgent.Agents
{
    /// <summary>
    /// Main agent
    /// <code>https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/connected-agents?pivots=csharp</code>
    /// </summary>
    internal class OrchestratorAgent
    {
        public void Do(string projectEndpoint, string modelDeploymentName)
        {
            PersistentAgentsClient client = new(projectEndpoint, new DefaultAzureCredential());

            //ConnectedAgentToolDefinition connectedAgentDefinition = new(new ConnectedAgentDetails(diAgent.Id, diAgent.Name, "Convert Upload Document to json"));

            PersistentAgent mainAgent = client.Administration.CreateAgent(
                model: modelDeploymentName,
                name: "stock_price_bot",
                instructions: "Your job is to get the stock price of a company, using the available tools.",
                tools: [] //[connectedAgentDefinition]
             );

            PersistentAgentThread thread = client.Threads.CreateThread();

            // Create message to thread
            PersistentThreadMessage message = client.Messages.CreateMessage(
                thread.Id,
                MessageRole.User,
                "What is the stock price of Microsoft?");

            // Run the agent
            ThreadRun run = client.Runs.CreateRun(thread, mainAgent);
            do
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                run = client.Runs.GetRun(thread.Id, run.Id);
            }
            while (run.Status == RunStatus.Queued
                || run.Status == RunStatus.InProgress);

            // Confirm that the run completed successfully
            if (run.Status != RunStatus.Completed)
            {
                throw new Exception("Run did not complete successfully, error: " + run.LastError?.Message);
            }

            Pageable<PersistentThreadMessage> messages = client.Messages.GetMessages(
                threadId: thread.Id,
                order: ListSortOrder.Ascending
            );

            foreach (PersistentThreadMessage threadMessage in messages)
            {
                Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
                foreach (MessageContent contentItem in threadMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        string response = textItem.Text;
                        if (textItem.Annotations != null)
                        {
                            foreach (MessageTextAnnotation annotation in textItem.Annotations)
                            {
                                if (annotation is MessageTextUriCitationAnnotation urlAnnotation)
                                {
                                    response = response.Replace(urlAnnotation.Text, $" [{urlAnnotation.UriCitation.Title}]({urlAnnotation.UriCitation.Uri})");
                                }
                            }
                        }
                        Console.Write($"Agent response: {response}");
                    }
                    else if (contentItem is MessageImageFileContent imageFileItem)
                    {
                        Console.Write($"<image from ID: {imageFileItem.FileId}");
                    }
                    Console.WriteLine();
                }

            }

            //agentClient.DeleteThread(threadId: thread.Id);
            //agentClient.DeleteAgent(agentId: agent.Id);
            //agentClient.DeleteAgent(agentId: connectedAgent.Id);
        }
    }
}
