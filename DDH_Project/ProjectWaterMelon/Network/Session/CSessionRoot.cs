using System;
using System.Net;

using ProjectWaterMelon.Log;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Network.Server;

namespace ProjectWaterMelon.Network.Session
{
    public abstract class CSessionRoot : ISessionBase
    {
        /// <summary>
        /// set linked server info
        /// 세션과 연결된 서버 정보 (서버는 여러종류가 있으므로 인터페이스로 보유)
        /// </summary>
        public IAppServer server { get; private set; }

        /// <summary>
        /// session에서 사용할 서버 config
        /// </summary>
        public IServerConfig config { get; protected set; }

        /// <summary>
        /// logger 클래스 
        /// </summary>
        public CLogger logger { get; protected set; }

        /// <summary>
        /// set session id
        /// Todo: session id 관련 풀링 필요?
        /// </summary>
        public string sessionID { get; private set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// set connection state
        /// </summary>
        public bool isConnected { get; protected set; } = false;

        /// <summary>
        /// set connected client endpoint
        /// </summary>
        public IPEndPoint remoteEndPoint { get; protected set; }

        /// <summary>
        /// set server host endpoint
        /// </summary>
        public IPEndPoint hostEndPoint { get; protected set; }

        /// <summary>
        /// init session
        /// </summary>
        public abstract void Initalize();

        /// <summary>
        /// start session
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// close session
        /// </summary>
        public abstract void Close(eCloseReason reason);

        /// <summary>
        /// occur close event
        /// </summary>
        protected Action<ISessionBase, eCloseReason> OnClose;

        public CSessionRoot(IAppServer server, CLogger logger)
        {
            if (server == null)
                throw new ArgumentException(nameof(server));

            this.server = server;
            this.config = server.config;
            this.logger = logger;   
        }
    }
}

