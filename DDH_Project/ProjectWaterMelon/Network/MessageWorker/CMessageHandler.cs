using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Network.CustomSocket;
// -------------- //

namespace ProjectWaterMelon.Network.MessageWorker
{
    public class CMessageHandler
    {
        public Protocol.PacketId mMessageId { get; private set; }
        public CTcpSocket mSocket { get; private set; }
        public CPacket mPacket { get; private set; }

        public CMessageHandler(Protocol.PacketId _MessageId)
        {
            mMessageId = _MessageId;
        }
        public Protocol.PacketId MessageId()
        {
            return mMessageId;
        }

        public bool Prepare(in CPacket _Packet)
        {
            mSocket = _Packet.mTcpSocket;
            mPacket = _Packet;

            return true;
        }

        public virtual bool Process()
        {
            return true;
        }

        public virtual void CleanUp()
        {
        }

        public void Relay(Protocol.PacketId msgid, IMessageBase handler)
        {
            //mSocket?.AsyncSend<IMessageBase>(mMessageId, message);
        }
    }
}
