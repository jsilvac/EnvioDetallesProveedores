using System.Net.Mail;

namespace Helpers
{
    public class EmailService
    {
        private readonly EmailFactory _emailFactory;

        public EmailService(EmailFactory emailFactory)
        {
            _emailFactory = emailFactory;
        }

        public async Task SendEmailAsync(string subject, string body, List<string> destinatarios)
        {
            using var client = _emailFactory.CreateSmtpClient();
            var message = new MailMessage
            {
                From = new MailAddress(client.Credentials.GetCredential(client.Host, client.Port, "").UserName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var destinatario in destinatarios)
            {
                message.To.Add(destinatario);
            }

            await client.SendMailAsync(message);
        }
    }
}
