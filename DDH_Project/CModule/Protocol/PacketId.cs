using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CModule.Protocol
{
    // 클라 - 서버간 통신용 프로토콜 아이디 
    // ex) req_패킷용도 (req - ack : 클라 - 서버간 양방향 통신프로토콜, notify : 서버 -> 클라 단방향 통신 프로토콜)

    public enum PacketId
    {
        //TEST_PACKET
        _TEST_PACKET_BEGIN = 0,
        req_test_packet = 1,
        ack_test_packet = 2,
        notify_test_packet = 3,
        req_network_sessionid = 4,
        ack_network_sessionid = 5,
        _TEST_PACKET_END = 10
        //
    }
}
