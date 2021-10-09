using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
// --- custom --- //
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Log;
// -------------- //

namespace ProjectWaterMelon.Network.MessageWorker
{
    static public class CMessageProcessorManager
    {
        static private ConcurrentQueue<CPacket> mConQueue = new ConcurrentQueue<CPacket>();

        public delegate void MessageProcessFunc(CPacket packet);
        static public Dictionary<Protocol.PacketId, MessageProcessFunc> mHandlers = new Dictionary<Protocol.PacketId, MessageProcessFunc>();

        static private void SetHandler(Protocol.PacketId msgid, MessageProcessFunc func)
        {
            if (mHandlers.ContainsKey(msgid))
            {
                CLog4Net.LogError($"Error in CMessageProcessorMng.SetHandler - {msgid} is already existed in HandlerContainer");
                return;
            }

            mHandlers.Add(msgid, func);
        }

        static public void RegisterMessageHandler(Protocol.PacketId msgid, MessageProcessFunc func)
        {
            SetHandler(msgid, func);
        }

        static public void RegisterMessageHandler<T>() where T : new()
        {
            var lMessageProcessor = new CMessageProcessor<T>();
            var lMessageId = lMessageProcessor.MessageId();
            MessageProcessFunc func = lMessageProcessor.Process;

            SetHandler(lMessageId, func);
        }

        static public bool RemoveMessageHandler(Protocol.PacketId msgid)
        {
            if (!mHandlers.ContainsKey(msgid))
            {
                mHandlers.Remove(msgid);
                return true;
            }
            else
            {
                CLog4Net.LogError($"Error in CMessageProcessorMng.RemoveMessageHandler - Can't find messageid");
                return false;
            }
        }

        static public bool IsExist(Protocol.PacketId msgid)
        {
            return mHandlers.ContainsKey(msgid);
        }

        public static void PushMsgToQueue(in CPacket packet)
        {
            if (packet == null) return;

            mConQueue.Enqueue(packet);
        }

        static public void HandleProcess(Protocol.PacketId msgid, CPacket packet)
        {
            if (mHandlers.ContainsKey(msgid))
            {
                MessageProcessFunc lFunc;
                foreach (var handler in mHandlers)
                {
                    if (handler.Key == msgid)
                    {
                        lFunc = handler.Value;
                        lFunc(packet);
                        break;
                    }
                }
            }
        }
    }
}
