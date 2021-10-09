using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Log;
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
                var curTick = GetTickCount();
                var notify_msg = mPacket.BufferToMessage<Protocol.msg_test.notify_test_packet_game2user>(mPacket.mMsgBuffer);
                Console.WriteLine($"{notify_msg.msg_id} --- {notify_msg.cur_datetime}");

                ChkPacketDelay(this.GetType().Name, curTick, mPacket.mPacketHeader.mProcessTickCount);
               
                return true;
            }
            catch (Exception ex)
            {
                CLog4Net.gLog4Net.Error($"Exception in handler_notify_test_packet_game2user - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public override void CleanUp()
        {

        }
    }

    public class handler_req_network_session_user2game : CMessageHandler
    {
        public handler_req_network_session_user2game() : base(Protocol.PacketId.req_network_sessionid) { }

        public override bool Process()
        {
            try
            {
                var curTick = GetTickCount();
                var req_msg = mPacket.BufferToMessage<Protocol.msg_test.req_network_sessionid_user2game>(mPacket.mMsgBuffer);
                Console.WriteLine($"{req_msg.cur_datetime} --- {req_msg.session_id}");

                var ack_msg = new Protocol.msg_test.ack_network_sessionid_game2user();
                ack_msg.cur_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                ack_msg.session_id = req_msg.session_id;
                mSocket.Relay<Protocol.msg_test.ack_network_sessionid_game2user>(ack_msg.msg_id, ack_msg, true);

                ChkPacketDelay(this.GetType().Name, curTick, mPacket.mPacketHeader.mProcessTickCount);

                return true;
            }
            catch (Exception ex)
            {
                CLog4Net.gLog4Net.Error($"Exception in handler_req_network_session_game2user - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public override void CleanUp()
        {

        }
    }

}
