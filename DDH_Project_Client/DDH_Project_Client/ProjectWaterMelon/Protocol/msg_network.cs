using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProtoBuf;
// -------------- //

namespace ProjectWaterMelon.Protocol
{
    /// <summary>
    /// 서버 - 클라간 네트워크 통신에 관련된 패킷 아이디 저장 
    /// </summary>
    class msg_network
    {
        [ProtoContract]
        public class notify_socket_session_connect_game2user
        {
            [ProtoMember(1)]
            public readonly PacketId msg_id = PacketId.notify_socket_session_connect;
            [ProtoMember(2)]
            public long session_id { get; set; }
        }

        [ProtoContract]
        public class notify_socket_session_close_game2user
        {
            [ProtoMember(1)]
            public readonly PacketId msg_id = PacketId.notify_socket_session_close;
            [ProtoMember(2)]
            public long session_id { get; set; }
        }
    }
}
