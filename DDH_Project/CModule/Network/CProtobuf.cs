using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static ConstModule.ConstDefine;

namespace CModule.Network
{
    // protobuf 를 활용한 시리얼라이즈, 디시리얼라이즈 전역 클래스 
    static class CProtobuf
    {
        public static byte[] BuildPacket<T>(T data, int type) where T: class
        {
            if (data == null)
                return null;

            var lMsgBuffer = ProtobufSerialize<T>(data);
            var result = new byte[lMsgBuffer.Length + MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE];

            System.Buffer.BlockCopy(BitConverter.GetBytes(lMsgBuffer.Length), 0, result, 0, MAX_PACKET_HEADER_SIZE);
            System.Buffer.BlockCopy(BitConverter.GetBytes(type), 0, result, MAX_PACKET_HEADER_SIZE, MAX_PACKET_TYPE_SIZE);
            System.Buffer.BlockCopy(lMsgBuffer, 0, result, MAX_PACKET_HEADER_SIZE + MAX_PACKET_TYPE_SIZE, lMsgBuffer.Length);

            return result;
        }

        //IntelCPU => 리틀엔디안, 네트워크 패킷 통신 => 빅엔디안 (*해당 처리 별도로 필요없음)
        private static byte[] ProtobufSerialize<T>(T data) where T : class
        {
            if (data == null) return null;

            try
            {
                using (var lMemoryStream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(lMemoryStream, data);
                    return lMemoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CProtobuf.ProtobufSerialize({nameof(T)}) - {ex.Message},{ex.StackTrace}");
                return null;
            }
        }

        public static T ProtobufDeserialize<T>(byte[] data) where T : class
        {
            if (data == null) return null;

            try
            {
                using (var lMemoryStream = new MemoryStream(data))
                {
                    return ProtoBuf.Serializer.Deserialize<T>(lMemoryStream);
                }
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CProtobuf.ProtobufDeserialize({nameof(T)}) - {ex.Message},{ex.StackTrace}");
                return null;
            }
        }
    }
}
