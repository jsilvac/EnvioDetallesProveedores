using Datos;
using Dto;
using Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ManejoComprobantesDatos
    {
        private readonly CrudManager _crud;
        private IConexion _icon;

        public ManejoComprobantesDatos(IConexion icon)
        {
            _icon = icon;
            _crud = new CrudManager(icon);
        }

        public string ListarComprobantes()
        {

            string sql = "SELECT * FROM eltit_conta.conta_comprobante_cabeza WHERE estadoenvio=0";

            var dt = _crud.ExecuteConsulta(sql);

            var sb = new StringBuilder();

            if (dt?.Rows.Count > 0) { 
                foreach (DataRow row in dt.Rows)
                {
                    sb.AppendLine($"{row["folio"]} - {row["n_proveedor"]}");
                }
            }

            return sb.ToString();
        }
        public List<comprobantesDTO> ListarComprobantesDTO()
        {
            string sql = "SELECT folio, n_proveedor, fecha, estadoenvio " +
                         "FROM eltit_conta.conta_comprobante_cabeza " +
                         "WHERE estadoenvio = 0";

            var dt = _crud.ExecuteConsulta(sql);
            var comprobantes = new List<comprobantesDTO>();

            if (dt != null)
            {
                
                foreach (DataRow row in dt.Rows)
                { 
                    comprobantes.Add(fillComprobantes(row));
                    
                }

            }

            return comprobantes;
        }

        private comprobantesDTO fillComprobantes(DataRow row)
        {
           return new comprobantesDTO
            {
                Numero = row["folio"]?.ToString(),
                Proveedor = row["n_proveedor"]?.ToString(),
                Fecha = row["fecha"] != DBNull.Value ? Convert.ToDateTime(row["fecha"]) : null,
                EstadoEnvio = Convert.ToInt32(row["estadoenvio"])
            };
        }

        public bool ActualizaEstadoEnvio(string estadoEnvio, int numeroComprobante)
        {
            string sql = "UPDATE eltit_conta.conta_comprobante_cabeza " +
                         "SET estadoenvio = @estado " +
                         "WHERE numero = @numero";
            var param = new Dictionary<string, object>()
            {
                {"@numero", numeroComprobante },
                {"@estado", estadoEnvio},
            };

            return _crud.ExecuteCommand(sql, param) > 0;

        }

    }
}
    