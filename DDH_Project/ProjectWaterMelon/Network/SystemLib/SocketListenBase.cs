using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ProjectWaterMelon.Network.Config;


namespace ProjectWaterMelon.Network.SystemLib
{
    public abstract class SocketListenBase : ISocketListenBase
    {
        public IPEndPoint EndPoint { get; protected set; }

        public SocketListenBase() { }

        public abstract bool Start(IListenConfig config, int maxAcceptSize);

        public abstract void Stop();

        public event NewClientAccessHandler NewClientAccepted;

        public event ErrorHandler Error;

        public event EventHandler Stopped;

        protected void OnNewClientAcceptHandler(ISocketListenBase listener, Socket listenSocket, object state)
        {
            NewClientAccepted?.Invoke(listener, listenSocket, state);
        }

        protected void OnError(string errorMessage)
        {
            OnError(new Exception(errorMessage));
        }

        protected void OnError(in Exception ex)
        {
            Error?.Invoke(ex);
        }

        protected void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }
        
    }
}
