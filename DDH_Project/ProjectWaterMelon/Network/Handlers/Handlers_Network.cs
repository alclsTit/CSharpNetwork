﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Log;
using ProjectWaterMelon.Network.Session;
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
                Console.WriteLine($"{notify_msg.msg_id} --- {notify_msg.cur_datetime} --- {mPacket.mPacketHeader.mDirectFlag}");

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
}
