using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.Server
{
    public enum ServerType
    {
        GameServer = 0
    }

    public enum ServerState
    {
        NotInitialized = 0,

        Initialized = 1,

        SetupFinished = 2,

        StartFinished = 3,

        Running = 4,

        Stopped = 5
    }

    public static class GServerState
    {
        public const int NotInitialized = 0;

        public const int Initialized = 1;

        public const int SetupFinished = 2;

        public const int StartFinished = 3;

        public const int Running = 4;

        public const int Stopped = 5;
    }
}
