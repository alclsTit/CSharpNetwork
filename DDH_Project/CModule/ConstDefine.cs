using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//LogFolder 네이밍
namespace ConstModule
{   
    public static class ConstLogsFolder
    {
        public const string gLogHandlerFolder = "LogicHandler";
    }

    //LogFile 네이밍 
    public static class ConstLogsFile
    {
        public const string gLogHandlerFile = "Logiclogs";
    }

    public static class ConstDefine
    {
        // (2,147,483,647 / 1000 * 2 * 1024) = 1048
        public static readonly int MAX_ACCEPT_OPT = 10;
        public static readonly int MAX_CONNECTION = 1000;
        public const int MAX_PACKET_HEADER_SIZE = 4;
        public const int MAX_PACKET_TYPE_SIZE = 4;
        public const int MAX_BUFFER_SIZE = 1024;

        public static readonly string DateFormatYMDHMS = "yyyy-MM-dd hh:mm:ss";
        public static readonly string DateFormatYMD = "yyyy-MM-dd";

        public static readonly string DateFormatHMS = "HH:mm:ss.F";
        public static readonly string DateFormatHMSf = "HH:mm:ss.f";
        public static readonly string DateFormatHMSff = "HH:mm:ss.ff";
        public static readonly string DateFormatHMSfff = "HH:mm:ss.fff";

    }

    public static class GSocketState
    {
        public enum eSocketState : short
        {
            DISCONNECTED = 0,
            CONNECTED = 1,
            CONNECTING = 2,
            DISABLED = 3
        }

        public enum eSocketType
        {
            SEND = 1,
            RECV = 2
        }

        public enum eSockEvtType
        {
            CONCURRENT,
            NOCONCURRENT
        }

        public static readonly int MAX_SOCKET_HEARTBEAT_GAUGE = 6;              // 6회
        public static readonly double MAX_HEARTBEAT_INTERVAL = 10000;           // 10초
        public static readonly double MAX_CHECK_CONNECTION_INTERVAL = 50;       // 0.5초
        public static readonly double MAX_RECONNECT_INTERVAL = 20000;           // 20초

    }

    public static class GResult
    {
        public enum eErrCode : int
        {
            NORMAL = 0,
            SOCKET_ALLOC_ERROR = 1,
            SOCKET_CONNECT_ERROR = 2,
            SOCKET_SEND_EVENT_ARGS_ALLOC_ERROR_Q = 3,
            SOCKET_SEND_EVENT_ARGS_ALLOC_ERROR = 4,
            SOCKET_RECV_EVENT_ARGS_ALLOC_ERROR_Q = 5,
            SOCKET_RECV_EVENT_ARGS_ALLOC_ERROR = 6,
            BUFFER_NO_DATA = 7
        }
    }

}
