using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Log;
// -------------- //

namespace ProjectWaterMelon.Network.MessageWorker
{
    public class CMessageProcessor<T> where T : new()
    {
        public CMessageProcessor() {}

        public Protocol.PacketId MessageId()
        {
            var handler = (IMessageBase)new T();
            return handler.MessageId();
        }

        public bool SafePrepare(in CPacket packet, IMessageBase handler)
        {
            try
            {
                var result = handler.Prepare(packet);
                if (!result)
                    CLog4Net.LogDebug($"Debug in CMessageProcessor.SafePrepare({handler.MessageId().ToString()}) - Prepare Function fail");
                return result;
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CMessageProcessor.SafePrepare({handler.MessageId().ToString()}) - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public bool SafeProcess(IMessageBase handler)
        {
            try
            {
                var result = handler.Process();
                if (!result)
                    CLog4Net.LogDebug($"Debug in CMessageProcessor.SafeProcess({handler.MessageId().ToString()}) - Process Function fail");
                return result;
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CMessageProcessor.SafeProcess({handler.MessageId().ToString()}) - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public void SafeCleanUp(IMessageBase handler)
        {
            try
            {
                handler.CleanUp();

            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CMessageProcessor.SafeCleanUp({handler.MessageId().ToString()}) - {ex.Message} - {ex.StackTrace}");
            }
        }

        public void ProcessHandler(in CPacket packet, IMessageBase handler)
        {
            bool result;

            result = SafePrepare(packet, handler);
            if (!result)
            {
                SafeCleanUp(handler);
                return;
            }

            result = SafeProcess(handler);
            if (!result)
            {
                SafeCleanUp(handler);
                return;
            }

            //SafeCleanUp(handler);
        }

        public void Process(CPacket packet)
        {
            try
            {
                var handler = (IMessageBase)new T();
                ProcessHandler(packet, handler);
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CMessageProcessor.Process - {ex.Message} - {ex.StackTrace}");
            }
        }
    }
}
