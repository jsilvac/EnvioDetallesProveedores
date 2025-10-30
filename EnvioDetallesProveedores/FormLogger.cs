using System;
using System.Drawing;
using System.Windows.Forms;
using Helpers;

public class FormLogger : ILogger
{
    private readonly RichTextBox _output;

    public FormLogger(RichTextBox output)
    {
        _output = output;
    }

    public void Log(string message, LogLevel level)
    {
        string prefix;
        Color color;

        switch (level)
        {
            case LogLevel.Info:
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
            case LogLevel.Warning:
                prefix = "✅ ";
                color = Color.BlueViolet;
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
        try
        {
            // Siempre log a consola para debugging
            Console.WriteLine($"[FORMLOG] {message}");

            // Si el RichTextBox no está disponible, solo usa consola
            if (_output == null || _output.IsDisposed || !_output.IsHandleCreated)
            {
                Console.WriteLine($"[FORMLOG-SKIP] Control no disponible: {message}");
                return;
            }

            // Usar Invoke para thread safety
            if (_output.InvokeRequired)
            {
                _output.Invoke(new Action(() => AppendLogToControl(message, color)));
            }
            else
            {
                AppendLogToControl(message, color);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FORMLOG-ERROR] {message} - Error: {ex.Message}");
        }
    }

    private void AppendLogToControl(string message, Color color)
    {
        try
        {
            // Verificar nuevamente dentro del contexto del UI thread
            if (_output.IsDisposed || !_output.IsHandleCreated)
                return;

            _output.SelectionStart = _output.TextLength;
            _output.SelectionLength = 0;
            _output.SelectionColor = color;
            _output.AppendText(message + Environment.NewLine);
            _output.SelectionColor = _output.ForeColor;
            _output.ScrollToCaret();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[APPEND-ERROR] {ex.Message}");
        }
    }

    private string FormatearMensaje(string mensaje)
    {
        return $" [{DateTime.Now:HH:mm:ss}] {mensaje}";
    }
}