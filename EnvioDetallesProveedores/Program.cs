using Helpers;
using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace EnvioDetallesProveedores
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            IConfigurationRoot Config = ConfiguraInicio.ConfiguraInicioApp();

            Application.Run(new Form1(Config ));
        }
    }
}