using Microsoft.SqlServer.Server;
using System.Net.Mail;
using System.Net;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using CopilotChat.WebApi.Options;
using CopilotChat.WebApi.Plugins.Utils;

namespace CopilotChat.WebApi.Plugins.Services
{
    public class EmailAgent
    {
        private readonly IConfiguration _config;
        public EmailAgent(IConfiguration config)
        {
            _config = config;
        }

        [KernelFunction("isenabled_custom_send_email"),
        Description("Check if custom email sender is enabled")]
        public string IsEnabled()
        {
            return "Enabled";
        }
        
        [KernelFunction("send_email"),
        Description("Send Email with data in chat and email and subject that user is required to add.")]
        public string SendEmail(
            [Description("Recipient email address.")]
            string email,
            [Description("Email subject line.")]
            string subject,
            [Description("Email body content.")]
            string body)
        { 
            if (string.IsNullOrWhiteSpace(email)) return "Please provide a recipient email.";
            if (string.IsNullOrWhiteSpace(subject)) return "Please provide a subject.";

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
                Body = body
            };
            mail.To.Add(email);

            client.Send(mail);
            return $"Email sent to {email} ✔️";
        }
    }
}
