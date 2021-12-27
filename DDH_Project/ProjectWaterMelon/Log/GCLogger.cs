using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ProjectWaterMelon.Log
{
    // 20211103 GetLogger 로거 세팅필요
    // Todo: 새로운 로거 클래스, 이후 해당 로거 클래스로 로그 변경예정
    // ALL -> Debug -> Info -> Warn -> Error -> Fatal
    public static class GCLogger
    {
        private static readonly ILog mLogger = LogManager.GetLogger(typeof(GCLogger));

        /// <summary>
        /// Debug Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        public static void Debug(in string class_name, in string method_name, in string message = "")
        {
            mLogger.Debug($"Debug in {class_name}.{method_name} - {message}");
        }

        /// <summary>
        /// Debug Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Debug(in string class_name, in string method_name, in Exception ex, in string message = "")
        {
            mLogger.Debug($"Debug in {class_name}.{method_name} - {message} - {ex.StackTrace}", ex);
        }

        /// <summary>
        /// Info Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        public static void Info(in string class_name, in string method_name, in string message = "")
        {
            mLogger.Info($"Info in {class_name}.{method_name} - {message}");
        }

        /// <summary>
        /// Info Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Info(in string class_name, in string method_name, in Exception ex, in string message = "")
        {
            mLogger.Info($"Info in {class_name}.{method_name} - {message} - {ex.StackTrace}", ex);
        }

        /// <summary>
        /// Just can view Compile Debug mode
        /// Info Level Log for Server flow
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        public static void LogDebugMode(in string class_name, in string method_name, in string message = "")
        {
#if DEBUG
            mLogger.Info($"[ServerFlow] {class_name}.{method_name} - {message}");
#endif
        }

        /// <summary>
        /// Warn Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        public static void Warn(in string class_name, in string method_name, in string message = "")
        {
            mLogger.Warn($"Warn in {class_name}.{method_name} - {message}");
        }

        /// <summary>
        /// Warn Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Warn(in string class_name, in string method_name, in Exception ex, in string message = "")
        {
            mLogger.Warn($"Warn in {class_name}.{method_name} - {message} - {ex.StackTrace}", ex);
        }

        /// <summary>
        /// Error Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        public static void Error(in string class_name, in string method_name, in string message = "")
        {
            mLogger.Error($"Exception in {class_name}.{method_name} - {message}");
        }

        /// <summary>
        /// Error Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Error(in string class_name, in string method_name, in Exception ex, in string message = "")
        {
            mLogger.Error($"Exception in {class_name}.{method_name} - {message} - {ex.StackTrace}", ex);
        }

        /// <summary>
        /// Fatal Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        public static void Fatal(in string class_name, in string method_name, in string message = "")
        {
            mLogger.Fatal($"Fatal in {class_name}.{method_name} - {message}");
        }

        /// <summary>
        /// Fatal Level Log
        /// </summary>
        /// <param name="class_name"></param>
        /// <param name="method_name"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Fatal(in string class_name, in string method_name, in Exception ex, in string message = "")
        {
            mLogger.Fatal($"Fatal in {class_name}.{method_name} - {message} - {ex.StackTrace}", ex);
        }

    }
}
