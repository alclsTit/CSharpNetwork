using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.SystemLib
{
    public abstract class SocketServerBase : ISocketServerBase
    {
        public bool IsRunning { get; private set; }

        public virtual bool Start()
        {

        }
    }
}
