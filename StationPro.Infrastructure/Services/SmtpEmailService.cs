using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using StationPro.Application.Contracts.Services;
using StationPro.Application.Settings;
using StationPro.Infrastructure.Templates.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public SmtpEmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            // Detect forgot-password email by subject and use the dedicated template
            string htmlBody = subject.Contains("Reset", StringComparison.OrdinalIgnoreCase)
                ? BuildForgotPasswordHtml(body)
                : EmailTemplateBuilder.BuildGenericEmail(subject, body);

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = body   // plain-text fallback
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // Extract the URL from the plain body and pass it to the dedicated template
        private static string BuildForgotPasswordHtml(string plainBody)
        {
            var urlMatch = System.Text.RegularExpressions.Regex.Match(plainBody, @"https?://\S+");
            var resetLink = urlMatch.Success ? urlMatch.Value : "#";
            return EmailTemplateBuilder.BuildForgotPasswordEmail(resetLink);
        }
    }
}

