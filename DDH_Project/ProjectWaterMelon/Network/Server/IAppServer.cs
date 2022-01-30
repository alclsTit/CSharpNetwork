using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.Network.Config;

namespace ProjectWaterMelon.Network.Server
{
    /// <summary>
    /// Server (gameserver, worldserver etc...)
    /// </summary>
    public interface IAppServer
    {
        ServerState state { get; }
        ServerType type { get; }

        IServerConfig config { get; }

        void Initialize();

        void Start();

        void Setup();

        void Stop();

    }
}
