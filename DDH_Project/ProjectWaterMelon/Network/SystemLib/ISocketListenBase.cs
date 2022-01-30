using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

using ProjectWaterMelon.Network.Config;

/// <summary>
/// CTcpListener -> SocketListenBase -> ISocketListenBase
/// </summary>
namespace ProjectWaterMelon.Network.SystemLib
{
    public delegate void ErrorHandler(Exception ex);
    public delegate void NewClientAccessHandler(ISocketListenBase listenSocket, Socket socket, object state);

    public interface ISocketListenBase
    {
        IPEndPoint EndPoint { get; }

        bool Start(IListenConfig config, int maxAcceptSize);

        void Stop();

        event NewClientAccessHandler NewClientAccepted;

        event ErrorHandler Error;

        event EventHandler Stopped;

    }
}
