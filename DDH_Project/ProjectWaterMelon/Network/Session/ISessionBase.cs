using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using ProjectWaterMelon.Network.CustomSocket;
using ProjectWaterMelon.Network.Config;

namespace ProjectWaterMelon.Network.Session
{
    /// <summary>
    /// Session Interface (client : session = 1 : 1)
    /// 각각의 세션은 고유의 TCPSOCKET 클래스를 갖고있다 (여기서 socket 패킷처리)
    /// </summary>
    public interface ISessionBase
    {
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
