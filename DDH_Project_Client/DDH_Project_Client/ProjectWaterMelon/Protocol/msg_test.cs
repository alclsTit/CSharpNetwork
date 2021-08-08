using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ProjectWaterMelon.Network;

namespace ProjectWaterMelon.Protocol
{
    public static class msg_test
    {
        // format 
        // 1. PacketId => 클라 - 서버간 통신용 프로토콜 아이디 
        // 2. data     => 패킷에 담길 데이터 

        [StructLayout(LayoutKind.Sequential)]
        public class handler_req_test_packet_user2game
        {
            public readonly PacketId msg_id = PacketId.req_test_packet;
            public long session_id;
            public string req_logdate;
        }

        public class handler_ack_test_packet_user2game
        {
            public readonly PacketId msg_id = PacketId.ack_test_packet;
            public long session_id;
            public string ack_logdate;
        }

        [Serializable]
        public class hanlder_notify_test_packet_game2user : CPacket
        {
            public readonly PacketId msg_id = PacketId.notify_test_packet;
            public string cur_datetime;
        }
    }
}
