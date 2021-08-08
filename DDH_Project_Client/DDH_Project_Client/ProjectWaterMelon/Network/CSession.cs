using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ProjectWaterMelon.Network
{
    // 실제 유저 객체 
    class CSession
    {
        // 연결된 소켓
        public long mSessionId { get; private set; }
        public CTcpSocket mTcpSocket { get; set; }

        public CSession()
        {
            mTcpSocket = new CTcpSocket();
        }

        public void SetSessionId(long id)
        {
            mSessionId = id;
        }
    }
}
