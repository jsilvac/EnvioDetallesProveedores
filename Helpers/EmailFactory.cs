using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace Helpers
{
    public class EmailFactory
    {
        private readonly IConfiguration _configuration;

        public EmailFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SmtpClient? CreateSmtpClient()
        {
            string provider = _configuration["EmailSettings:DefaultProvider"] ?? "Gmail";

            // Si el proveedor es Mailchimp Transactional (o Mailchimp en general), no usamos SMTP
            if (provider.StartsWith("MailchimpTransactional", StringComparison.OrdinalIgnoreCase))
                return null;

            var correoConfig = _configuration.GetSection($"EmailSettings:Providers:{provider}");

            return new SmtpClient
            {
                Host = correoConfig["Host"],
                Port = int.Parse(correoConfig["Port"]),
                Credentials = new NetworkCredential(correoConfig["UserName"], correoConfig["Password"]),
                EnableSsl = bool.Parse(correoConfig["EnableSsl"] ?? "true")
            };
        }

        public MailchimpConfig? GetMailchimpConfig()
        {
             string provider = _configuration["EmailSettings:DefaultProvider"] ?? "MailchimpTransactional";

            var section = _configuration.GetSection($"EmailSettings:Providers:{provider}");
            return section.Exists() ? section.Get<MailchimpConfig>() : null;
        }

    }


}
