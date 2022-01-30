using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ProjectWaterMelon.GameLib;

namespace ProjectWaterMelon.Network.Config
{
    [Serializable]
    public class CListenConfig : IListenConfig
    {
        public string ip { get; private set; }

        public ushort port { get; private set; }

        public int backlog { get; private set; }

        public bool noDelay { get; private set; }

        public string serverName { get; private set; }

        public IPEndPoint endpoint { get; private set; }

        public CListenConfig(string ip, ushort port, bool noDelay, string serverName, int backlog)
        {
            this.ip = ip;
            this.port = port;
            this.backlog = backlog;
            this.serverName = serverName;
            this.noDelay = noDelay;

            this.endpoint = ListenOption.GetListenIPEndPoint(ip, port);
        }

        public CListenConfig(string ip, ushort port, bool noDelay, string serverName) : this(ip, port, noDelay, serverName, 100) { }
    }
}
