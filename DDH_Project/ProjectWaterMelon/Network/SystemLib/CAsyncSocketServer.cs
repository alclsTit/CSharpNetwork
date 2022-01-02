using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

using ProjectWaterMelon.GameLib;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Log;


namespace ProjectWaterMelon.Network.SystemLib
{
    /// <summary>
    /// 비동기 소켓서버 총괄
    /// </summary>
    public sealed class CAsyncSocketServer
    {
        /// <summary>
        /// 서버 전용 Config (recv / send buffer 옵션 등)
        /// </summary>
        private CServerConfig mServerConfig;
        
        /// <summary>
        /// 지속적으로 사용되어야할 스레드 관리객체 
        /// </summary>
        private CThreadPoolManager mThreadPool;

        /// <summary>
        /// Listen 전용 Config (ip, port, endpoint, backlog)
        /// </summary>
        private List<CListenConfig> mListenConfigList = new List<CListenConfig>();

        /// <summary>
        /// Recv SocketAsyncEventArgs Pool의 SetBuffer진행 시 사용할 버퍼 매니저
        /// 서버에서 관리하는 모든 소켓 객체의 recv buffer 관리
        /// </summary>
        private CBufferManager mBufferManager;

        private CSocketAsyncEventArgsPool mConcurrentRecvPool;

        public CAsyncSocketServer(IServerConfig config)
        {
            mServerConfig = (CServerConfig)config;

            // Load Server Config
            LoadConfig();
        }

        /// <summary>
        /// 서버에서 사용할 config(설정) 데이터로드
        /// </summary>
        public void LoadConfig()
        {
            var fileName = "ConnectInfo";
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var filePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(exePath, @"..\..\..\..\")) + $"Config\\{fileName}.ini";

            // ini section [ConnectInfo]
            var secConnectInfo = "ConnectInfo";
            // ini section [ServerInfo]
            var secServerInfo = "ServerInfo";

            var connectedServerCnt = Convert.ToInt32(IniConfig.IniFileRead(secConnectInfo, "Connect_Server", "0", filePath));
            for(var idx = 1; idx <= connectedServerCnt; ++idx)
            {
                // 연결될 서버 IP, PORT
                var serverIP = IniConfig.IniFileRead(secConnectInfo, $"Server_IP_{idx}", "", filePath);
                var serverPort = Convert.ToUInt16(IniConfig.IniFileRead(secConnectInfo, $"Server_Port_{idx}", "", filePath));
                
                CListenConfig listenConfig = new CListenConfig(serverIP, serverPort, true);
                mListenConfigList.Add(listenConfig);
            }

            // 서버별 Accept 가능한 클라이언트 수
            mServerConfig.max_accept_count = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Max_Accept_Number", "10000", filePath));

            // 서버별 Connect 가능한 클라이언트 수 
            mServerConfig.max_connect_count = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Max_Connect_Number", "10000", filePath));

            // 하트비트 시작 기준 시간(초)
            mServerConfig.keepAliveTime = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Keep_Alive_Time", "3", filePath));

            // 하트비트 보내는 횟수
            mServerConfig.keepAliveInterval = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Keep_Alive_Interval", "5", filePath));

            // Send 전용 버퍼사이즈 
            mServerConfig.sendBufferSize = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Send_Buffer_Size", "4096", filePath));

            // Recv 전용 버퍼사이즈 
            mServerConfig.recvBufferSize = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Recv_Buffer_Size", "4096", filePath));

            // Send Queue 사이즈 
            mServerConfig.sendingQueueSize = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Send_Queue_Size", "5", filePath));

            // BackLog
            mServerConfig.listenBacklog = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Listen_Backlog", "100", filePath));

            // 최소 스레드 갯수 (main thread 제외)
            mServerConfig.minThreadCount = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Min_Thread_Count", "1", filePath));

            // 최대 스레드 갯수 (main thread 제외)
            mServerConfig.maxThreadCount = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Max_Thread_Count", "8", filePath));

            // Session Socket Linger Option (false = 소켓 Close 요청 후 대기하지않고 Close, true = 소켓 Close 요청 후 일정시간 대기 후 Close)
            mServerConfig.socketLingerFlag = Convert.ToBoolean(IniConfig.IniFileRead(secServerInfo, "Socket_Close_Delay", "false", filePath));

            // Session Socket Linger Option (True) 일 때, delay 시간
            mServerConfig.socketLingerDelayTime = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Socket_Close_DelayTime", "10", filePath));
        
        }

        public async Task Start()
        {
            bool result;

            //result = await SetupRecvBufferPool();

            if (await SetupThreadPool())
                await mThreadPool.StartAllThread();          
        }

        /// <summary>
        /// Recv 전용 buffer pool 생성 및 세팅
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SetupRecvBufferPool()
        {
            mBufferManager = new CBufferManager(mServerConfig.recvBufferSize, mServerConfig.recvBufferSize * mServerConfig.max_connect_count);
            mConcurrentRecvPool = new CSocketAsyncEventArgsPool(mServerConfig.max_connect_count);

            try
            {
                // 단순 루프문 및 이 곳에서만 사용되므로 람다식으로 작성
                await Task.Run(() => {
                    for (var idx = 0; idx < mServerConfig.max_connect_count; ++idx)
                    {
                        SocketAsyncEventArgs recvAsyncEvtObj = new SocketAsyncEventArgs();
                        if (mBufferManager.SetBuffer(ref recvAsyncEvtObj))
                        {
                            mConcurrentRecvPool.Push(recvAsyncEvtObj);
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CAsyncSocketServer), "SetupRecvBufferPool", ex);
                return false;
            }
        }

        /// <summary>
        /// ThreadPoolManager 세팅
        /// </summary>
        private async Task<bool> SetupThreadPool()
        {
            mThreadPool = new CThreadPoolManager(mServerConfig.minThreadCount, mServerConfig.maxThreadCount);

            try
            {
                // 1.Accept 전용 스레드 생성
                var result = await mThreadPool.SetupThreadInfo(() => {
                        foreach (var listenObj in mListenConfigList)
                        {
                            // TcpListener 세팅, 많지않고 한번 세팅진행하므로 따로 풀링하지 않음
                            CTcpListener listener = new CTcpListener(listenObj, mServerConfig);
                            listener.Start();
                        }
                    }, "AcceptThread", true);
                if (!result)
                {
                    GCLogger.Error(nameof(CAsyncSocketServer), "SetupThreadPool", "ThreadPoolManager[AcceptThread] set fail");
                    return result;
                }

                return result;

            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CAsyncSocketServer), "SetUpThreadPool", ex);
                return false;
            }
        }

    }

   
}
