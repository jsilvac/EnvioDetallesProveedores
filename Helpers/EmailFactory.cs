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

        public SmtpClient CreateSmtpClient()
        {
            string tipoCorreo = _configuration["TipoCorreo"] ?? "Gmail";
            var correoConfig = _configuration.GetSection($"Correo:{tipoCorreo}");

            var client = new SmtpClient
            {
                Host = correoConfig["Host"],
                Port = int.Parse(correoConfig["Port"]),
                Credentials = new NetworkCredential(correoConfig["User"], correoConfig["Password"]),
                EnableSsl = bool.Parse(correoConfig["EnableSsl"] ?? "true")
            };

            return client;
        }
    }
}
