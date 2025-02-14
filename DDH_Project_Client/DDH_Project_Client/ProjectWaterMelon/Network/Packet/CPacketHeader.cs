﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
// --- custom --- //
using ProjectWaterMelon.Log;
using ProtoBuf;
using static ProjectWaterMelon.ConstDefine;
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
        [ProtoMember(5)]
        public bool mDirectFlag { get; private set; }

        public CPacketHeader() { }

        // 더미 생성자 
        public CPacketHeader(int LenOfMsgBuff, bool setTick, bool dummyFlag, bool directFlag, Protocol.PacketId msgid = Protocol.PacketId.notify_nohandled_packet)
        {
            mMessageId = msgid;
            mHeaderSize = Marshal.SizeOf(this);
            mTotalSize = MAX_PACKET_HEADER_SIZE + mHeaderSize + LenOfMsgBuff;
            mDirectFlag = directFlag;

            if (setTick) SetProcessTickCount();
            if (!dummyFlag) Init(LenOfMsgBuff, setTick, directFlag, msgid);
        }

        // HeaderSize 때문에 CPacketHeader 클래스를 한번 생성자초기화 시킨다음에 이를 시리얼라이징해서 실제사용할 헤더사이즈를 가져온다.
        // 메시지 버퍼 사이즈와 헤더사이즈를 더해서 실제 사용할 버퍼에 담길 패킷사이즈를 저장한다.
        // 외부에서 CPacketHeader 생성자 호출로 더미세팅 이후 Init을 호출하여야 한다.
        public void Init(int LenOfMsgBuff, bool setTick, bool directFlag, Protocol.PacketId msgid = Protocol.PacketId.notify_nohandled_packet)
        {
            var lPacketHeader = new CPacketHeader(LenOfMsgBuff, setTick, true, directFlag, msgid);
            mMessageId = msgid;
            mHeaderSize = CProtobuf.ProtobufSerialize<CPacketHeader>(lPacketHeader).Length;
            mTotalSize = MAX_PACKET_HEADER_SIZE + mHeaderSize + LenOfMsgBuff;

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
