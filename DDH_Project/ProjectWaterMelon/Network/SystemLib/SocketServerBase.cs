using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ProjectWaterMelon.Network.SystemLib
{
    public abstract class SocketServerBase : ISocketServerBase
    {
        public bool isRunning { get; protected set; }
        public abstract void Initialize(int numberOfMaxConnect);

        public abstract bool Start();

        public abstract void Stop();

        public SocketServerBase(bool isRunning)
        {
            this.isRunning = isRunning;
        }
    }
}
