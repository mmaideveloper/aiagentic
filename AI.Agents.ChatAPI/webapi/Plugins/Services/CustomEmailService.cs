using Microsoft.SqlServer.Server;
using System.Net.Mail;
using System.Net;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using CopilotChat.WebApi.Options;
using CopilotChat.WebApi.Plugins.Utils;
using CopilotChat.WebApi.Models.Storage;
using CopilotChat.WebApi.Plugins.Chat;
using CopilotChat.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using CopilotChat.WebApi.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.Graph.TermStore;
using System.ComponentModel.DataAnnotations;

namespace CopilotChat.WebApi.Plugins.Services
{
    /// <summary>
    /// Setup for chat agent, to enable import it in chatcontroller!
    /// </summary>
    public class BaseChatAgent
    {
        protected readonly IHubContext<MessageRelayHub> _messageRelayHubContext;
        protected readonly IPluginAuthCredentialsService _pluginAuthCredentialsService;
        protected readonly KernelArguments _variables;
        protected readonly ILogger _logger;

        public BaseChatAgent(
            IHubContext<MessageRelayHub> messageRelayHubContext,
            IPluginAuthCredentialsService pluginAuthCredentialsService,
            KernelArguments variables,
            ILogger logger)
        {
            _messageRelayHubContext = messageRelayHubContext;
            _variables = variables;
            _pluginAuthCredentialsService = pluginAuthCredentialsService;
            _logger = logger;
        }

        protected virtual async Task ShowMessage(string message, string chatIdF = null, string userIdF = null)
        {
            var chatId = _variables["chatId"].ToString();
            var userId = _variables["userId"].ToString();
            var chatMessage = CopilotChatMessage.CreateBotResponseMessage(chatId, message, nameof(CustomDocumentServicePlugin), null, null);
            await this._messageRelayHubContext.Clients.Group(chatId).SendAsync("ReceiveMessage", chatId, userId, chatMessage);

        }
    }

    public class EmailAgent : BaseChatAgent
    {
        private readonly IConfiguration _config;

        public EmailAgent(
            IConfiguration config, 
            IHubContext<MessageRelayHub> messageRelayHubContext,
            IPluginAuthCredentialsService pluginAuthCredentialsService,
            KernelArguments variables,
            ILogger logger)
            : base(messageRelayHubContext, pluginAuthCredentialsService, variables, logger) 
        {
            _config = config;
        }

        [KernelFunction("isenabled_custom_send_email"),
        Description("Execute if user ask for check if custom email service is enabled")]
        public async Task<string> IsEnabled(
        [Description("Name of the user")] string userName)
        {
            var chatId = this._variables["chatId"];
            await ShowMessage($"Custom Email Service is enabled. Chat: {chatId} User:{userName}");
            //todo check credentials
            return "Custom Email Service is Enabled.";
        }
        
        [KernelFunction("send_email"),
        Description("Send Email. Email and subject user is required to add.")]
        public async Task<string> SendEmail(
            //KernelContent context,
            [Description("Recipient email address.")]
            string email,
            [Description("Email subject line.")]
            string subject,
            [Description("Email body content.")]
            string body    )
        { 
            if (string.IsNullOrWhiteSpace(email)) return "Please provide a recipient email.";
            if (string.IsNullOrWhiteSpace(subject)) return "Please provide a subject.";

            await ShowMessage($"Email is sending to {email}");

            var smtpSection = _config.GetPluginValue<SmtpOptions>("CustomEmailService", "SMTP");
            var client = new SmtpClient(smtpSection.Server, smtpSection.Port)
            {
                Credentials = new NetworkCredential(smtpSection.Username, smtpSection.Password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpSection.Username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,             
            };
            mail.To.Add(email);
            client.Send(mail);


            await ShowMessage($"Email sent to { email} ✔️");
            return $"Email sent to {email} ✔️";
        }
    }
}
