using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;

namespace Cubes.Core.Email
{
    public class EmailDispatcher : IEmailDispatcher
    {
        private readonly int _maxRetries = 5;
        private readonly ISmtpClient _client;
        private readonly ILogger<EmailDispatcher> _logger;

        public EmailDispatcher(ISmtpClient client, ILogger<EmailDispatcher> logger)
        {
            _client = client;
            _logger = logger;
        }

        public virtual void DispatchEmail(EmailContent content, SmtpSettings smtpSettings)
        {
            var cleanup = new List<MemoryStream>();
            var mail = new MimeMessage();
            mail.Subject = content.Subject;
            mail.To.AddRange(content.ToAddresses.Select(adr => new MailboxAddress(adr)));
            mail.From.Add(new MailboxAddress(smtpSettings.Sender));

            var bodyBuilder = new BodyBuilder { TextBody = content.Body };
            if (content.Attachments?.Count() > 0)
            {
                foreach (var item in content.Attachments)
                {
                    var ms = new MemoryStream(item.Content);
                    bodyBuilder.Attachments.Add(item.FileName, ms);
                    cleanup.Add(ms);
                }
            }
            mail.Body = bodyBuilder.ToMessageBody();

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetry(_maxRetries,
                    retry => TimeSpan.FromSeconds(Math.Pow(2, retry)),
                    onRetry: (ex, ts, retry, __) =>
                    {
                        _logger.LogWarning($"Mail dispatch failed: {ex.Message}. Retrying after {ts.TotalSeconds} secs (retry {retry} of {_maxRetries})...");
                    });
            policy.Execute(() => _client.Send(mail, smtpSettings));

            cleanup.ForEach(m => m?.Dispose());
        }
    }
}