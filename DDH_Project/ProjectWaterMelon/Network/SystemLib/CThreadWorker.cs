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

        // 하트비트 체크 
        private CHeartbeatManager mHearbeatMng = new CHeartbeatManager();

        public CThreadWorker() { }

        public void Start()
        {
            // SendQ 는 스레드 1개 할당 
            mThreadSendQ = new Thread(ProcessSendQ);
            mThreadSendQ.Start();

            // 모니터링 체크 on
            mHearbeatMng.MoniterTimerOn();
        }


        public void ProcessSendQ()
        {
            while(true)
            {
                // 서버에 접속한 모든 세션대상 관리
                foreach (var session in CSessionManager.GetSessionSeq<CSession>())
                {
                    foreach(var packet in session.mTcpSocket.GetPacketSendQ())
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
