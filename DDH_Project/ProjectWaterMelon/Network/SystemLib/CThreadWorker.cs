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
    /// <summary>
    /// ThreadPool 관리 클래스
    /// accept와 같이 한 스레드에서 계속 작업되는 대상은 Thread를 직접할당하여 진행
    /// 짧은 작업은 ThreadPool을 사용하여 진행
    /// </summary>
    class CThreadPoolManager
    {
        public int mMinThreadCount { get; private set; }
        public int mMaxThreadCount { get; private set; }

        /// <summary>
        /// ThreadPoolManager에서 관리되는 스레드 아이디
        /// </summary>
        private static int mThreadNum = 1;

        private object mLockObj = new object();
        private Thread mThreadSendQ;

        // 하트비트 체크 
        private CHeartbeatManager mHearbeatMng = new CHeartbeatManager();

        private Dictionary<int, CThreadBase> mThreadInfo = new Dictionary<int, CThreadBase>();

        public CThreadPoolManager() { }

        public CThreadPoolManager(int minThreadCount, int maxThreadCount)
        {
            mMinThreadCount = minThreadCount;
            mMaxThreadCount = maxThreadCount > Environment.ProcessorCount ? Environment.ProcessorCount : maxThreadCount;
        }

        private bool CheckThreadCountOver()
        {
            return mThreadNum > mMaxThreadCount;
        }

        /// <summary>
        /// ThreadPoolManager에서 관리하는 스레드 객체가 몇개 안되므로 메서드에서 생성       
        /// </summary>
        /// <param name="work">파라미터 및 반환 값이 없는 메서드</param>
        /// <param name="threadName"></param>
        /// <param name="isBackground">true => 메인스레드 종료시 같이 종료, false(foreground) => 메인스레드 종료와 무관하게 작동</param>
        /// <returns></returns>
        public Task<bool> SetupThreadInfo(Action work, string threadName, bool isBackground)
        {
            if (this.CheckThreadCountOver())
                return Task.FromResult(false);

            Thread thread = new Thread(new ThreadStart(work));
            CThreadBase threadInfo = new CThreadBase(thread.ManagedThreadId, threadName, isBackground, thread);

            mThreadInfo.Add(mThreadNum, threadInfo);

            ++mThreadNum;

            return Task.FromResult(true);
        }

        public void StartAllThread()
        {
            foreach(var threadObj in mThreadInfo)
            {
                threadObj.Value.thread.Start();
                threadObj.Value.SetState(true);
            }
        }

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
