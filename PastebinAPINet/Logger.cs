namespace PastebinAPINet
{    /// <summary>
     /// Logging for the Pastebin client
     /// </summary>
    public class Logger
    {    
        /// <summary>
        /// Type of log message.
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// Informational message.
            /// </summary>
            Log,

            /// <summary>
            /// Potiental issue.
            /// </summary>
            Warning,

            /// <summary>
            /// Failure or exeception.
            /// </summary>
            Error,
        }

        /// <summary>
        /// Delegate for handling log messages.
        /// </summary>
        /// <param name="logType">Type of the log message.</param>
        /// <param name="message">Content of the log message.</param>
        public delegate void OnLogHandler(LogType logType, string message);

        /// <summary>
        /// Fired when a log is sent. 
        /// </summary>
        public event OnLogHandler? OnLog;

        /// <summary>
        /// Logs infomation.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Log(string message) => OnLog?.Invoke(LogType.Log, message);

        /// <summary>
        /// Logs potiental issue.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Warning(string message) => OnLog?.Invoke(LogType.Warning, message);

        /// <summary>
        /// Logs failure or exeception.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Error(string message) => OnLog?.Invoke(LogType.Error, message);
    }
}
