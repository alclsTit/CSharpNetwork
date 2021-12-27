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
    public interface ISessionRoot : ISessionBase
    {
        /// <summary>
        /// init session
        /// </summary>
        void Initalize();

        /// <summary>
        /// start session
        /// </summary>
        void Start();

        /// <summary>
        /// close session
        /// </summary>
        void Close();

        /// <summary>
        /// occur close event
        /// </summary>
        Action<ISessionRoot, eCloseReason> OnClosed { get; }
    }
}
