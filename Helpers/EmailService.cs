using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;

namespace Helpers
{
    public class EmailService
    {
        private readonly EmailFactory _emailFactory;

        public EmailService(EmailFactory emailFactory)
        {
            _emailFactory = emailFactory;
        }

        // Para SMTP
        public async Task SendEmailAsync(string subject, string body, List<string> destinatarios, string? attachmentPath = null)
        {
            using var client = _emailFactory.CreateSmtpClient();

            if (client == null)
                throw new InvalidOperationException("El proveedor configurado no usa SMTP. Use SendEmailViaApiAsync en su lugar.");

            var from = ((NetworkCredential)client.Credentials).UserName;
            var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                message.Attachments.Add(new Attachment(attachmentPath));
            }

            foreach (var dest in destinatarios)
                message.To.Add(dest);

            await client.SendMailAsync(message);
        }

        // Para Mailchimp (API)
        public async Task SendEmailViaApiAsync(string subject, string body, List<string> destinatarios, string? attachmentPath = null)
        {
            var config = _emailFactory.GetMailchimpConfig();
            if (config == null)
                throw new InvalidOperationException("Configuración de Mailchimp no encontrada.");

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri($"https://{config.ServerPrefix}.api.mailchimp.com/3.0/");
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{config.ApiKey}")));

            foreach (var dest in destinatarios)
            {
                var payload = new
                {
                    email_address = dest,
                    status = "subscribed", // o "pending"
                    merge_fields = new { FNAME = "", LNAME = "" }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"lists/{config.ListId}/members", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error enviando correo a {dest}: {error}");
                }
            }
        }
        // Para Mailchimp Transactional (Mandrill)
        public async Task SendEmailViaMailchimpTransactionalAsync(string subject, string body, List<string> destinatarios, string? attachmentPath = null)
        {
            var config = _emailFactory.GetMailchimpConfig();
            if (config == null)
                throw new InvalidOperationException("Configuración de Mailchimp Transactional no encontrada.");

            byte[]? fileBytes = null;
            string? base64File = null;

            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                fileBytes = File.ReadAllBytes(attachmentPath);
                base64File = Convert.ToBase64String(fileBytes);
            }

            var message = new
            {
                key = config.ApiKey, // API Key de Mailchimp Transactional
                message = new
                {
                    from_email = config.From,
                    subject = subject,
                    html = body,
                    to = destinatarios.Select(d => new { email = d, type = "to" }).ToArray(),
                    attachments = string.IsNullOrEmpty(base64File) ? null : new[]
                    {
                    new {
                        type = "application/pdf",
                        name = Path.GetFileName(attachmentPath),
                        content = base64File
                        }
                    }
                }
            };

            using var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://mandrillapp.com/api/1.0/messages/send.json", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error enviando correo via Mailchimp Transactional: {response.StatusCode}, {responseString}");

            Console.WriteLine("Correo enviado correctamente: " + responseString);
        }

    }
}
