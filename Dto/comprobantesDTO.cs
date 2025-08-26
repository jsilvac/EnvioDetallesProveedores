using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dto
{
    public class comprobantesDTO
    {
        public string Numero { get; set; }
        public string Proveedor { get; set; }
        public DateTime? Fecha { get; set; }
        public int EstadoEnvio { get; set; }

        public comprobantesDTO() { }
    }
}

