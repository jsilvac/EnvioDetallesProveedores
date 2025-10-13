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
            _crud = new CrudManager(_icon);
        }

        public List<ComprobantesDTO> ListarComprobantes()
        {
            string sql = "SELECT * FROM eltit_conta.conta_comprobante_cabeza WHERE estadoenvio=0";

            var dt = _crud.ExecuteConsulta(sql);

            var comprobantes = new List<ComprobantesDTO>();
            var sb = new StringBuilder();

            if (dt != null)
            {

                foreach (DataRow row in dt.Rows)
                {
                    comprobantes.Add(fillComprobantes(row));
                }
            }

            return comprobantes;
        }

        public List<ComprobantesDTO> ListarComprobantesDTO()
        {
            string sql = """
                SELECT 
                  cb.folio,
                  cb.fecha,
                  cb.rut_proveedor,
                  cb.n_proveedor,
                  cb.correos,
                  dt.numero,
                  cb.banco,
                  cb.ctacte_proveedor,
                  cb.total,
                  cb.num_docus,
                  cb.estadoenvio,
                  cb.rut_empresa,
                  cb.empresa,
                  cb.mensaje,
                  dt.egreso,
                  dt.proveedor,
                  dt.glosa,
                  dt.td,
                  dt.monto,
                  cb.numero AS numeros
                FROM
                  eltit_conta.conta_comprobante_cabeza AS cb 
                  INNER JOIN eltit_conta.conta_comprobante_detalle AS dt 
                    ON dt.folio = cb.folio 
                    AND dt.fecha = cb.fecha 
                    AND dt.proveedor = cb.rut_proveedor 
                WHERE cb.estadoenvio = '0' 
                
                """;

            var dt = _crud.ExecuteConsulta(sql);
            var comprobantes = new List<ComprobantesDTO>();

            if (dt != null)
            {
                
                foreach (DataRow row in dt.Rows)
                { 
                    comprobantes.Add(fillComprobantes(row));              
                }
            }
            return comprobantes;
        }

        private ComprobantesDTO fillComprobantes(DataRow? row)
        {
            return new ComprobantesDTO
            {
                Folio = row["folio"]?.ToString(),
                Fecha = row["fecha"] != DBNull.Value ? Convert.ToDateTime(row["fecha"]) : null,
                Rut_Proveedor = row["rut_proveedor"]?.ToString(),
                N_Proveedor = row["n_proveedor"]?.ToString(),
                Correos = row["correos"]?.ToString()
                                          .Split(';', StringSplitOptions.RemoveEmptyEntries) ?  // Separa en partes
                                         .Select(correo => correo.Trim()) ?                    // Recorre cada elemento y lo procesa
                                         .ToList()?? new List<string>(),
                Numero = row["numero"]?.ToString(),
                Banco = row["banco"]?.ToString(),
                CtaCte_Proveedor = row["ctacte_proveedor"]?.ToString(),
                Total = Convert.ToDouble(row["total"]?.ToString()),
                Num_Docus = row["num_docus"]?.ToString(),
                EstadoEnvio = Convert.ToInt32(row["estadoenvio"]),
                Rut_Empresa = row["rut_empresa"]?.ToString(),
                Empresa = row["empresa"]?.ToString(),
                Mensaje = row["mensaje"]?.ToString(),
                Egreso = row["egreso"]?.ToString(),
                Proveedor = row["proveedor"]?.ToString(),
                Glosa = row["glosa"]?.ToString(),
                Td = row["td"]?.ToString(),
                Monto = Convert.ToDouble(row["monto"]?.ToString()),
                Numeros = row["numeros"]?.ToString()
                                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)  // separa por espacios
                                           .ToList(),
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
    