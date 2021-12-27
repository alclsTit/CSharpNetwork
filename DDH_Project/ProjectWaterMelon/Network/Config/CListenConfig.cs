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

        public IPEndPoint mIPEndPoint { get; private set; }

        public CListenConfig(string ip, ushort port, bool noDelay, int backlog)
        {
            this.ip = ip;
            this.port = port;
            this.backlog = backlog;
            this.noDelay = noDelay;

            this.mIPEndPoint = ListenOption.GetListenIPEndPoint(ip, port);
        }

        public CListenConfig(string ip, ushort port, bool noDelay) : this(ip, port, noDelay, 100) { }
    }
}
