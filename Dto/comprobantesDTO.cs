using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class ComprobantesDTO
    {
        //cabeza
        public string Folio { get; set; }
        public string Rut_Proveedor { get; set; }
        public string N_Proveedor { get; set; }
        public string Numero { get; set; }
        public string Banco { get; set; }
        public string CtaCte_Proveedor { get; set; }
        public DateTime? Fecha { get; set; }
        public List<string> Correos { get; set; }
        public double Total { get; set; }
        public string Num_Docus { get; set; }

        public int EstadoEnvio { get; set; }
        public string Rut_Empresa { get; set; }
        public string Empresa { get; set; }
        public string Mensaje { get; set; }

        // detalles
        public string Egreso { get; set; }
        public string Proveedor { get; set; }
        public string Glosa { get; set; }
        public string Td { get; set; }
        public double Monto { get; set; }

        public List<string> Numeros { get; set; }


        public ComprobantesDTO() { }
    }
}

