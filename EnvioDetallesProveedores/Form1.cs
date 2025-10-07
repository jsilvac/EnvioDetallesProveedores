
using Negocio;
using Repository;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Configuration;


namespace EnvioDetallesProveedores
{
    public partial class Form1 : Form
    {
        private IConfigurationRoot Config;
        
        private readonly ILogger _logger;

        public Form1(IConfigurationRoot ConfigInit)
        {
            InitializeComponent();
            Config = ConfigInit;
            _logger = new FormLogger(txt_log);
        }


        private void button1_Click(object sender, EventArgs e)
        {

        }

        private string FormatearMensaje(string mensaje)
        {
             return $" [{DateTime.Now:HH:mm:ss}] {mensaje}";
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



                var manejoComprobantes = new ProcesaComprobantesNegocio(cnn, Config, _logger);
                var comprobantes = manejoComprobantes.procesaComprobantes();
                MessageBox.Show(comprobantes);


                timerInicio.Stop();
        
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
