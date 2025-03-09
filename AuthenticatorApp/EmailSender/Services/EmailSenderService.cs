using EmailSender.InterFaces;
using EmailSender.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EmailSender.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly GmailOptions _gmailOptions;
        public EmailSenderService(IOptions<GmailOptions> gmailOptions)
        {
            _gmailOptions = gmailOptions.Value;
        }

        public async Task SendEmailAsync(string Recepient, string Subject, string body)
        {
  
            if (string.IsNullOrWhiteSpace(Recepient))
            {
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(Recepient));
            }

            try
            {
                MailMessage message = new MailMessage()
                {
                    From = new MailAddress(_gmailOptions.Email),
                    Subject = Subject,
                    Body = body
                };

                message.To.Add(new MailAddress(Recepient));

                using var smtpClient = new SmtpClient
                {
                    Host = _gmailOptions.Host,
                    Port = _gmailOptions.Port, 
                    Credentials = new NetworkCredential(_gmailOptions.Email, _gmailOptions.Password),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(message);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid email format.", nameof(Recepient), ex);
            }

        }
    }
}
