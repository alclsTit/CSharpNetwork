using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ProjectWaterMelon.GameLib;

namespace ProjectWaterMelon.Network.Config
{
    /// <summary>
    /// 프로세스 내에서도 여러부분에서 사용될 수 있으므로 따로 전역클래스로 만들지는 않았음
    /// </summary>
    public class CConfigLoader
    {
        /// <summary>
        /// file 경로 혹은 이름은 한번 설정되면 변경되지 않는다 (이건 규칙)
        /// </summary>
        private readonly string mFileName = "";
        private readonly string mFilePath = "";
        private readonly string mFilePathName = "";

        /// <summary>
        /// 파일 이름 및 경로 세팅, 파일이 존재하는지 확인 및 config 세팅
        /// </summary>
        /// <param name="name"></param>
        /// <param name="serverConfig"></param>
        /// <param name="listenConfig"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public CConfigLoader(string name) 
        { 
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(name);

            mFileName = name;
            mFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"..\..\..\Config"));

            var lFileRealPath = new StringBuilder();
            lFileRealPath.Append(mFilePath);
            lFileRealPath.Append(@"\");
            lFileRealPath.Append(name);

            mFilePathName = lFileRealPath.ToString();
            if (!File.Exists(mFilePathName))
            {
                mFilePathName = "";
                throw new DirectoryNotFoundException($"Can't to load file [{lFileRealPath.ToString()}");
            }
        }


        // AppServer 한 객체에서만 한번 호출될 예정이므로 non-threadsafe 가능
        public bool LoadConfig(List<IListenConfig> listeners, IServerConfig serverConfig)
        {
            if (string.IsNullOrEmpty(mFilePathName))
                throw new ArgumentNullException(mFilePathName);

            if (listeners == null)
                throw new ArgumentNullException("List<IListenConfig> param is null");

            if (serverConfig == null)
                throw new ArgumentNullException("IServerConfig param is null");

            // [ConnectInfo] ini Section
            var lConnectInfoSection = "ConnectInfo";
            var lCountOfConnectedListener = Convert.ToInt32(IniConfig.IniFileRead(lConnectInfoSection, "Connect_Server", "0", mFilePathName));
            
            if (lCountOfConnectedListener >= 1)
            {
                if (listeners.Count > 0)
                    listeners.Clear();
            }
            else
            {
                throw new Exception($"Count of ConnectServer is abnormal - {lCountOfConnectedListener}");
            }
            
            for(var idx = 1; idx <= lCountOfConnectedListener; ++idx)
            {
                var lServerIP = IniConfig.IniFileRead(lConnectInfoSection, $"Sever_IP_{idx}", "127.0.0.1", mFilePathName);
                var lServerPort = Convert.ToUInt16(IniConfig.IniFileRead(lConnectInfoSection, $"Server_Port_{idx}", "8800", mFilePathName));
                var lServerName = IniConfig.IniFileRead(lConnectInfoSection, $"Server_Name_{idx}", "Default", mFilePathName);

                // 기본적으로 nagle 알고리즘을 사용하지 않는다. 즉 패킷을 모아서 보내지 않는다
                IListenConfig lListenConfig = new CListenConfig(lServerIP, lServerPort, true, lServerName);
                listeners.Add(lListenConfig);
            }

            // [ServerInfo] ini Section
            var lServerInfoSection = "ServerInfo";
            CServerConfig config = new CServerConfig();

            // 서버별 Accept 가능한 클라이언트 수
            config.max_accept_count = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Max_Accept_Number", $"{config.max_accept_count}", mFilePathName));

            // 서버별 Connect 가능한 클라이언트 수 
            config.max_connect_count = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Max_Connect_Number", $"{config.max_connect_count}", mFilePathName));

            // 하트비트 시작 기준 시간(초)
            config.keepAliveTime = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Keep_Alive_Time", $"{config.keepAliveTime}", mFilePathName));

            // 하트비트 보내는 횟수
            config.keepAliveInterval = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Keep_Alive_Interval", $"{config.keepAliveInterval}", mFilePathName));

            // Send 전용 버퍼사이즈 
            config.sendBufferSize = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Send_Buffer_Size", $"{config.sendBufferSize}", mFilePathName));

            // Recv 전용 버퍼사이즈 
            config.recvBufferSize = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Recv_Buffer_Size", $"{config.recvBufferSize}", mFilePathName));

            // Send Queue 사이즈 
            config.sendingQueueSize = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Send_Queue_Size", $"{config.sendingQueueSize}", mFilePathName));

            // BackLog
            config.listenBacklog = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Listen_Backlog", $"{config.listenBacklog}", mFilePathName));

            // 최소 스레드풀 워커 스레드 갯수 
            config.minWorkThreadCount = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Min_WorkThread_Count", $"{config.DefaultMinWorkThreadCount}", mFilePathName));

            // 최대 스레드풀 워커 스레드 갯수 
            config.maxWorkThreadCount = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Max_WorkThread_Count", $"{config.DefaultMaxWorkThreadCount}", mFilePathName));

            // 최소 스레드풀 IO 스레드 갯수
            config.minIOThreadCount = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Min_IOThread_Count", $"{config.DefaultMinIOThreadCount}", mFilePathName));

            // 최대 스레드풀 IO 스레드 갯수
            config.maxIOThreadCount = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Max_IOThread_Count", $"{config.DefaultMaxIOThreadCount}", mFilePathName));

            // Session Socket Linger Option (false = 소켓 Close 요청 후 대기하지않고 Close, true = 소켓 Close 요청 후 일정시간 대기 후 Close)
            config.socketLingerFlag = Convert.ToBoolean(IniConfig.IniFileRead(lServerInfoSection, "Socket_Close_Delay", $"{config.socketLingerFlag}", mFilePathName));

            // Session Socket Linger Option (True) 일 때, delay 시간
            config.socketLingerDelayTime = Convert.ToInt32(IniConfig.IniFileRead(lServerInfoSection, "Socket_Close_DelayTime", $"{config.socketLingerDelayTime}", mFilePathName));

            serverConfig = config;

            return true;
        }
    }
}
