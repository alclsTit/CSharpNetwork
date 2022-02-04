using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.Utility;
using ProjectWaterMelon.Network.SystemLib;
using ProjectWaterMelon.Log;

namespace ProjectWaterMelon.Network.Server
{
    public interface ISocketServer
    {
        /// <summary>
        /// App (응용프로그램) 전체에 걸쳐서 적용되는 대상을 가져올 때 사용
        /// </summary>
        IAppServer appServer { get; }

        /// <summary>
        /// Logger 추상클래스
        /// </summary>
        CLogger logger { get; }

        /// <summary>
        /// Pooling Manager - SendingQueue Pooling에 사용
        /// </summary>
        CPoolingManager<CSendingQueue> sendQueuePool { get; }

        /// <summary>
        /// SocketServer Start
        /// </summary>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// SocketServer Stop
        /// </summary>
        void Stop();
    }


    public abstract class CSocketServerBase : ISocketServer
    {
        public IAppServer appServer { get; private set; }

        public CPoolingManager<CSendingQueue> sendQueuePool { get; private set; }

        public List<CTcpListener> mListeners;

        public CLogger logger { get; private set; }

        CSocketServerBase(IAppServer appServer, int queuePerSize, int queueMaxSize)
        {
            this.appServer = appServer;

            this.logger = appServer.logger;

        }

        public virtual bool Start()
        {
            try
            {
                var serverConfig = appServer.serverConfig;
                var listenConfig = appServer.listenConfig;

                var queuePerSize = Math.Max(serverConfig.max_connect_count / 6, 256);
                var queueMaxSize = Math.Max(serverConfig.max_connect_count * 2, 256);

                sendQueuePool = new CPoolingManager<CSendingQueue>(new CSendingQueueCreator(queuePerSize), queueMaxSize);

                for(var idx = 0; idx < mListeners.Count; ++idx)
                {
                    if (string.IsNullOrEmpty(listenConfig.ip))
                    {
                        logger.Error(nameof(CSocketServerBase), "Start", "Listener IP doesn't set up");
                        return false;
                    }

                    if (listenConfig.port <= 0)
                    {
                        logger.Error(nameof(CSocketServerBase), "Start", "Listener Port is abnormal");
                        return false;
                    }

                    //Todo: ISocketServer 없애도록 수정 (2번째 파라미터)
                    CTcpListener newListener = new CTcpListener(listenConfig, this, serverConfig);

                    if (newListener.Start())
                    {
                        mListeners.Add(newListener);
                        logger.Debug($"CSocketServerBase.Start - Listener {newListener.hostEndPoint} was started");
                    }
                    else
                    {
                        // 하나의 listener가 구동 실패 시 모든 listener stop. 다른 listener 설정도 잘 못되었을 가능성이 있으므로...
                        foreach (var oldListener  in mListeners)
                        {
                            oldListener.Stop();
                            logger.Error(nameof(CSocketServerBase), "Start", $"Listener {newListener.hostEndPoint} failed to start");
                        }

                        mListeners.Clear();
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(nameof(CSocketServerBase), "Start", ex);
                return false;
            }

            return true;
        }

        public virtual void Stop()
        {

        }
    }
}
