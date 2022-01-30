using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using ProjectWaterMelon.Network.Server;

namespace ProjectWaterMelon.Network.SystemLib
{
    public abstract class SocketServerBase : ISocketServerBase
    {
        public IAppServer server { get; private set; }

        public bool isRunning { get; protected set; }
        public abstract void Initialize();

        public abstract bool Start();

        public abstract void Stop();

        public EndPoint hostEndPoint { get; protected set; }

        public SocketServerBase(IAppServer server, bool isRunning)
        {
            this.server = server;
            this.isRunning = isRunning;
        }

        public event EventHandler AfterStop;

        protected void OnAfterStopHandler()
        {
            AfterStop?.Invoke(this, EventArgs.Empty);
        }
    }
}
