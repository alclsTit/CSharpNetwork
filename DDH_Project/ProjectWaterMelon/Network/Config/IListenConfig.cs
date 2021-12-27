using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.Config
{
    /// <summary>
    /// Listen에서 사용할 Config
    /// </summary>
    public interface IListenConfig
    {
        string ip { get; }

        ushort port { get; }

        int backlog { get; }

        bool noDelay { get; }
    }
}
