using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.SystemLib
{
    public interface ISocketServerBase
    {
        bool IsRunning { get; }

        bool Start();

        void Stop();

        bool ReStart();

    }
}
