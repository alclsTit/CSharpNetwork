using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.Session
{
    public enum SessionState
    {
        NotInitialized = 0,

        Initialized = 1,

        Running = 2,

        Stop = 3
    }


    public static class GSessionState
    {
        public const int NotInitialized = 0;

        public const int initialized = 1;

        public const int running = 2;

        public const int stop = 3;
    }
}
