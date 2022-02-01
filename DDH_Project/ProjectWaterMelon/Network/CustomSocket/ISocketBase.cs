using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Network.Session;

namespace ProjectWaterMelon.Network.CustomSocket
{
    public interface ISocketBase
    {
        /// <summary>
        /// get client socket
        /// </summary>
        Socket clientsocket { get; }

        /// <summary>
        /// get local endpoint
        /// </summary>
        IPEndPoint localEP { get; }

        /// <summary>
        /// start receive function
        /// </summary>
        void Start();

        /// <summary>
        /// close session
        /// </summary>
        void Close(eCloseReason reason);

        /// <summary>
        /// try to send array segment
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        bool TrySend(IList<ArraySegment<byte>> segments);

        /// <summary>
        /// try to send array segment
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        bool TrySend(ArraySegment<byte> segment);


        /// <summary>
        /// change socket state (socketstate = 32bit(4byte))
        /// [0]         [1 ~ 15]          [16 ~ 31] 
        /// no use     state(past)      state(current)
        /// </summary>
        /// <returns></returns>
        bool ChangeState(int state);

        int GetCurrentState { get; }

        int GetOldState { get; }

        bool CheckState(int state);

    }
}
