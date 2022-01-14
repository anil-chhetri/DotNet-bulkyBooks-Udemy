using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBooks.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;

        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("dotnettrainging@gmail.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));

            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            //send email
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.mailtrap.io", 2525, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("6f1eb5a8c06a72", "c7c80106f9e9a5");
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }

                return Task.CompletedTask;
        }
    }
}
