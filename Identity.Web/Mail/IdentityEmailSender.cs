using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Identity.Web.Mail
{
    public class IdentityEmailSender : IEmailSender
    {
        private readonly SendGridClient _client;
        private readonly string _sender;
        public IdentityEmailSender(MailOptions options)
        {
            _client = new SendGridClient(options.Key);
            _sender = options.Sender;
        }


        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = new SendGridMessage
            {
                From = new EmailAddress(_sender),
                Subject = subject,
                HtmlContent = htmlMessage
            };
            mail.AddTo(email);

            var sendGridResponse = await _client.SendEmailAsync(mail);

            if (sendGridResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new Exception("Not authorized");
            }
            if (sendGridResponse.StatusCode != HttpStatusCode.Accepted)
            {
                throw new Exception("Failed. Status: " + sendGridResponse.StatusCode);
            }
        }
    }
}
