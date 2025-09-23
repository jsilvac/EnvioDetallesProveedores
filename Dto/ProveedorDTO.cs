
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class ProveedorDTO
    {
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string ModoPago { get; set; }
        public string Banco { get; set; }
        public string Sucursal { get; set; }
        public string Cuentacorreinte { get; set; }
        public string RutRetira { get; set; }
        public string NombreRetira { get; set; }
        public string Email { get; set; }
        public string Plazo { get; set; }
        public ProveedorDTO() { }
    }
}
