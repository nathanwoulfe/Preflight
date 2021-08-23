#if NET472
using System;
using Umbraco.Core.Logging;

namespace Preflight.Logging
{
    public interface ILogger<T>
    {
        void LogError(Exception ex, string message, params object[] values);
        void LogDebug(string message, params object[] values);
        void LogInformation(string message, params object[] values);
        void LogWarning(string message, params object[] values);
    }

    public class Logger<TEntity> : ILogger<TEntity>
    {
        private readonly ILogger _logger;

        public Logger(ILogger logger) => _logger = logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public void LogDebug(string message, object[] values) =>        
            _logger.Debug(typeof(TEntity), message, values);
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public void LogWarning(string message, object[] values) =>        
            _logger.Warn(typeof(TEntity), message, values);
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public void LogError(Exception ex, string message, object[] values) =>
            _logger.Error(typeof(TEntity), ex, message, values);
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="values"></param>
        public void LogInformation(string message, object[] values) =>
            _logger.Info(typeof(TEntity), message, values);        
    }
}
#endif