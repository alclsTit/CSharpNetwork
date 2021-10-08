using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.MessageWorker;
// -------------- //

namespace ProjectWaterMelon.Network.Handlers
{
    public class handler_notify_test_packet_game2user : CMessageHandler
    {
        public handler_notify_test_packet_game2user() : base(Protocol.PacketId.notify_test_packet) { }

        public override bool Process()
        {
            try
            {
                var notify_msg = mPacket.BufferToMessage<Protocol.msg_test.notify_test_packet_game2user>(mPacket.mMsgBuffer);
                Console.WriteLine($"{notify_msg.msg_id} --- {notify_msg.cur_datetime}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override void CleanUp()
        {

        }
    }

    public class handler_req_network_session_game2user : CMessageHandler
    {
        public handler_req_network_session_game2user() : base(Protocol.PacketId.req_network_sessionid) { }

        public override bool Process()
        {
            try
            {
                var req_msg = mPacket.BufferToMessage<Protocol.msg_test.req_network_sessionid_user2game>(mPacket.mMsgBuffer);
                Console.WriteLine($"{req_msg.cur_datetime} --- {req_msg.session_id}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public override void CleanUp()
        {

        }
    }

}
