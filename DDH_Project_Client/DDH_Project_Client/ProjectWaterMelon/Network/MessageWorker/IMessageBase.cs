using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Packet;
// -------------- //

namespace ProjectWaterMelon.Network.MessageWorker
{
    // 프로토콜에 정의된 패킷들이 상속하여 구현할 대상 
    public interface IMessageBase
    {
        Protocol.PacketId MessageId();
        // 데이터 세팅
        bool Prepare(in CPacket packet);

        // 송,수신 받은 패킷 내용 처리 
        bool Process();

        // 패킷 익셉션 처리 
        void CleanUp();

    }
}
