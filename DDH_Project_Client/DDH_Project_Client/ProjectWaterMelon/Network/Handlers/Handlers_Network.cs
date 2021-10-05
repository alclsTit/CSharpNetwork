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
    public class handler_req_network_session_game2user : CMessageHandler
    {
        public handler_req_network_session_game2user() : base(Protocol.PacketId.req_network_sessionid) { }

        public override bool Process()
        {
            try
            {
                var req_msg = new Protocol.msg_test.req_network_sessionid_user2game();
                Console.WriteLine($"{req_msg.cur_datetime} --- {req_msg.session_id}");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void CleanUp()
        {

        }
    }
}
