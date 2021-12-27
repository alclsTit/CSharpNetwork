using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.GameLib;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Log;

namespace ProjectWaterMelon.Network.SystemLib
{
    /// <summary>
    /// 비동기 소켓서버 총괄
    /// </summary>
    class CAsyncSocketServer
    {
        private CServerConfig mServerConfig;
        private CThreadPoolManager mThreadPool;

        private List<CListenConfig> mListenConfigList = new List<CListenConfig>();

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
            mServerConfig.sendBufferSize = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Send_Buffer_Size", "1024", filePath));

            // Recv 전용 버퍼사이즈 
            mServerConfig.recvBufferSize = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Recv_Buffer_Size", "1024", filePath));

            // Send Queue 사이즈 
            mServerConfig.sendingQueueSize = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Send_Queue_Size", "5", filePath));

            // BackLog
            mServerConfig.listenBacklog = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Listen_Backlog", "100", filePath));

            // 최소 스레드 갯수 (main thread 제외)
            mServerConfig.minThreadCount = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Min_Thread_Count", "1", filePath));

            // 최대 스레드 갯수 (main thread 제외)
            mServerConfig.maxThreadCount = Convert.ToInt32(IniConfig.IniFileRead(secServerInfo, "Max_Thread_Count", "8", filePath));
        }

        public void Start()
        {
            
        }

        public async ValueTask SetUp()
        {
            await SetupThreadPool();
        }


        /// <summary>
        /// TcpListener 세팅, 많지않고 한번 세팅진행하므로 따로 풀링하지 않음 
        /// </summary>
        private void StartTcpListener()
        {
            foreach (var listenObj in mListenConfigList)
            {
                CTcpListener listener = new CTcpListener(listenObj);
                listener.Start(mServerConfig.max_connect_count);
            }
        }

        /// <summary>
        /// ThreadPoolManager 세팅
        /// </summary>
        private async Task SetupThreadPool()
        {
            mThreadPool = new CThreadPoolManager(mServerConfig.minThreadCount, mServerConfig.maxThreadCount);

            var result = await mThreadPool.SetupThreadInfo(this.StartTcpListener, "AcceptThread", true);
            if (!result)
                GCLogger.Error(nameof(CAsyncSocketServer), "SetupThreadPool", "ThreadPoolManager[AcceptThread] set fail");

        }

    }

   
}
