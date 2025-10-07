using Repository;
using System.Drawing;
using System.Windows.Forms;

public class FormLogger : ILogger
{
    private readonly RichTextBox _output;

    public FormLogger(RichTextBox output)
    {
        _output = output;
    }

    public void Log(string message, LogLevel level )
    {
        string prefix;
        Color color;

        switch (level)
        {
            case LogLevel.Warning:
                prefix = "⚠️ ";
                color = Color.LightGray;
                break;
            case LogLevel.Error:
                prefix = "❌ ";
                color = Color.Red;
                break;
            case LogLevel.Success:
                prefix = "✅ ";
                color = Color.Green;
                break;
            default:
                prefix = "ℹ️ ";
                color = Color.Orange;
                break;
        }

        AppendLog(prefix + FormatearMensaje(message), color);
    }

    private void AppendLog(string message, Color color)
    {
        _output.Invoke((MethodInvoker)delegate
        {
            _output.SelectionStart = _output.TextLength;
            _output.SelectionLength = 0;

            _output.SelectionColor = color;
            _output.AppendText(message + Environment.NewLine);

            _output.SelectionColor = _output.ForeColor;
        });
    }


    private string FormatearMensaje(string mensaje)
    {
        return $" [{DateTime.Now:HH:mm:ss}] {mensaje}";
    }
}
