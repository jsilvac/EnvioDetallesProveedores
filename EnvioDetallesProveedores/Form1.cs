
using Negocio;
using Repository;
using Microsoft.Extensions.Configuration;
using Helpers;


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
            // Detenemos temporalmente el temporizador
            timerInicio.Stop();

            // Obtener el intervalo desde la configuración y convertirlo a milisegundos
            // NOTA: Asegúrate de que esta línea se ejecute correctamente al inicio de tu app.
            int intervaloMinutos = Config.GetValue<int>("JobSettings:IntervaloMinutos");
            int intervaloEspera = intervaloMinutos * 60 * 1000; // ¡Usamos el valor configurado!

            try
            {
                string tipoConexionStr = Config["TipoConexion"];

                if (!Enum.TryParse<TipoConexion>(tipoConexionStr, out var tipoConexion))
                {
                    throw new ArgumentException($"Tipo de conexión '{tipoConexionStr}' no es válido.");
                }

                IConexion cnn = FabricaConexion.CrearConexion(tipoConexion);
                cnn.setConnectionString(Config.GetConnectionString(tipoConexionStr));

                _logger.Log("Iniciando proceso de envío de comprobantes...", LogLevel.Success);

                var manejoComprobantes = new ProcesaComprobantesNegocio(cnn, Config, _logger);
                var comprobantes = manejoComprobantes.procesaComprobantes();

                _logger.Log(FormatearMensaje("Proceso de envío de comprobantes finalizado."), LogLevel.Success);
            }
            catch (Exception ex)
            {
                _logger.Log($"Error crítico: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                MessageBox.Show($"Error al enviar comprobantes: {ex.Message}");
            }
            finally
            {
                // Reconfiguración y reinicio para el siguiente ciclo
                timerInicio.Interval = intervaloEspera; // Establece el nuevo intervalo
                timerInicio.Start();

                _logger.Log($"Esperando {intervaloMinutos} minutos para el próximo ciclo...", LogLevel.Info);
            }
        }
    }
}
