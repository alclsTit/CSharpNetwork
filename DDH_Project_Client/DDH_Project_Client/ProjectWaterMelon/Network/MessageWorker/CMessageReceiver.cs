using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Handlers;
using static ProjectWaterMelon.Network.MessageWorker.CMessageProcessorManager;
// -------------- //

namespace ProjectWaterMelon.Network.MessageWorker
{
    public class CMessageReceiver
    {
        public CMessageReceiver()
        {
            RegisterMessageHandler<handler_req_network_session_game2user>();
        }
    }
}
