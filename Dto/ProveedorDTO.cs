
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class ProveedorDTO
    {
        public string Rut_Proveedor { get; set; }
        public string N_Proveedor { get; set; }

        public string Banco { get; set; }
        public string CtaCte_Proveedor { get; set; }
        public string TipoPago { get; set; }
        public ProveedorDTO() { }
    }
}
