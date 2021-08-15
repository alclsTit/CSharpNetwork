using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using CModule.Network;
using ProtoBuf;

namespace CModule.Protocol
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

        [ProtoContract]
        public class hanlder_notify_test_packet_game2user : CPacket
        {
            [ProtoMember(1)]
            public readonly PacketId msg_id = PacketId.notify_test_packet;
            [ProtoMember(2)]
            public string cur_datetime { get; set; }
        }

        [ProtoContract]
        public class handler_req_network_sessionid_user2game : CPacket
        {
            [ProtoMember(1)]
            public readonly PacketId msg_id = PacketId.req_network_sessionid;
            [ProtoMember(2)]
            public long session_id { get; set; }
            [ProtoMember(3)]
            public string cur_datetime { get; set; }
        }

        [Serializable]
        public class handler_ack_network_sessionid_user2game : CPacket
        {
            public readonly PacketId msg_id = PacketId.ack_network_sessionid;
            public long session_id;
            public string cur_datetime;
        }
    }
}
