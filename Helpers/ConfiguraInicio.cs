using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.Binder;


namespace Helpers
{
    public static class ConfiguraInicio
    {
        public static IConfigurationRoot ConfiguraInicioApp()
        {
            return new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();
        }

        public static string GetDefaultEmailProvider()
        {
            return ConfiguraInicioApp()["EmailSettings:DefaultProvider"] ?? "Gmail";
        }

        public static EmailProviderConfig GetEmailProvider(string? providerName = null)
        {
            var config = ConfiguraInicioApp();
            providerName ??= GetDefaultEmailProvider();

            var section = config.GetSection($"EmailSettings:Providers:{providerName}");
            // Ensure the Microsoft.Extensions.Configuration.Binder package is installed for the Get<T>() method
            return section.Get<EmailProviderConfig>() ?? new EmailProviderConfig();
        }
    }
    public class EmailProviderConfig
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }

}
