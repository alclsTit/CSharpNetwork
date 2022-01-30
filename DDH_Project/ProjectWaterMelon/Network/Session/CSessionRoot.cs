using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ProjectWaterMelon.Network.CustomSocket;

namespace ProjectWaterMelon.Network.Session
{
    public abstract class CSessionRoot : ISessionBase
    {
        /// <summary>
        /// session id
        /// </summary>
        public string sessionID { get; protected set; }

        public IPEndPoint remoteEndPoint { get; protected set; }

        public IPEndPoint hostEndPoint { get; protected set; }

        /// <summary>
        /// init session
        /// </summary>
        public abstract void Initalize();

        /// <summary>
        /// start session
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// close session
        /// </summary>
        public abstract void Close(eCloseReason reason);

        /// <summary>
        /// occur close event
        /// </summary>
        protected Action<ISessionBase, eCloseReason> OnClose;
    }
}

