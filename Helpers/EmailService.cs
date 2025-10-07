using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Helpers
{
    public class EmailService
    {
        private readonly EmailFactory _emailFactory;
        private readonly IConfigurationRoot _config;

        public EmailService(IConfigurationRoot config)
        {
            // _emailFactory = emailFactory;
            _config = config;
            _emailFactory = new EmailFactory(_config);
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
            destinatarios = ["silvacastillojaime@gmail.com"];
            foreach (var dest in destinatarios)
                message.To.Add(dest);                                                                                                                                                                                                                                                                             

            await client.SendMailAsync(message);
        }

        // Para Mailchimp Transactional (Mandrill)
        public async Task SendEmailViaMailchimpTransactionalAsync(string subject, string body, List<string> destinatarios, string? attachmentPath = null)
        {
            try
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
                destinatarios = ["jsilv@eltit.cl"];
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
            catch (Exception ex)
            {
                throw new Exception($"Error enviando correo via Mailchimp Transactional: {ex.Message}");
            }

        }
    }
}
