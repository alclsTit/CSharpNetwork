using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
// --- custom --- //
using ProjectWaterMelon.Log;
using ProtoBuf;
using static ConstModule.ConstDefine;
// -------------- //

namespace ProjectWaterMelon.Network.Packet
{
    [ProtoContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CPacketHeader
    {
        [ProtoMember(1)]
        public Protocol.PacketId mMessageId { get; private set; }
        [ProtoMember(2)]
        public int mHeaderSize { get; private set; }
        [ProtoMember(3)]
        public int mTotalSize { get; private set; }
        [ProtoMember(4)]
        public long mProcessTickCount { get; private set; }

        public CPacketHeader(Protocol.PacketId msgid = Protocol.PacketId.notify_nohandled_packet)
        {
            mMessageId = msgid;
            mHeaderSize = Marshal.SizeOf(this);
            mTotalSize = mHeaderSize + MAX_BUFFER_SIZE;
        }

        //패킷 송신시 사용하는 생성자
        public CPacketHeader(in byte[] msgbuffer, bool setTick, Protocol.PacketId msgid = Protocol.PacketId.notify_nohandled_packet)
        {
            mMessageId = msgid;
            mHeaderSize = Marshal.SizeOf(this);
            mTotalSize = mHeaderSize + msgbuffer.Length;

            if (setTick) SetProcessTickCount();
        }

        public bool CheckValidate()
        {
            if (mTotalSize > MAX_BUFFER_SIZE)
            {
                CLog4Net.LogError($"Error in CPacketHeader.CheckValidate - Size Of Packet error");
                return false;
            }

            if (mHeaderSize > Marshal.SizeOf(this))
            {
                CLog4Net.LogError($"Error in CPacketHeader.CheckValidate - Size Of PacketHeader error");
                return false;
            }

            if (mHeaderSize > mTotalSize)
            {
                CLog4Net.LogError($"Error in CPacketHeader.CheckValidate - Size Of PacketHeader is bigger than totalsize");
                return false;
            }

            return true;
        }

        public byte[] MessageToBuffer(in CPacketHeader message)
        {
            if (message == null)
                return null;

            return CProtobuf.ProtobufSerialize<CPacketHeader>(message);
        }

        public void BufferToMessage(byte[] msgbuffer)
        {
            if (msgbuffer == null)
                return;

            var lHeaderInfo = CProtobuf.ProtobufDeserialize<CPacketHeader>(msgbuffer);

            mTotalSize = lHeaderInfo.mTotalSize;
            mMessageId = lHeaderInfo.mMessageId;
            mHeaderSize = lHeaderInfo.mHeaderSize;
        }

        public void SetProcessTickCount()
        {
            mProcessTickCount = DateTime.Now.Ticks;
        }

    }
}
