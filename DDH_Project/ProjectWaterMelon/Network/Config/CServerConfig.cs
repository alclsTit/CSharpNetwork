using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.Config
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CServerConfig : IServerConfig
    {
        // Set Default Value
        //-------------------------------------------------------------------------

        /// <summary>
        /// Default max accept count
        /// </summary>
        public readonly int DefaultMaxAcceptCount = 100;

        /// <summary>
        /// Default max connect count
        /// </summary>
        public readonly int DefaultMaxConnectCount = 100;

        /// <summary>
        /// Default SendBufferSize
        /// </summary>
        public const int DefaultSendBufferSize = 1024 * 4;

        /// <summary>
        /// Default RecvBufferSize
        /// </summary>
        public const int DefaultRecvBufferSize = 1024 * 4;

        /// <summary>
        /// Default KeepAlivetime
        /// </summary>
        public readonly int DefaultKeepAliveTime = 60 * 10; // sec

        /// <summary>
        /// Default KeepAliveInterval
        /// </summary>
        public readonly int DefaultKeepAliveInterval = 60; // sec

        /// <summary>
        /// Default Per SendingQueue Size
        /// </summary>
        public readonly int DefaultSendingQueueSize = 5;

        /// <summary>
        /// Default Listen backlog
        /// </summary>
        public const int DefaultListenbacklog = 100;

        /// <summary>
        /// Default Min Thread Count
        /// </summary>
        public readonly int DefaultMinThreadCount = 1;

        /// <summary>
        /// Default Max Thread Count
        /// </summary>
        public readonly int DefaultMaxThreadCount = 8;

        // 15. Socket Option (Linger) flag
        public readonly bool DefaultSocketLingerFlag = false;

        // 16. Socket Option (Linger = true) DelayTime
        public readonly int DefaultSocketLingerDelayTime = 10;

        // 17. Server Name
        public readonly string DefaultServerName = "DefaultServerName";

        // 18. encoding
        public readonly string DefaultEncoding = Encoding.Default.ToString();

        //-------------------------------------------------------------------------

        // 1. Accept SocketAsyncEventArgs Pooling 갯수
        public int max_accept_count { get; set; }

        // 2. Accpet Socket Recv/Send SocketAsyncEventArgs Pooling 갯수
        public int max_connect_count { get; set; }

        // 3. IP
        public string ip { get; set; }

        // 4. Port
        public ushort port { get; set; }

        // 5. KeepAliveTime 
        public int keepAliveTime { get; set; }

        // 6. KeepAliveInterval
        public int keepAliveInterval { get; set; }

        // 7. Naggle 알고리즘 사용 유뮤 (true:미사용, false:사용)
        public bool noDelay { get; set; }

        // 8. 소켓 타입 (tcp - udp)
        public eSocketMode socketType { get; set; }

        // 9. Send용 버퍼사이즈
        public int sendBufferSize { get; set; }

        // 10. Recv용 버퍼사이즈
        public int recvBufferSize { get; set; }

        // 11. Per SendingQueue 사이즈
        public int sendingQueueSize { get; set; }

        // 12. ListenBacklog
        public int listenBacklog { get; set; }

        // 13. Min Thread Count 
        public int minThreadCount { get; set; }

        // 14. Max Thread Count
        public int maxThreadCount { get; set; }

        // 15. Socket Option (Linger) flag
        public bool socketLingerFlag { get; set; }

        // 16. Socket Option (Linger = true) DelayTime
        public int socketLingerDelayTime { get; set; }

        // 17. Server Name
        public string serverName { get; set; }

        // 18. Encoding
        public string encoding { get; set; }

        public CServerConfig()
        {
            max_accept_count = DefaultMaxAcceptCount;
            max_connect_count = DefaultMaxConnectCount;
            keepAliveTime = DefaultKeepAliveTime;
            keepAliveInterval = DefaultKeepAliveInterval;
            sendBufferSize = DefaultSendBufferSize;
            recvBufferSize = DefaultRecvBufferSize;
            sendingQueueSize = DefaultSendingQueueSize;
            listenBacklog = DefaultListenbacklog;
            minThreadCount = DefaultMinThreadCount;
            maxThreadCount = DefaultMaxThreadCount;
            socketLingerFlag = DefaultSocketLingerFlag;
            socketLingerDelayTime = DefaultSocketLingerDelayTime;
            serverName = DefaultServerName;
            encoding = DefaultEncoding;
        }
    }
}
