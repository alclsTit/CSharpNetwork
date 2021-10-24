using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
// --- custom --- //
using ProtoBuf;
using ProjectWaterMelon.Network.Packet;
// -------------- //

namespace ProjectWaterMelon.Protocol
{
    public static class msg_test
    {
        // 별도의 class, interface 상속은 아직 하지않은 상태
        // format 
        // 1. PacketId => 클라 - 서버간 통신용 프로토콜 아이디 
        // 2. data     => 패킷에 담길 데이터 
        /* [ProtoContract]                                                      -> protobuf 캡션추가 
         * public class handler_[req/ack/notify/tell]_[contents]_[수신대상]      -> 송수신 클래스 제작 
         * {
         *      [ProtoMember(1)]                                                -> protobuf를 사용해 전달할 메시지 캡션 (n 은 1부터 증가)
         *      public readonly PacketId msg_id = xxx                           -> msg type 정의
         *      [ProtoMember(2)
         *      public string name {get; set;}                                  -> msg data 정의
         *      ...    
         * }
         */
        [ProtoContract]
        public class req_test_packet_user2game
        {
            [ProtoMember(1)]
            public readonly PacketId msg_id = PacketId.req_test_packet;
            [ProtoMember(2)]
            public string logdate;
        }

        [ProtoContract]
        public class ack_test_packet_user2game
        {
            [ProtoMember(1)]
            public readonly PacketId msg_id = PacketId.ack_test_packet;
            [ProtoMember(2)]
            public string logdate;
        }

        [ProtoContract]
        public class notify_test_packet_game2user
        {
            [ProtoMember(1)]
            public readonly PacketId msg_id = PacketId.notify_test_packet;
            [ProtoMember(2)]
            public string cur_datetime { get; set; }
        }

    }
}
