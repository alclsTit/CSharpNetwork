using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
// --- custom --- //
using ProjectWaterMelon.Network.CustomSocket;
using ProjectWaterMelon.Log;
using static ProjectWaterMelon.ConstDefine;
// ------------- //

/*
 * 패킷은 워커 스레드에서 처리한다
 * 패킷은 자주 사용되는 객체, 재활용을 해야된다
 * 패킷은 큐에 넣고 큐처리해야한다.
 */
namespace ProjectWaterMelon.Network.Packet
{
    // 패킷 = [헤더][데이터]
    // [헤더] = 패킷 사이즈, 패킷 타입 [데이터] = 패킷 본문 
    // 패킷 암호화도 필요
    [Serializable]
    public class CPacket
    {
        // 패킷 데이터(바디)
        public byte[] mMsgBuffer { get; private set; }
        // 패킷 데이터(헤더)
        public byte[] mMsgHeaderBuffer { get; private set; }
        public CTcpSocket mTcpSocket { get; private set; }
        public CPacketHeader mPacketHeader { get; private set; }

        public CPacket()
        {
            //mMsgBuffer = new byte[ConstDefine.MAX_BUFFER_SIZE];
            //mPacketHeader = new CPacketHeader(mMsgBuffer.Length, false);
        }

        // 패킷 수신받았을 때 디시리얼라이징된 대상으로 세팅되는 패킷생성자
        public CPacket(in CTcpSocket _TcpSocket, in byte[] _HeaderBuffer, in byte[] _BodyBuffer)
        {
            mTcpSocket = _TcpSocket;
            mMsgHeaderBuffer = _HeaderBuffer;
            mMsgBuffer = _BodyBuffer;
            mPacketHeader = CProtobuf.ProtobufDeserialize<CPacketHeader>(_HeaderBuffer);
        }

        // 패킷 송,수신 시 호출되는 CPacket 생성자 
        public CPacket(in CTcpSocket _TcpSocket, in byte[] _MsgBuffer, Protocol.PacketId msgid)
        {
            mTcpSocket = _TcpSocket;
            mMsgBuffer = _MsgBuffer;
            mPacketHeader = new CPacketHeader(_MsgBuffer.Length, true, false, msgid);
            mMsgHeaderBuffer = CProtobuf.ProtobufSerialize<CPacketHeader>(mPacketHeader);    
        }

        // check packet validate
        public bool CheckValidate()
        {
            if (mMsgBuffer.Length > MAX_BUFFER_SIZE)
            {
                CLog4Net.LogError($"Exception in CPacket.CheckValidate - Size Of PacketBody error");
                return false;
            }

            return mPacketHeader.CheckValidate();
        }

        private byte[] BuildPacket<T>(T data) where T : class
        {
            mPacketHeader.SetProcessTickCount();

            var lHeaderBuffer = CProtobuf.ProtobufSerialize<CPacketHeader>(mPacketHeader);
            var lBodyBuffer = MessageToBuffer<T>(data);
            var lResultBuffer = new byte[lHeaderBuffer.Length + lBodyBuffer.Length];
            
            Buffer.BlockCopy(lHeaderBuffer, 0, lResultBuffer, 0, lHeaderBuffer.Length);
            Buffer.BlockCopy(lBodyBuffer, 0, lResultBuffer, lHeaderBuffer.Length, lBodyBuffer.Length);

            return lResultBuffer;
        }

        private byte[] MessageToBuffer<T>(T message) where T : class
        {
            if (message == null)
                return null;

            return CProtobuf.ProtobufSerialize<T>(message);
        }

        public T BufferToMessage<T>(byte[] msgbuffer) where T : class
        {
            if (msgbuffer == null)
                return null;

            return CProtobuf.ProtobufDeserialize<T>(msgbuffer);
        }
        public Protocol.PacketId GetMessageId()
        {
            return mPacketHeader.mMessageId;
        }

        // 패킷 헤더 버퍼사이즈
        public int GetHeaderSize()
        {
            return mPacketHeader.mHeaderSize;
        }

        // 패킷 바디 버퍼사이즈
        public int GetBodySize()
        {
            return mMsgBuffer.Length;
        }

        // 패킷 헤더사이즈 + 패킷 헤더 + 바디 버퍼사이즈 
        public int GetTotalSize()
        {
            return mPacketHeader.mTotalSize;
        }

        // 사용한 패킷 반환
        public void Destroy()
        {

        }

        #region 주석코드[테스트 코드]
        /*
         public byte[] GetData()
        {
            // 패킷 전체 사이즈 
            var lHeaderSize = mMsgBuffer.Length;
            // 헤더 (1.패킷 사이즈)
            byte[] lPHeaderBytesArray = BitConverter.GetBytes(lHeaderSize);
            // 헤더 (2.패킷 타입)
            byte[] lPTypeBytesArray = BitConverter.GetBytes(mType);
            // 패킷 (패킷 사이즈 + 패킷 타입 + 데이터)
            byte[] lPDataBytesAry = new byte[lPHeaderBytesArray.Length + lPTypeBytesArray.Length + mMsgBuffer.Length];

            // 헤더(패킷 사이즈) 값 채우기 
            Array.Copy(lPHeaderBytesArray, 0, lPDataBytesAry, 0, lPHeaderBytesArray.Length);

            // 헤더(패킷 타입) 값 채우기
            Array.Copy(lPHeaderBytesArray, 0, lPDataBytesAry, lPHeaderBytesArray.Length, lPTypeBytesArray.Length);

            // 바디 값 채우기 
            Array.Copy(mMsgBuffer, 0, lPDataBytesAry, lPHeaderBytesArray.Length + lPTypeBytesArray.Length, mMsgBuffer.Length);

            return lPDataBytesAry;
        }

        public void CopyPacketData(CPacket Packet)
        {
            Array.Copy(Packet.mMsgBuffer, 0, mMsgBuffer, 0, Packet.mSize);
        }

        public void SetPacketSize()
        {
            var lHeaderSize = mMsgBuffer.Length;
            var lPHeaderBytesArray = BitConverter.GetBytes(lHeaderSize);
            Array.Copy(lPHeaderBytesArray, 0, mMsgBuffer, 0, lHeaderSize);
        }
      
       

        // Convert mType of struct To byte array (Marshaling)
        // 1.Set structure data -> 2. Serialize -> 3. buildPacket
        public byte[] SerializeStructToByteArray<T>(T data) where T : struct
        {
            try
            {
                var packet_size = Marshal.SizeOf(data);
                byte[] tmpArray = new byte[packet_size];

                IntPtr tmpPtr = Marshal.AllocHGlobal(packet_size);
                Marshal.StructureToPtr(data, tmpPtr, false);
                Marshal.Copy(tmpPtr, tmpArray, 0, packet_size);
                Marshal.FreeHGlobal(tmpPtr);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(tmpArray);

                return tmpArray;
            }
            catch(Exception ex)
            {
                CLog4Net.LogError($"Exception in CPacket.SerializeStructToByteArray - {ex.Message},{ex.StackTrace}");
                return null;
            }
        }

        public void BuildPacketStruct<T>(T data, int PacketType) where T : struct
        {
            var msg_byteArray = SerializeStructToByteArray(data);
            if (msg_byteArray != null)
            {
                var packet_size = MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE + msg_byteArray.Length;

                this.mMsgBuffer = new byte[packet_size];
                Buffer.BlockCopy(BitConverter.GetBytes(packet_size), 0, this.mMsgBuffer, 0, MAX_PACKET_HEADER_SIZE);
                Buffer.BlockCopy(BitConverter.GetBytes(PacketType), 0, this.mMsgBuffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
                Buffer.BlockCopy(msg_byteArray, 0, this.mMsgBuffer, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, msg_byteArray.Length);

                this.mSize = packet_size;
                this.mType = PacketType;
            }
        }

        public void BuildPacketClass<T>(ref T data, int packetType) where T : class
        {
            var lByteArray = SerializeClassToByteArray(ref data);
            if (lByteArray != null)
            {
                var lBodySize = lByteArray.Length;
                var lPacketSize = MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE + lBodySize;

                this.mMsgBuffer = new byte[lPacketSize];
                Buffer.BlockCopy(BitConverter.GetBytes(lPacketSize), 0, mMsgBuffer, 0, MAX_PACKET_HEADER_SIZE);
                Buffer.BlockCopy(BitConverter.GetBytes(packetType), 0, mMsgBuffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
                Buffer.BlockCopy(lByteArray, 0, mMsgBuffer, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, lBodySize);

                mSize = lPacketSize;
                mType = packetType;
                mBodySize = lBodySize;
            }
        }

        public byte[] SerializeClassToByteArray<T>(ref T data) where T : class
        {
            byte[] result = null;
            try
            {
                var lBinaryFormatter = new BinaryFormatter();
                using (var lMemoryStream = new MemoryStream())
                {
                    lBinaryFormatter.Serialize(lMemoryStream, data);
      
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(lMemoryStream.ToArray());

                    result = lMemoryStream.ToArray();
                }
            }
            catch(Exception ex)
            {
                CLog4Net.gLog4Net.ErrorFormat($"Exception in CPacket.SerializeClassToByteArray - {ex.Message},{ex.StackTrace}");
            }
            return result;
        }

        public T DeSerializeByteArrayToClass<T>(in byte[] data) where T : class
        {
            T result = null;
            try
            {
                using(var lMemoryStream = new MemoryStream())
                {
                    lMemoryStream.Write(data, 0, data.Length);
                    lMemoryStream.Position = 0;
                    
                    var lBinaryFormatter = new BinaryFormatter();
                    result = lBinaryFormatter.Deserialize(lMemoryStream) as T;
                }
            }
            catch(Exception ex)
            {
                CLog4Net.gLog4Net.ErrorFormat($"Exception in CPacket.DeSerializeByteArrayToClass - {ex.Message},{ex.StackTrace}");
            }
            return result;
        }

        public void BuildPacketClassProtoBuf<T>(ref T data, int packetType) where T : class
        {
            var lByteArray = SerializeClassToByteArrayProtoBuf(data);
            if (lByteArray != null)
            {
                var lHeaderSize = MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE;
                var lBodySize = lByteArray.Length;
                var lPacketSize = lHeaderSize + lBodySize;

                this.mMsgBuffer = new byte[lPacketSize];
                Buffer.BlockCopy(BitConverter.GetBytes(lPacketSize), 0, mMsgBuffer, 0, MAX_PACKET_HEADER_SIZE);
                Buffer.BlockCopy(BitConverter.GetBytes(packetType), 0, mMsgBuffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
                Buffer.BlockCopy(lByteArray, 0, mMsgBuffer, lHeaderSize, lBodySize);

                mSize = lPacketSize;
                mType = packetType;
                mBodySize = lBodySize;
            }
        }

       // Convert byte array To mType of struct (Marshaling)
       public static T DeSerializeByteArrayToStruct<T>(byte[] data) where T : struct
       {
           T tmpStruct = new T();

           var packet_size = Marshal.SizeOf(tmpStruct);
           IntPtr tmpPtr = Marshal.AllocHGlobal(packet_size);

           Marshal.Copy(data, 0, tmpPtr, packet_size);

           tmpStruct = (T)Marshal.PtrToStructure(tmpPtr, tmpStruct.GetType());
           Marshal.FreeHGlobal(tmpPtr);

           return tmpStruct;
       }

       // Packet(struct) to byteArray
       public void DeSerializePacket<T>(T data, int mType) where T : struct
       {
           var lPBodyByteArray = SerializeStructToByteArray(data);
           var lPHeaderSize = MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE + lPBodyByteArray.Length;
           var lPHeaderSizeArray = BitConverter.GetBytes(lPHeaderSize);
           var lPHeaderTypeArray = BitConverter.GetBytes(mType);

           this.mMsgBuffer = new byte[lPHeaderSize];
           Array.Copy(lPHeaderSizeArray, 0, this.mMsgBuffer, 0, MAX_PACKET_HEADER_SIZE);
           Array.Copy(lPHeaderTypeArray, 0, this.mMsgBuffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
           Array.Copy(lPBodyByteArray, 0, this.mMsgBuffer, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, lPBodyByteArray.Length);

           this.mSize = lPHeaderSize;
           this.mType = mType;
       }

       public T DeSerializePacket<T>(byte[] msg) where T : struct
       {
           var packet_size = Marshal.SizeOf(typeof(T));
           byte[] data = new byte[packet_size];
           Array.Copy(msg, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, data, 0, packet_size);

           IntPtr tmpPtr = Marshal.AllocHGlobal(packet_size);
           Marshal.Copy(data, 0, tmpPtr, packet_size);
           T retValue = Marshal.PtrToStructure(tmpPtr, typeof(T)) as T;
           Marshal.FreeHGlobal(tmpPtr);

           return retValue;
       }

       */
        #endregion
    }
}
