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
    public static class CMessageReceiver
    {
        public static void Init()
        {
            RegisterMessageHandler<handler_notify_test_packet_game2user>();
        }
    }
}
