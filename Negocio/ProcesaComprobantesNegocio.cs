using Datos;
using Dto;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ProcesaComprobantesNegocio
    {
        private readonly ManejoComprobantesDatos _comprobanteData;
        private IConexion _icon;
        public ProcesaComprobantesNegocio(IConexion icon)
        {   
            _icon = icon;
        }


        public string procesaComprobantes()
        {
            string listaC = ObtenerComprobantesCabezaList();




            return "Proceso finalizado";
        }

        public string ObtenerComprobantesCabezaList()
        {
            ManejoComprobantesDatos _comprobanteData = new ManejoComprobantesDatos(_icon);
            return _comprobanteData.ListarComprobantes();
        }

        public string ObtenerComprobantesDetalle()
        {
            ManejoComprobantesDatos _comprobanteData = new ManejoComprobantesDatos(_icon);
            return _comprobanteData.ListarComprobantes();
        }

        public List<comprobantesDTO> obtenerComprobnatesPendientes()
        {
            try
            {
                ManejoComprobantesDatos _comprobanteData = new ManejoComprobantesDatos(_icon);

                var comprobantes = _comprobanteData.ListarComprobantesDTO();

                if (comprobantes.Count == 0)
                {
                    Console.WriteLine("No hay comprobantes");
                }
                return comprobantes;
            }
            catch (Exception ex) {
                Console.WriteLine("erro: ", ex);
                throw new ApplicationException(ObtenerComprobantesComoString(), ex);
            }
        }
    }
}
