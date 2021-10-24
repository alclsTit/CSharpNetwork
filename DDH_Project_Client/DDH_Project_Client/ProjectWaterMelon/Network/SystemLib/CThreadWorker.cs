using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ProjectWaterMelon.Network.MessageWorker;
using ProjectWaterMelon.Network.Session;

namespace ProjectWaterMelon.Network.SystemLib
{
    class CThreadWorker
    {
        private object mLockObj = new object();
        private Thread mThreadSendQ;

        public CThreadWorker() { }

        public void Start()
        {
            // SendQ 는 스레드 1개 할당 
            mThreadSendQ = new Thread(ProcessSendQ);
            mThreadSendQ.Start();
        }

        public void ProcessSendQ()
        {
            while(true)
            {
                // 단일 클라이언트 관리
                foreach(var session in CSessionManager.GetSessionSeq<CSession>())
                {
                    foreach (var packet in session.mTcpSocket.GetPacketSendQ())
                    {
                        // 해당 세션에 연관된 모든 SendQ 대상 send 진행 
                        if (!packet.mSending)
                            packet.mTcpSocket.StartSend(packet);
                    }
                }
            }
        }
    }
}
