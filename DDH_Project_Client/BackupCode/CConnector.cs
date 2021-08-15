using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static ConstModule.ConstDefine;
using static ConstModule.GSocketState;
using ProjectWaterMelon.Game;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network
{
    class CConnector
    {
        public Socket mSocket { get; private set; }

        private AutoResetEvent mFlowControlEvt;
        private SocketAsyncEventArgs mConnectArgs;
        private CSocketAsyncEventArgsPool mConnectAsyncPool;

        private CBufferManager mBufferMng = new CBufferManager(MAX_CONNECTION, MAX_BUFFER_SIZE);

        private Object mTestLockObj = new object();

        private AutoResetEvent lAutoResetEvt = new AutoResetEvent(true);
        public CConnector()
        {
            //Set Connect SocketAsyncEventArgsPool
            mConnectAsyncPool = new CSocketAsyncEventArgsPool(MAX_CONNECTION, eSockEvtType.CONCURRENT);
            //SetConnectArgsMax();

            for (var tNum = 1; tNum <= 4; ++tNum)
            {
                //Console.WriteLine($"Number = {tNum}, Count = {mConnectAsyncPool.CurrentCount}");          
                lAutoResetEvt.WaitOne();
                var lStartTime = DateTime.Now;
                var localThread = tNum;
                for (var n = 1; n <= tNum; ++n)
                {
                    Thread lThread = new Thread(() => PushAndPopInThreads((10000 / localThread), localThread, ref lStartTime));
                    lThread.Start();
                }
            }
            

            // AutoResetEvent 를 false로 지정시 해당 이벤트가 호출된 스레드는 다음 AutoResetEvent.Set을 만나기 전까지 대기상태
            // 비동기 -> 동기처럼 진행하게끔 
            mFlowControlEvt = new AutoResetEvent(true);
        }

        public void PushAndPopInThreads(int count, int num, ref DateTime stime)
        {
            lock (mTestLockObj)
            {
                for (var i = 0; i < count; ++i)
                {
                    SocketAsyncEventArgs lItem = new SocketAsyncEventArgs();
                    lItem.UserToken = new CSession();
                    mConnectAsyncPool.Push(lItem);
                    //var lSession = lItem.UserToken as CSession;
                    //Console.WriteLine($"[{num}] ThreadId = {Thread.CurrentThread.ManagedThreadId}, TokenId = {lSession?.GetTokenId()}");
                    //Console.WriteLine($"{i} - {count}");
                }
                //Console.WriteLine($"[{num}] ThreadId = {Thread.CurrentThread.ManagedThreadId}, Count = {mConnectAsyncPool.CurrentCount}");

                for (var i = 0; i < count; ++i)
                {
                    var lConnectArgs = mConnectAsyncPool.Pop();
                    var lSession = lConnectArgs.UserToken as CSession;
                    //Console.WriteLine($"[{num}] ThreadId = {Thread.CurrentThread.ManagedThreadId}, TokenId = {lSession?.GetTokenId()}");
                }
                var lElaspedTime = DateTime.Now - stime;
                Console.WriteLine($"[{num}] ThreadId = {Thread.CurrentThread.ManagedThreadId}, ElaspedTime = {lElaspedTime}, Count = {mConnectAsyncPool.CurrentCount}");

                lAutoResetEvt.Set();
            }
        }

        public void Init(string host, ushort port, bool tcpFlag = true, bool ipv4Flag = true)
        {
            try
            {
                if (tcpFlag)
                {
                    //tcp 
                    if (ipv4Flag)
                        mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    else
                        mSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    // udp
                    if (ipv4Flag)
                        mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    else
                        mSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                }

                var llPAdress = host == "0.0.0.0" ? IPAddress.Any : IPAddress.Parse(host);
                IPEndPoint lEndPoint = new IPEndPoint(llPAdress, port);

                mSocket.NoDelay = true;   // nagle 알고리즘을 사용하지 않는다. (네트워크 트래픽 부하 감소 보단 성능선택)

                CSocketAsyncEventManager.Init(MAX_CONNECTION, eSockEvtType.CONCURRENT);
                CSocketAsyncEventManager.InitRecvAndSendArgsMax(mSocket);

            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CConnector.Init!!! - {ex.Message},{ex.StackTrace}");
            }
        }

        public void Start()
        {
            try
            {
                CLog4Net.LogInfo(true, $"1.[FLOW]CConnector.Start", $"Connect Start");

                while (true)
                {
                    mFlowControlEvt.WaitOne();

                    mConnectArgs = mConnectAsyncPool.Count > 0 ? mConnectAsyncPool.Pop() : CreateConnectArgs();
                    mConnectArgs.AcceptSocket = mSocket;

                    var lPending = mSocket.ConnectAsync(mConnectArgs);
                    if (!lPending)
                        OnConnectHandler(this, mConnectArgs);
                }
            }
            catch(Exception ex)
            {
                CLog4Net.LogError($"Exception in CConnector.Start!!! - {ex.Message},{ex.StackTrace}");
            }
        }

        public void SetConnectArgsMax()
        {
            for(var i = 0; i < MAX_ACCEPT_OPT; ++i)
            {
                SocketAsyncEventArgs lConnectArgs = new SocketAsyncEventArgs();
                mBufferMng.SetBuffer(lConnectArgs);

                lConnectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectHandler);
                //lConnectArgs.UserToken = new CSession();

                mConnectAsyncPool.Push(lConnectArgs);
            }
        }

        public SocketAsyncEventArgs CreateConnectArgs()
        {
            SocketAsyncEventArgs lConnectArgs = new SocketAsyncEventArgs();
            lConnectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectHandler);
            //lConnectArgs.UserToken = new CSession();
            return lConnectArgs;
        }

        public void OnBadConnectHandler(ref SocketAsyncEventArgs args)
        {
            var session = args.UserToken as CSession;
            session?.mSocket.Disconnect();
            args.AcceptSocket = null;
            mConnectAsyncPool.Push(args);
        }

        public void OnConnectHandler(object send, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    CLog4Net.LogInfo(true, $"2.[FLOW]CConnector.OnConnectHandler", $"Connect Success");

                    CGameUser lGameUser = new CGameUser();


                }
                else
                {

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception in CConnector.OnConnectHandler!!! - {0}, {1}", ex.Message, ex.StackTrace);
            }
            mFlowControlEvt.Set();
        }

    }
}
