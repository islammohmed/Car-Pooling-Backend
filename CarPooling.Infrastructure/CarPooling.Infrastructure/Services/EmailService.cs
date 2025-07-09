using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CarPooling.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarPooling.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient(_config["Email:Host"])
            {
                Port = int.Parse(_config["Email:Port"]),
                Credentials = new NetworkCredential(
                    _config["Email:Username"],
                    _config["Email:Password"]
                ),
                EnableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "true")
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:Username"], _config["Email:DisplayName"] ?? "Car Pooling App"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);
            await smtpClient.SendMailAsync(mail);
        }
    }
}
