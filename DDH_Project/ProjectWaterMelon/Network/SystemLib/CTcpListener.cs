using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Log;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Network.Server;
using static ProjectWaterMelon.ConstDefine;
using static ProjectWaterMelon.GSocketState;
// -------------- //

namespace ProjectWaterMelon.Network.SystemLib
{
    /// <summary>
    /// Tcp ListenSocket Class
    /// </summary>
    public sealed class CTcpListener : SocketServerBase //SocketListenBase
    {
        /// <summary>
        /// Listen 전용 클라이언트 통신 소켓, accept 소켓과 별도 
        /// </summary>
        public Socket mListenSocket { get; private set; }
        
        /// <summary>
        /// Listen 전용 Config 설정 파일(ip, port, backlog, ipendpoint)
        /// </summary>
        private IListenConfig mListenConfig;

        /// <summary>
        /// Server 전용 Config 설정 파일(recv, send buffer size, timeout...) 
        /// </summary>
        private IServerConfig mServerConfig;

        /// <summary>
        /// Accept 처리 객체, Listen -> Accept가 진행(1:1관계)
        /// </summary>
        private CAcceptor mAcceptObj;

        /// <summary>
        /// Lock 오브젝트
        /// </summary>
        private readonly object mLockObj = new object();

        /// <summary>
        /// Listener와 연결된 Server 정보, CAcceptor 객체에 전달을 위해서 해당 값 정의
        /// </summary>
        private ISocketServer mAppServer;

        public CTcpListener(IListenConfig config, ISocketServer server, IServerConfig serverConfig) : base(server, false)
        {
            mListenConfig = config;
            mServerConfig = serverConfig;
            mAppServer = server;

            this.Initialize();
            this.AfterStop += new EventHandler(OnListenerAfterStop);
        }

        public override void Initialize()
        {

        }

        public override bool Start()
        {
            var listenConfig = mListenConfig;
            try
            {
                var listenSocket = mListenSocket = new Socket(listenConfig.endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                listenSocket.LingerState = new LingerOption(false, 0);

                if (listenConfig.noDelay)
                    listenSocket.NoDelay = true;

                hostEndPoint = listenConfig.endpoint;
                listenSocket.Bind(listenConfig.endpoint);
                listenSocket.Listen(listenConfig.backlog);

                mAcceptObj = new CAcceptor(listenSocket, mAppServer, mServerConfig);
                mAcceptObj.Start();

                return true;
            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CTcpListener), "Start", ex, "Listen start fail");
                return false;
            }
        }

        public override void Stop()
        {
            lock (mLockObj)
            {
                if (mListenSocket == null)
                    return;

                try
                {
                    mListenSocket.Close();
                }
                catch (Exception ex)
                {
                    GCLogger.Error(nameof(CTcpListener), "Stop", ex); 
                }
                finally
                {
                    mListenSocket = null;
                }

                OnAfterStopHandler();
            }
        }

        /// <summary>
        /// Listen Stop 후처리 작업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListenerAfterStop(object sender, System.EventArgs e)
        {
            var listener = sender as ISocketServerBase;

            if (listener != null)
                GCLogger.Info(nameof(listener), "OnListenerAfterStop", $"Listen Stopped [EndPoint = {listener.hostEndPoint}]");
        }


        /*
        // Listen 소켓 하나당 Accpet Thread를 생성해서 Accpet 지속적으로 처리 
        public override bool Start(in IListenConfig config, int maxAcceptSize)
        {     
            try
            {
               
                // accept 
                //mAcceptor = new CAcceptor(mListenSocket, EndPoint, maxAcceptSize);
                //Thread lAsyncThread = new Thread(mAcceptor.Start);
                //lAsyncThread.Start();

                return true;
            }
            catch(Exception ex)
            {
                GCLogger.Error(nameof(CTcpListener), $"Start", ex, $"Host = {config.ip} - Port = {config.port}");
                return false;
            }
        }
         
        // Listen 소켓 하나당 Accpet Thread를 생성해서 Accpet 지속적으로 처리 
        public override bool Start(in IListenConfig config, int maxAcceptSize)
        {     
            try
            {
                EndPoint = CHostFinder.GetServerIPEndPointByLocal(config.port, true);
                if (EndPoint == null)
                {
                    GCLogger.Error(nameof(CTcpListener), $"Start", $"Endpoint is null!!!");
                    return false;
                }
                mIPEndPoint = EndPoint;

                // set socket
                mListenSocket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // bind/listen 
                mListenSocket.Bind(EndPoint);
                mListenSocket.Listen(config.backlog);
                mBackLog = config.backlog;

                // accept 
                mAcceptor = new CAcceptor(mListenSocket, EndPoint, maxAcceptSize);
                Thread lAsyncThread = new Thread(mAcceptor.Start);
                lAsyncThread.Start();

                return true;
            }
            catch(Exception ex)
            {
                GCLogger.Error(nameof(CTcpListener), $"Start", ex, $"Host = {config.ip} - Port = {config.port}");
                return false;
            }
        }

        public void Start(ushort port, int backlog)
        {
            IPEndPoint lEndPoint = CHostFinder.GetServerIPEndPointByLocal(port, mipv4Flag);
            try
            {
                // bind/listen 
                mListenSocket.Bind(lEndPoint);
                mListenSocket.Listen(backlog);

                // recv/send pool 생성 
                CSocketAsyncEventManager.Init(MAX_CONNECTION, eSockEvtType.CONCURRENT);
                CSocketAsyncEventManager.InitRecvAndSendArgsMax();

                // accept 
                m_Acceptor = new CAcceptor(mListenSocket, lEndPoint);
                Thread lAsyncThread = new Thread(m_Acceptor.Start);
                lAsyncThread.Start();
            }
            catch(Exception ex)
            {
                CLog4Net.LogError($"Exception in CListener.Start!!! - {ex.Message} - {ex.StackTrace}, host = {lEndPoint.Address}, port = {lEndPoint.Port}");
            }
        }
        */
    }
}
