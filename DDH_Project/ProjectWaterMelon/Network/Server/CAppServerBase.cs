using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.Log;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Network.SystemLib;
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Network.ThreadLib;

namespace ProjectWaterMelon.Network.Server
{
    /// <summary>
    /// 어플리케이션 관련 내용 (대부분 한번 세팅되면 되는 대상)
    /// config, threadpool, encoding, logger...
    /// </summary>
    /// <typeparam name="TAppSession"></typeparam>
    public abstract class CAppServerBase<TAppSession> : IAppServer 
        where TAppSession: class, ISessionBase
    {
        /// <summary>
        /// 서버 상태
        /// </summary>
        public ServerState state { get; protected set; } = ServerState.NotInitialized;

        /// <summary>
        /// 서버 타입
        /// </summary>
        public ServerType type { get; protected set; }

        /// <summary>
        /// 서버 이름 
        /// </summary>
        public string name { get; protected set; }

        /// <summary>
        /// 서버 시작 시간
        /// </summary>
        public DateTime mStartTime { get; private set; }
        
        /// <summary>
        /// 서버 옵션 세팅
        /// </summary>
        public IServerConfig serverConfig { get; private set; }

        /// <summary>
        /// 텍스트 인코딩 방식 지정 (default = ANSI)
        /// </summary>
        public Encoding mTextEncoding { get; private set; }

        /// <summary>
        /// 서버 당 관리하는 Listener는 1개
        /// </summary>
        private CTcpListener mListener;

        /// <summary>
        /// Listen 세팅관련 정보가 포함된 컨테이너 
        /// </summary>
        protected List<IListenConfig> mListenInfo =  new List<IListenConfig>();

        /// <summary>
        /// 세션관리 매니저
        /// </summary>
        private CSessionManager<TAppSession> mSessionManager;

        /// <summary>
        /// Log Factory
        /// </summary>
        public ILogFactory logFactory { get; private set; }

        /// <summary>
        /// Logger 클래스
        /// </summary>
        public CLogger logger { get; private set; }

        /// <summary>
        /// Config Load 전용 클래스
        /// </summary>
        public CConfigLoader configLoader { get; private set; }

        private static bool msThreadPoolOnceFlag = false;

        public abstract void Initialize();
        public abstract void Start();
        public abstract void Stop();
        public abstract void Setup();

        protected CAppServerBase(string configFileName, ServerType type, ILogFactory logFactory = null)
        {
            if (string.IsNullOrEmpty(configFileName))
                throw new ArgumentNullException(configFileName);

            try
            {
                this.type = type;

                this.logFactory = logFactory ?? new CConsoleLogFactory();
                logger = this.logFactory.GetLogger();

                configLoader = new CConfigLoader(configFileName);
            }
            catch (ArgumentNullException NullException)
            {
                logger.Error("Exception in CAppServerBase.Constructor", NullException);
            }
            catch (System.IO.DirectoryNotFoundException dirException)
            {
                logger.Error("Exception in CAppServerBase.Constructor", dirException);
            }
            catch (Exception ex)
            {
                logger.Error("Exception in CAppServerBase.Constructor", ex);
            }
        }

        private CLogger GetLogger()
        {
            return logFactory.GetLogger();
        }

        /// <summary>
        /// Setup 메서드에서 공통적으로 사용되는 부분 세팅
        /// </summary>
        /// <param name="serverConfig"></param>
        /// <returns></returns>
        protected Task<bool> SetupBase(IServerConfig serverConfig)
        {
            if (serverConfig == null)
                throw new ArgumentNullException(nameof(serverConfig));

            try
            {
                if (configLoader.LoadConfig(mListenInfo, serverConfig))
                {
                    name = serverConfig.serverName;

                    mTextEncoding = string.IsNullOrEmpty(serverConfig.encoding) ? Encoding.Default : Encoding.GetEncoding(serverConfig.encoding);

                    if (!msThreadPoolOnceFlag)
                    {
                        CThreadPoolEx.ResetThreadPoolInfo(serverConfig.)
                        msThreadPoolOnceFlag = true;
                    }

                    state = ServerState.Initialized;
                }
                else
                {

                }

            }
            catch (Exception)
            {

                throw;
            }

            CThreadPoolEx.ResetThreadPoolInfo()


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
                    logger.Error(nameof(CAppServerBase<TAppSession>), "SetupListener", "Lister IP doesn't set up");
                    return Task.FromResult<bool>(false);
                }

                if (listenConfig.port <= 0)
                {
                    logger.Error(nameof(CAppServerBase<TAppSession>), "SetupListener", "Lister Port is abnormal");
                    return Task.FromResult<bool>(false);
                }

                mListener = new CTcpListener(listenConfig, server, config);

                return Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                logger.Error(nameof(CAppServerBase<TAppSession>), "SetupListener", ex);
                return Task.FromResult<bool>(false);
            }
        }

        protected Task<bool> StartListener()
        {

        }
        

    }
}
