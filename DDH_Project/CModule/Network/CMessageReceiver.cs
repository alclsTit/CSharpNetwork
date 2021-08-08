using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using ConstModule;
using System.Collections.Concurrent;
using static ConstModule.ConstDefine;

namespace CModule.Network
{
    class CMessageResolver
    {
        public delegate void OnReceiveCallback(CPacket Packet);

        int mReadMsgPos;
        int mRemainBytes;                                                               // 수신된 패킷에서 읽어야될 나머지 데이터 사이즈 
        int mMessageSize;                                                               // 패킷 메시지 사이즈 
        int mMessageType;                                                               // 패킷 메시지 타입

        byte[] mMessageBuffer = new byte[MAX_BUFFER_SIZE];                              // 메시지를 담아둘 수 있는 버퍼(버퍼 매니저의 Chunk)

        public bool OnReadUntil(byte[] Buffer, ref int Offset, int PosToRead)
        {
            if (mRemainBytes < 0)
                return false;

            if (PosToRead > mRemainBytes)
                mRemainBytes = PosToRead;

            Array.Copy(Buffer, Offset, mMessageBuffer, mReadMsgPos, PosToRead);

            mReadMsgPos += PosToRead;
            Offset += PosToRead;
            mRemainBytes -= PosToRead;

            if (mReadMsgPos < PosToRead)
                return false;

            return true;
        }

        public void OnReceive(byte[] Buffer, int Offset, int ByteTransferred, OnReceiveCallback OnMsgCompleted)
        {
            try
            {
                // 클라에서 서버로 수신된 패킷 사이즈 (처리해야할 패킷 메시지 양)
                // 총 패킷 사이즈 = 100(mMessageSize), 수신된 데이터 크긱 = 80
                mRemainBytes = ByteTransferred;

                while (mRemainBytes > 0)
                {
                    var lCompleted = false;

                    // 읽은 패킷의 헤더(패킷 사이즈, 패킷 타입 포함)를 읽지 못한경우 이를 읽는다, 헤더를 먼저 읽어 패킷 사이즈 확인
                    if (mReadMsgPos < MAX_PACKET_HEADER_SIZE)
                    {
                        //Offset: 현재 수신버퍼에서 읽은 패킷 읽은 패킷 첫 위치                  
                        lCompleted = OnReadUntil(Buffer, ref Offset, MAX_PACKET_HEADER_SIZE);
                        if (!lCompleted)
                            return;

                        //mMessageSize: 헤더에 저장된 전체 패킷 사이즈
                        mMessageSize = GetMessageBodySize();
                        if (mMessageSize <= 0)
                            return;
                    }

                    // 패킷 타입 읽기
                    if (mReadMsgPos >= MAX_PACKET_HEADER_SIZE && mReadMsgPos < MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE)
                    {
                        lCompleted = OnReadUntil(Buffer, ref Offset, MAX_PACKET_TYPE_SIZE);
                        if (!lCompleted)
                            return;

                        mMessageType = GetMessageType();
                        if (mMessageType <= 0)
                            return;
                    }

                    // 패킷 데이터를 읽는다
                    lCompleted = OnReadUntil(Buffer, ref Offset, mMessageSize - MAX_PACKET_HEADER_SIZE - MAX_PACKET_TYPE_SIZE);
                    if (!lCompleted)
                        return;
                }

                if (mRemainBytes == 0)
                {
                    // 데이터를 모두 받았으면 이를 이용해서 패킷으로 만든다
                    CPacket lPacket = new CPacket(mMessageBuffer, mMessageSize, mMessageType);
                    OnMsgCompleted(lPacket);
                    ClearBuffer();
                }
                else
                {
                    CLog4Net.LogError($"Exception in CMessageResolver.OnReceive - Packet size error!!![RemainBytes = {mRemainBytes}]");
                }
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CMessageResolver.OnReceive - {ex.Message}, {ex.StackTrace}");
            }
        }

        public void ProcessPacket(CPacket packet)
        {
            try
            {
                switch (packet.m_type)
                {
                    case (int)Protocol.PacketId.req_test_packet:
                        //packet.DeSerializePacket<Protocol.msg_test.handler_req_test_packet_user2game>(packet.m_buffer);
                        break;
                    case (int)Protocol.PacketId.ack_test_packet:
                        break;
                    case (int)Protocol.PacketId.notify_test_packet:
                        break;
                    default:
                        CLog4Net.LogError($"Error in CMessageResolver.ProcessPacket - Packet Type Error({packet.m_type})");
                        break;
                }
                Console.WriteLine($"=========TEST PACKET => {packet.m_type}=========");
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CMessageResolver.ProcessPacket - {ex.Message},{ex.StackTrace}");
            }
        }

        public int GetMessageBodySize()
        {
            var type = MAX_PACKET_HEADER_SIZE.GetType();
            if (type.Equals(typeof(Int16)))
                return BitConverter.ToInt16(mMessageBuffer, 0);

            return BitConverter.ToInt32(mMessageBuffer, 0);
        }

        public int GetMessageType()
        {
            var type = MAX_PACKET_TYPE_SIZE.GetType();
            if (type.Equals(typeof(Int16)))
                return BitConverter.ToInt16(mMessageBuffer, MAX_PACKET_HEADER_SIZE);

            return (BitConverter.ToInt32(mMessageBuffer, MAX_PACKET_HEADER_SIZE));
        }

        public byte[] GetMessageBuffer()
        {
            return mMessageBuffer;
        }

        public void ClearBuffer()
        {
            Array.Clear(mMessageBuffer, 0, mMessageBuffer.Length);
            mRemainBytes = 0;
            mReadMsgPos = 0;
            mMessageSize = 0;
            mMessageType = -1;
        }
    }
}
