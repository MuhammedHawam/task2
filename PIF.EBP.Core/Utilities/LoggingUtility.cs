using Castle.Core.Logging;
using PIF.EBP.Core.DependencyInjection;

namespace PIF.EBP.Core.Utilities
{
    public class LoggingUtility : ILoggingUtility
    {
        private readonly ILogger _logger;

        public LoggingUtility(ILogger logger)
        {
            _logger = logger;
        }

        public void LogError(string textToLog)
        {
            _logger.Error(textToLog);
        }

        public void LogInfo(string textToLog)
        {
            _logger.Info(textToLog);
        }

        public void LogWarning(string textToLog)
        {
            _logger.Warn(textToLog);
        }

        public void LogCustom(string textToLog)
        {
            _logger.Warn("----------------------------------custom----------------------------------");
            _logger.Info(textToLog);
            _logger.Warn("----------------------------------custom----------------------------------");
        }
    }

    public interface ILoggingUtility : ITransientDependency
    {
        void LogInfo(string textToLog);
        void LogWarning(string textToLog);
        void LogError(string textToLog);
        void LogCustom(string textToLog);
    }
}
