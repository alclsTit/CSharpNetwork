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
        private CListenConfig mListenInfo;

        /// <summary>
        /// Accept 처리 객체, Listen -> Accept가 진행(1:1관계)
        /// </summary>
        private CAcceptor mAcceptObj;

        private int mNumberOfMaxConnect;

        /// <summary>
        /// Listen, Accept 상태 
        /// </summary>
        //public bool isRunning { get; private set; } = false;

        // 클라이언트 접속 시 호출될 델리게이트 
        public delegate void NewClientAccessHandler(Socket _clientSocket, object _tocken);
        public NewClientAccessHandler CallBack_On_NewClient = null;

        public CTcpListener(IListenConfig config, int numberOfMaxConnect) : base(false)
        {       
            mListenInfo = config as CListenConfig;

            this.Initialize(numberOfMaxConnect);
        }

        public override void Initialize(int numberOfMaxConnect)
        {
            mNumberOfMaxConnect = numberOfMaxConnect;
        }

        public override bool Start()
        {
            var listenInfo = mListenInfo;
            try
            {
                if (listenInfo != null)
                {
                    var listenSocket = mListenSocket = new Socket(listenInfo.mIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    if (listenInfo.noDelay)
                        listenSocket.NoDelay = true;

                    listenSocket.Bind(listenInfo.mIPEndPoint);
                    listenSocket.Listen(listenInfo.backlog);

                    mAcceptObj = new CAcceptor(listenSocket, mNumberOfMaxConnect);
                    mAcceptObj.Start();

                    return true;
                }
                else
                {
                    GCLogger.Error(nameof(CTcpListener), "Start", "Listen config isn't set yet");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CTcpListener), "Start", ex, "Listen start fail");
                return false;
            }
        }

        public override void Stop()
        {
            if (mListenSocket == null)
                return;

            lock (this)
            {

            }

            OnStopped();
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
                    GCLogger.Error(nameof(CTcpListener), $"Start", $"Endpoin is null!!!");
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
