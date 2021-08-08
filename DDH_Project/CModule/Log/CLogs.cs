using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CModule
{
    /*
     * 전역으로 사용할 수 있어야 한다?
     */

    // Log4Net
    public static class CLog4Net
    {
        public static readonly ILog gLog4Net = LogManager.GetLogger(typeof(CLog4Net));

        // 패킷 흐름을 알기위해 디버그 모드에서만 출력
        // 형식 [FLOW] (클래스이름.호출메서드이름) (메시지내용) 
        public static void LogDebugSysLog(string HeadMsg, string BodyMsg)
        {
        #if DEBUG
            gLog4Net.InfoFormat($"[FLOW] {HeadMsg} - {BodyMsg}");
        #endif
        }

        public static void LogError(string message)
        {
            gLog4Net.ErrorFormat(message);
        }
    }

    public class CLogger
    {
        public readonly ILog mLog;

        public void init()
        {

        }

        public void SetLogEnvironment()
        {
            var start_time = System.Diagnostics.Process.GetCurrentProcess().StartTime;

        }
    }


    // CustomLog - Contents Log
    internal sealed class CLogs
    {
        private CFileIOSystemManager m_fileManager;

        public CLogs(string _folderName, string _fileName)
        {
            m_fileManager = new CFileIOSystemManager(_folderName, _fileName);
        }

        public async void TestLog(int _testLogId)
        {
            var msg = _testLogId.ToString();
            await m_fileManager.WriteAsync(msg); 
        }

    }
}
