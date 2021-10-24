using System;
// --- custom --- //
// -------------- //

namespace ProjectWaterMelon.Protocol
{
    // 클라 - 서버간 통신용 프로토콜 아이디 
    // ex) req_패킷용도 (req - ack : 클라 - 서버간 양방향 통신프로토콜, notify : 서버 -> 클라 단방향 통신 프로토콜)

    public enum PacketId : UInt32
    {
        //TEST_PACKET
        _TEST_PACKET_BEGIN = 0,
        notify_nohandled_packet = 1,
        req_test_packet = 2,
        ack_test_packet = 3,
        notify_test_packet = 4,
        req_network_sessionid = 5,
        ack_network_sessionid = 6,
        notify_network_sessionid = 7,
        _TEST_PACKET_END = 10,
        //

        _MSG_NETWORK_BEGIN = 20,
        notify_socket_session_connect = 21,
        notify_socket_session_close = 22,

        _MSG_NETWORK_END = 30
    }
}
