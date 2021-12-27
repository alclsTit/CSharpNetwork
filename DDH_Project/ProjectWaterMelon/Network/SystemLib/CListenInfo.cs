using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ProjectWaterMelon.Network.Config;

namespace ProjectWaterMelon.Network.SystemLib
{
    /// <summary>
    /// Set Listen Info
    /// </summary>
    [Serializable]
    public class CListenInfo
    {   
        public IPEndPoint mEndPoint { get; private set; }
        public int mBackLog { get; private set; }
    }
}
