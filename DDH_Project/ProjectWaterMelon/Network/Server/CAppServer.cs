using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Log;

namespace ProjectWaterMelon.Network.Server
{
    /// <summary>
    /// Server 객체
    /// Session 과 Server는 서로를 포함하고 있음
    /// </summary>
    public class CAppServer : CAppServerBase
    {
        private int mServerState = GServerState.NotInitialized;

        public int ServerState => mServerState;

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
        public override void Setup()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 서버에서 사용할 config(설정) 데이터로드
        /// </summary>
        public override void LoadConfig()
        {
            var fileName = "ConnectInfo";
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var filePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(exePath, @"..\..\..\..\")) + $"Config\\{fileName}.ini";

            // ini section [ConnectInfo]
            var secConnectInfo = "ConnectInfo";
            // ini section [ServerInfo]
            var secServerInfo = "ServerInfo";

            var connectedServerCnt = Convert.ToInt32(IniConfig.IniFileRead(secConnectInfo, "Connect_Server", "0", filePath));
            for (var idx = 1; idx <= connectedServerCnt; ++idx)
            {
                // 연결될 서버 IP, PORT
                var serverIP = IniConfig.IniFileRead(secConnectInfo, $"Server_IP_{idx}", "127.0.0.1", filePath);
                var serverPort = Convert.ToUInt16(IniConfig.IniFileRead(secConnectInfo, $"Server_Port_{idx}", "8800", filePath));
                var serverName = IniConfig.IniFileRead(secConnectInfo, $"Server_Name_{idx}", $"{mServerConfig.DefaultServerName}", filePath);

                CListenConfig listenConfig = new CListenConfig(serverIP, serverPort, true, serverName);
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

        // Todo: 반환 값 변경, 어느 부분에서 문제가 생긴건지 정보 전달 필요
        private async Task<bool> SubSetup(IServerConfig serverConfig, IListenConfig listenConfig, IAppServer server)
        {
            try
            {
                if (await SetupBasic(serverConfig))
                {
                    if (await SetupListener(listenConfig, server))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex) 
            {
                GCLogger.Error(nameof(CAppServer), "SubSetup", ex);
                return false;
            }
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

    }
}
