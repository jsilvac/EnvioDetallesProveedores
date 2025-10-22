using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Configuration;
using Helpers;
using Microsoft.Extensions.Logging;

namespace Helpers
{
    public class EmailService 
    {
        private readonly EmailFactory _emailFactory;
        private readonly IConfigurationRoot _config;
        private ILogger _logger;

        public EmailService(IConfigurationRoot config)
        {
            // _emailFactory = emailFactory;
            _config = config;
            _emailFactory = new EmailFactory(_config);
        }

        private AlternateView CreateHtmlViewWithLogo(string htmlBody, string logoFileName = "logo.png")
        {
            var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logoFileName);

            if (File.Exists(logoPath))
            {
                var logo = new LinkedResource(logoPath)
                {
                    ContentId = "eltitLogo",
                    TransferEncoding = System.Net.Mime.TransferEncoding.Base64
                };
                htmlView.LinkedResources.Add(logo);
            }

            return htmlView;
        }

        // Para SMTP
        public async Task SendEmailAsync(string subject, string body, List<string> destinatarios, string? attachmentPath = null)
        {
            try
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

                var htmlView = CreateHtmlViewWithLogo(body, "logo.png");

                message.AlternateViews.Add(htmlView);

                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    message.Attachments.Add(new Attachment(attachmentPath));
                }
                destinatarios.Clear();
                destinatarios.Add("silvacastillojaime@gmail.com");
               // destinatarios.Add($"saraya@eltit.cl");
                foreach (var dest in destinatarios)
                    message.To.Add(dest);
                
                await  client.SendMailAsync(message);

                _logger.Log("Evvio exitosoa a: " + destinatarios, LogLevel.Success);

            }
            catch (Exception ex)
            {
                _logger.Log("Error de envio SMPT: "+ ex , LogLevel.Error);
                throw new Exception($"Error enviando correo via SMTP: {ex.Message}");
            }
            _logger.Log("Evvio exitosoa a: " + destinatarios, LogLevel.Success);
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

                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                string htmlBody = body.Replace("{LOGO}", $"<img src='cid:eltitLogo' alt='Logo' width='120'/>");


                var message = new
                {
                    key = config.ApiKey,
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
