using MySql.Data.MySqlClient;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class ProveedorDatos
    {
        private readonly CrudManager _crud;
        private IConexion _icon;
        public ProveedorDatos(IConexion icon)
        {
            _icon = icon;
            _crud = new CrudManager(_icon);
        }
        public List<Dto.ProveedorDTO> ListarProveedores()
        {
            string sql = "SELECT * FROM eltit_conta.conta_proveedor";
            var dt = _crud.ExecuteConsulta(sql);
            var proveedores = new List<Dto.ProveedorDTO>();
            var sb = new StringBuilder();
            if (dt != null)
            {
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    proveedores.Add(fillProveedores(row));
                }
            }
            return proveedores;
        }
        private Dto.ProveedorDTO fillProveedores(System.Data.DataRow row)
        {
            var proveedor = new Dto.ProveedorDTO();
            proveedor.Rut = row["rut"] != DBNull.Value ? Convert.ToString(row["rut"]) : string.Empty;
            proveedor.Nombre = row["nombre"] != DBNull.Value ? Convert.ToString(row["nombre"]) : string.Empty;
            proveedor.ModoPago = row["modopago"] != DBNull.Value ? Convert.ToString(row["modopago"]) : string.Empty;
            proveedor.Banco = row["banco"] != DBNull.Value ? Convert.ToString(row["banco"]) : string.Empty;
            proveedor.Sucursal = row["sucursal"] != DBNull.Value ? Convert.ToString(row["sucursal"]) : string.Empty;
            proveedor.Cuentacorreinte = row["cuentacorriente"] != DBNull.Value ? Convert.ToString(row["cuentacorriente"]) : string.Empty;
            proveedor.RutRetira = row["rutretira"] != DBNull.Value ? Convert.ToString(row["rutretira"]) : string.Empty;
            proveedor.NombreRetira = row["nombreretira"] != DBNull.Value ? Convert.ToString(row["nombreretira"]) : string.Empty;
            proveedor.Email = row["email"] != DBNull.Value ? Convert.ToString(row["email"]) : string.Empty;
            proveedor.Plazo = row["plazo"] != DBNull.Value ? Convert.ToString(row["plazo"]) : string.Empty;

            return proveedor;
        }

        public Dto.ProveedorDTO ObtenerProveedorPorRut(string rut)
        {
            rut = rut.Replace("-", "");
            string sql = "SELECT * FROM eltit_conta.cuentascorrientes_datos_pago WHERE rut = @rut";

            var parametros = new Dictionary<string, object>
                    {
                        { "@rut", rut }
                    };


            var dt = _crud.ExecuteConsulta(sql,parametros);
            Dto.ProveedorDTO proveedor = null;

            if (dt != null && dt.Rows.Count > 0)
            {
                // Tomamos solo la primera fila encontrada
                proveedor = fillProveedores(dt.Rows[0]);
            }

            return proveedor;

        }
    }
}
