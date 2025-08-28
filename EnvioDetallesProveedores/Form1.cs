
using Negocio;
using Repository;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;




namespace EnvioDetallesProveedores
{
    public partial class Form1 : Form
    {
        private IConfigurationRoot Config;
        public Form1(IConfigurationRoot ConfigInit)
        {
            InitializeComponent();
            Config = ConfigInit;
        }


        private void button1_Click(object sender, EventArgs e)
        {

            
            
        }

        private void timerInicio_Tick(object sender, EventArgs e)
        {
            try
            {
                string tipoConexionStr = Config["TipoConexion"];

                if (!Enum.TryParse<TipoConexion>(tipoConexionStr, out var tipoConexion))
                {
                    throw new ArgumentException($"Tipo de conexión '{tipoConexionStr}' no es válido.");
                }

                IConexion cnn = FabricaConexion.CrearConexion(tipoConexion);

                cnn.setConnectionString(Config.GetConnectionString(tipoConexionStr));

                var manejoComprobantes = new ProcesaComprobantesNegocio(cnn);
                //var comprobantes = manejoComprobantes.obtenerComprobnatesPendientes();
                var comprobantes = manejoComprobantes.ObtenerComprobantesComoString();
                MessageBox.Show(comprobantes);


                //if (comprobantes.Count > 0)
                //{

                //    DG_MuestraComprobantes.DataSource = comprobantes;

                //    //foreach (var comprobante in comprobantes)
                //    //{
                //    //    Console.WriteLine($"Enviando comprobante: {comprobante.Numero} - {comprobante.Proveedor}");
                //    //    actualizamos
                //    //    manejoComprobantes.ActualizaEstadoEnvio("1", Convert.ToInt32(comprobante.Numero));


                //    //}

                timerInicio.Stop();
                //}
                //else
                //{
                //    Console.WriteLine("No hay comprobantes pendientes de envío.");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar comprobantes: {ex.Message}");
            }
            finally
            {
               

            }
        }
    }
}
