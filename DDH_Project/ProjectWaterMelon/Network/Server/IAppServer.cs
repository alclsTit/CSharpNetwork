using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.Log;
using ProjectWaterMelon.Network.Config;

namespace ProjectWaterMelon.Network.Server
{
    /// <summary>
    /// Server (gameserver, worldserver etc...)
    /// </summary>
    public interface IAppServer
    {
        /// <summary>
        /// 서버 이름
        /// </summary>
        string name { get; }

        /// <summary>
        /// 서버 상태 
        /// </summary>
        ServerState state { get; }

        /// <summary>
        /// 서버 타입
        /// </summary>
        ServerType type { get; }

        /// <summary>
        /// 서버 전용 config(세팅) 파일
        /// </summary>
        IServerConfig serverConfig { get; }

        /// <summary>
        /// 서버 Listener 전용 config(세팅) 파일
        /// </summary>
        IListenConfig listenConfig { get; }
        
        /// <summary>
        /// Log 전용 팩토리 패턴
        /// </summary>
        ILogFactory logFactory { get; }

        /// <summary>
        /// Logger 클래스
        /// </summary>
        CLogger logger { get; }

        CConfigLoader configLoader { get; }

        void Initialize();

        void Start();

        void Setup();

        void Stop();

    }
}
