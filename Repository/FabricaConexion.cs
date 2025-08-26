using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Repository
{
    public static class FabricaConexion
    {
      

        static FabricaConexion()
        {
            //Config = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json")
            //    .Build();
        }

        public static IConexion CrearConexion(TipoConexion tipoConexion)
        {
            //string tipoConexionStr = Config["TipoConexion"];

            //if (!Enum.TryParse<TipoConexion>(tipoConexionStr, out var tipoConexion))
            //{
            //    throw new ArgumentException($"Tipo de conexión '{tipoConexionStr}' no es válido.");
            //}

            switch (tipoConexion)
            {
                case TipoConexion.MySQL:
                    return new MySqlConexion();
                case TipoConexion.PostgresSQL:
                    return new PostgresConexion();
                case TipoConexion.ApiRest:
                    return new ApiRestConexion();
                default:
                    throw new ArgumentException("Tipo de conexión no soportado");
            }
        }
    }

}
