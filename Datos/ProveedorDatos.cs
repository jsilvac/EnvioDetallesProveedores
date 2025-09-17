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
                    proveedores.Add(fillProveedor(row));
                }
            }
            return proveedores;
        }
        private Dto.ProveedorDTO fillProveedor(System.Data.DataRow row)
        {
            var proveedor = new Dto.ProveedorDTO();
            proveedor.Rut_Proveedor = row["rut_proveedor"] != DBNull.Value ? Convert.ToString(row["rut_proveedor"]) : string.Empty;
            proveedor.N_Proveedor = row["n_proveedor"] != DBNull.Value ? Convert.ToString(row["n_proveedor"]) : string.Empty;
            proveedor.Banco = row["banco"] != DBNull.Value ? Convert.ToString(row["banco"]) : string.Empty;
            proveedor.CtaCte_Proveedor = row["ctacte_proveedor"] != DBNull.Value ? Convert.ToString(row["ctacte_proveedor"]) : string.Empty;
            proveedor.TipoPago = row["tipo_pago"] != DBNull.Value ? Convert.ToString(row["tipo_pago"]) : string.Empty;
            return proveedor;
        }
    }
}
