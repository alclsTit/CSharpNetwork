using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using ProjectWaterMelon.Log;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Network.Server;

namespace ProjectWaterMelon.Network.Session
{
    /// <summary>
    /// Session Interface (client : session = 1 : 1)
    /// 각각의 세션은 고유의 TCPSOCKET 클래스를 갖고있다 (여기서 socket 패킷처리)
    /// </summary>
    public interface ISessionBase
    {
        /// <summary>
        /// Session과 연동된 Server 
        /// </summary>
        IAppServer server { get; }

        /// <summary>
        /// Session에서 사용될 config
        /// </summary>
        IServerConfig config { get; }

        /// <summary>
        /// logger 클래스
        /// </summary>
        CLogger logger { get; }        

        /// <summary>
        /// session 연결 유무
        /// </summary>
        bool isConnected { get; }

        /// <summary>
        /// session id (set each client)
        /// </summary>
        string sessionID { get; }

        /// <summary>
        /// 원격지(client) ip, port 정보
        /// </summary>
        IPEndPoint remoteEndPoint { get; }

        /// <summary>
        /// 서버 호스트 ip, port 정보
        /// </summary>
        IPEndPoint hostEndPoint { get; }

        /// <summary>
        /// init session
        /// </summary>
        void Initalize();

        /// <summary>
        /// start session
        /// </summary>
        void Start();

        /// <summary>
        /// close session
        /// </summary>
        void Close(eCloseReason reason);

    }
}
