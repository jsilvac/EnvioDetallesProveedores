using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ProveedorNegocio
    {
        private readonly Datos.ProveedorDatos _proveedorDatos;
        public ProveedorNegocio(Datos.ProveedorDatos proveedorDatos)
        {
            _proveedorDatos = proveedorDatos;
        }
        public List<Dto.ProveedorDTO> ListarProveedores()
        {
            return _proveedorDatos.ListarProveedores();
        }
        public Dto.ProveedorDTO ObtenerProveedorPorRut(string rut)
        {
            return _proveedorDatos.ObtenerProveedorPorRut(rut);
        }

    }
}
