using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon
{
    /*
     * 전역으로 사용할 수 있어야 한다?
     */

    // Log4Net
    public static class CLog4Net
    {
        public static readonly ILog gLog4Net = LogManager.GetLogger(typeof(CLog4Net));

        // 패킷 흐름을 알기위해 디버그 모드에서만 출력 
        public static void LogDebugSysLog(string HeadMsg, string BodyMsg)
        {
        #if DEBUG
             gLog4Net.InfoFormat($"[FLOW] {HeadMsg} - {BodyMsg}");
        #endif
        }

        public static void LogError(string message)
        {
            gLog4Net.ErrorFormat($"{message}");
        }
    }



}
