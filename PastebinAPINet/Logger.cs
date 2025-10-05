namespace PastebinAPINet
{
    public class Logger
    {
        public enum LogType
        {
            Log,
            Warning,
            Error,
        }

        public delegate void OnLogHandler(LogType logType, string message);

        public event OnLogHandler? OnLog;

        public void Log(string message) => OnLog?.Invoke(LogType.Log, message);

        public void Warning(string message) => OnLog?.Invoke(LogType.Warning, message);

        public void Error(string message) => OnLog?.Invoke(LogType.Error, message);
    }
}
