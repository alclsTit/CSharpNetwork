using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.Log;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Network.SystemLib;
using ProjectWaterMelon.Network.Session;

namespace ProjectWaterMelon.Network.Server
{
    public abstract class CAppServerBase : IAppServer
    {
        /// <summary>
        /// 서버 상태
        /// </summary>
        public ServerState state { get; protected set; }

        /// <summary>
        /// 서버 타입
        /// </summary>
        public ServerType type { get; protected set; }

        /// <summary>
        /// 서버 이름 
        /// </summary>
        public string mName { get; private set; }

        /// <summary>
        /// 서버 시작 시간
        /// </summary>
        public DateTime mStartTime { get; private set; }
        
        /// <summary>
        /// 서버 옵션 세팅
        /// </summary>
        public IServerConfig config { get; private set; }

        /// <summary>
        /// 텍스트 인코딩 방식 지정 (default = ANSI)
        /// </summary>
        public Encoding mTextEncoding { get; private set; }

        /// <summary>
        /// 서버 당 관리하는 Listener는 1개
        /// </summary>
        private CTcpListener mListener;

        private CSessionManager mSessionManager;

        public abstract void Initialize();
        public abstract void Start();
        public abstract void Stop();
        public abstract void Setup();


        /// <summary>
        /// Setup 메서드에서 공통적으로 사용되는 부분 세팅
        /// </summary>
        /// <param name="serverConfig"></param>
        /// <returns></returns>
        protected Task<bool> SetupBasic(IServerConfig serverConfig)
        {
            if (serverConfig == null)
                throw new ArgumentException("ServerConfig");

            config = serverConfig;

            mName = config.serverName;

            type = ServerType.GameServer;

            mTextEncoding = string.IsNullOrEmpty(serverConfig.encoding) ? Encoding.Default : Encoding.GetEncoding(serverConfig.encoding);

            return Task.FromResult<bool>(true);
        }

        /// <summary>
        /// Listener 세팅. Server 당 Listener는 1개
        /// </summary>
        protected Task<bool> SetupListener(IListenConfig listenConfig, IAppServer server)
        {
            try
            {
                if (string.IsNullOrEmpty(listenConfig.ip))
                {
                    //TODO: Error 옵션 체크
                    GCLogger.Error(nameof(CAppServerBase), "SetupListener", "Lister IP doesn't set up");
                    return Task.FromResult<bool>(false);
                }

                if (listenConfig.port <= 0)
                {
                    //TODO: Error 옵션 체크
                    GCLogger.Error(nameof(CAppServerBase), "SetupListener", "Lister Port is abnormal");
                    return Task.FromResult<bool>(false);
                }

                mListener = new CTcpListener(listenConfig, server, config);

                return Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CAppServerBase), "SetupListener", ex);
                return Task.FromResult<bool>(false);
            }
        }

        protected Task<bool> StartListener()
        {

        }
        

    }
}
