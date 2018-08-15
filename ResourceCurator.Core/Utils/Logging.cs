using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ResourceCurator
{
    public static class Logging
    {
        private static ILoggerFactory _loggerFactory = new NullLoggerFactory();

        /// <summary>
        /// Setup logger factory
        /// </summary>
        /// <param name="loggerFactory"></param>
        public static void SetLoggerFactory(ILoggerFactory loggerFactory) => System.Threading.Interlocked.Exchange(ref _loggerFactory, loggerFactory);

        /// <summary>
        /// Creates logger with category
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static ILogger CreateLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);

        /// <summary>
        /// Creates logger with class name category
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger CreateLogger<T>() => _loggerFactory.CreateLogger<T>();
    }

}
