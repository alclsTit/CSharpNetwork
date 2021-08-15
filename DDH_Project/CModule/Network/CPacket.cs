using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ConstModule;

/*
 * 패킷은 워커 스레드에서 처리한다
 * 패킷은 자주 사용되는 객체, 재활용을 해야된다
 * 패킷은 큐에 넣고 큐처리해야한다.
 */
namespace CModule.Network
{
    // 패킷 = [헤더][데이터]
    // [헤더] = 패킷 사이즈, 패킷 타입 [데이터] = 패킷 본문 
    // 패킷 암호화도 필요
    [Serializable]
    public class CPacket
    {
        private readonly int MAX_PACKET_HEADER_SIZE = 4;
        private readonly int MAX_PACKET_TYPE_SIZE = 4;
        private readonly uint MAX_PACKET_ALLOC_SIZE = 8 * 1024 * 1024;

        // 패킷 데이터(바디)
        public byte[] m_buffer { get; private set; }
        // 패킷 데이터 사이즈(헤더)
        public int m_size { get; private set; }
        // 패킷 타입(헤더)
        public int m_type { get; private set; }
        // 패킷 메시지 바디 사이즈
        public int mBodySize { get; private set; }
        public int m_position { get; private set; }

        public CPacket() { }

        public CPacket(int packet_size = ConstDefine.MAX_BUFFER_SIZE)
        {
            m_buffer = new byte[packet_size];
            m_size = m_buffer.Length;
            m_type = 0;
        }

        public CPacket(byte[] buffer, int size, int type)
        {
            m_buffer = buffer;
            m_size = size;
            m_type = type;
        }

        // check packet validate
        public bool CheckValidate()
        {
            if (m_size > MAX_PACKET_ALLOC_SIZE)
                return false;

            if (m_size < m_buffer.Length)
                return false;

            return true;
        }


        /*
         public byte[] GetData()
        {
            // 패킷 전체 사이즈 
            var lHeaderSize = m_buffer.Length;
            // 헤더 (1.패킷 사이즈)
            byte[] lPHeaderBytesArray = BitConverter.GetBytes(lHeaderSize);
            // 헤더 (2.패킷 타입)
            byte[] lPTypeBytesArray = BitConverter.GetBytes(m_type);
            // 패킷 (패킷 사이즈 + 패킷 타입 + 데이터)
            byte[] lPDataBytesAry = new byte[lPHeaderBytesArray.Length + lPTypeBytesArray.Length + m_buffer.Length];

            // 헤더(패킷 사이즈) 값 채우기 
            Array.Copy(lPHeaderBytesArray, 0, lPDataBytesAry, 0, lPHeaderBytesArray.Length);

            // 헤더(패킷 타입) 값 채우기
            Array.Copy(lPHeaderBytesArray, 0, lPDataBytesAry, lPHeaderBytesArray.Length, lPTypeBytesArray.Length);

            // 바디 값 채우기 
            Array.Copy(m_buffer, 0, lPDataBytesAry, lPHeaderBytesArray.Length + lPTypeBytesArray.Length, m_buffer.Length);

            return lPDataBytesAry;
        }

        public void CopyPacketData(CPacket Packet)
        {
            Array.Copy(Packet.m_buffer, 0, m_buffer, 0, Packet.m_size);
        }

        public void SetPacketSize()
        {
            var lHeaderSize = m_buffer.Length;
            var lPHeaderBytesArray = BitConverter.GetBytes(lHeaderSize);
            Array.Copy(lPHeaderBytesArray, 0, m_buffer, 0, lHeaderSize);
        }
      
        */

        // Convert m_type of struct To byte array (Marshaling)
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

                this.m_buffer = new byte[packet_size];
                Buffer.BlockCopy(BitConverter.GetBytes(packet_size), 0, this.m_buffer, 0, MAX_PACKET_HEADER_SIZE);
                Buffer.BlockCopy(BitConverter.GetBytes(PacketType), 0, this.m_buffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
                Buffer.BlockCopy(msg_byteArray, 0, this.m_buffer, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, msg_byteArray.Length);

                this.m_size = packet_size;
                this.m_type = PacketType;
            }
        }

        public void BuildPacketClass<T>(ref T data, int packetType) where T : class
        {
            var lByteArray = SerializeClassToByteArray(ref data);
            if (lByteArray != null)
            {
                var lBodySize = lByteArray.Length;
                var lPacketSize = MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE + lBodySize;

                this.m_buffer = new byte[lPacketSize];
                Buffer.BlockCopy(BitConverter.GetBytes(lPacketSize), 0, m_buffer, 0, MAX_PACKET_HEADER_SIZE);
                Buffer.BlockCopy(BitConverter.GetBytes(packetType), 0, m_buffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
                Buffer.BlockCopy(lByteArray, 0, m_buffer, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, lBodySize);

                m_size = lPacketSize;
                m_type = packetType;
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

                this.m_buffer = new byte[lPacketSize];
                Buffer.BlockCopy(BitConverter.GetBytes(lPacketSize), 0, m_buffer, 0, MAX_PACKET_HEADER_SIZE);
                Buffer.BlockCopy(BitConverter.GetBytes(packetType), 0, m_buffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
                Buffer.BlockCopy(lByteArray, 0, m_buffer, lHeaderSize, lBodySize);

                m_size = lPacketSize;
                m_type = packetType;
                mBodySize = lBodySize;
            }
        }

        private byte[] SerializeClassToByteArrayProtoBuf<T>(in T data) where T : class
        {
            byte[] result = null;
            if (data != null)
            {
                try
                {
                    using(var lMemoryStream = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize<T>(lMemoryStream, data);
                        result = lMemoryStream.ToArray();

                        //IntelCPU => 리틀엔디안, 네트워크 패킷 통신 => 빅엔디안 
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(result);

                        return result;
                    }
                }
                catch(Exception ex)
                {
                    CLog4Net.LogError($"Exception in CPacket.SerializeClassToByteArrayProtoBuf({nameof(T)}) - {ex.Message},{ex.StackTrace}");
                }
            }
            return result;
        }

        public T DeSerializeByteArrayToClassProtoBuf<T>(in byte[] data) where T : class
        {
            T result = null;
            if (data != null)
            {
                try
                {
                    using(var lMemoryStream = new MemoryStream())
                    {
                        result = ProtoBuf.Serializer.Deserialize<T>(lMemoryStream);
                        return result;
                    }
                }
                catch(Exception ex)
                {
                    CLog4Net.LogError($"Exception in CPacket.DeSerializeByteArrayToClassProtoBuf({nameof(T)}) - {ex.Message},{ex.StackTrace}");
                }
            }
            return result;
        }
        

        // 사용한 패킷 반환
        public void Destroy()
        {

        }

        /*
       // Convert byte array To m_type of struct (Marshaling)
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
       public void DeSerializePacket<T>(T data, int m_type) where T : struct
       {
           var lPBodyByteArray = SerializeStructToByteArray(data);
           var lPHeaderSize = MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE + lPBodyByteArray.Length;
           var lPHeaderSizeArray = BitConverter.GetBytes(lPHeaderSize);
           var lPHeaderTypeArray = BitConverter.GetBytes(m_type);

           this.m_buffer = new byte[lPHeaderSize];
           Array.Copy(lPHeaderSizeArray, 0, this.m_buffer, 0, MAX_PACKET_HEADER_SIZE);
           Array.Copy(lPHeaderTypeArray, 0, this.m_buffer, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
           Array.Copy(lPBodyByteArray, 0, this.m_buffer, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, lPBodyByteArray.Length);

           this.m_size = lPHeaderSize;
           this.m_type = m_type;
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
    }
}
