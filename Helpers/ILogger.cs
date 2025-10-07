
namespace Repository
{
    public interface ILogger
    {
        void Log(string message, LogLevel level = LogLevel.Info);
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }
}
