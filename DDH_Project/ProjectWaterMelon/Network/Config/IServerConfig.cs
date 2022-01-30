using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.Config
{
    /// <summary>
    /// Personal Server specific feature
    /// </summary>
    /// Todo: ini 설정파일로 서버 옵션 읽어들여서 서버별 세팅 및 사용예정 
    public partial interface IServerConfig
    {
        // 1. Accept SocketAsyncEventArgs Pooling 갯수
        int max_accept_count { get; }

        // 2. Accpet Socket Recv/Send SocketAsyncEventArgs Pooling 갯수
        int max_connect_count { get; }

        // 3. IP
        string ip { get; }

        // 4. Port
        ushort port { get; }

        // 5. KeepAliveTime 
        int keepAliveTime { get; }

        // 6. KeepAliveInterval
        int keepAliveInterval { get; }

        // 7. Naggle 알고리즘 사용 유뮤 (true:미사용, false:사용)
        bool noDelay { get; }

        // 8. 소켓 타입 (tcp - udp)
        eSocketMode socketType { get; }

        // 9. Send용 버퍼사이즈
        int sendBufferSize { get; }

        // 10. Recv용 버퍼사이즈
        int recvBufferSize { get; }

        // 11. Per SendingQueue 사이즈 
        int sendingQueueSize { get; }

        // 12. ListenBacklog
        int listenBacklog { get; }

        // 13. Min Thread Count 
        int minThreadCount { get; }

        // 14. Max Thread Count
        int maxThreadCount { get; }

        // 15. Socket Option (Linger) flag
        bool socketLingerFlag { get; }

        // 16. Socket Option (Linger = true) DelayTime
        int socketLingerDelayTime { get; }

        // 17. Server name 
        string serverName { get; }

        // 18. Encoding
        string encoding { get; }
    }
}
