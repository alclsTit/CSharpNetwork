using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Utility;
using ProjectWaterMelon.Network.CustomSocket;
// -------------- //

namespace ProjectWaterMelon.Network.Session
{
    // 실제 유저 객체 
    public class CSession
    {
        // 세션 이벤트 핸들러
        public event AsyncEventHandler OnCloseEvent;
        // 세션 아이디 
        public long mSessionID { get; private set; }
        // 연결된 소켓
        public CTcpSocket mTcpSocket { get; set; }

        public CSession()
        {
            mTcpSocket = new CTcpSocket();
        }

        public void SetSessionID(long id)
        {
            mSessionID = id;       
        }

        public void NotifyConnected()
        {
            var notify_msg = new Protocol.msg_network.notify_socket_session_connect_game2user();
            notify_msg.session_id = mSessionID;
            mTcpSocket.Relay<Protocol.msg_network.notify_socket_session_connect_game2user>(notify_msg.msg_id, notify_msg);
        }

        public void NotifyClosed()
        {
            var notify_msg = new Protocol.msg_network.notify_socket_session_close_game2user();
            notify_msg.session_id = mSessionID;
            mTcpSocket.Relay<Protocol.msg_network.notify_socket_session_close_game2user>(notify_msg.msg_id, notify_msg);
        }

    }
}
