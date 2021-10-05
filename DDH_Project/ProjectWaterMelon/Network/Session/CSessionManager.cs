using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
// --- custom --- //
using static ConstModule.GSocketState;
// -------------- //

namespace ProjectWaterMelon.Network.Session
{
    // 전역으로 관리되는 세션매니저 클래스
    // *서버-클라 정상연결 시 세션이 생성되고 이를 관리하는 매니저 클래스, 세션의 접속상태 또한 관리한다 
    static class CSessionManager
    {
        // 세션 매니저에서 관리하는 세션아이디 
        private static long mSessionId;
        // 세션 컨테이너 - 스레드 세이프한 Dictionary 사용 (key = 세션 아이디, value = 세션 객체)
        private static ConcurrentDictionary<long, CSession> mSessionContainer = new ConcurrentDictionary<long, CSession>();
        // 해당 클래스에서 관리하는 대상에 접근하는 다중 스레드의 동시성제어를 위한 Lock 객체 
        private static object mSessionLock = new object();

        // 세션객체를 관리 컨테이너에 추가하는 함수
        // thread-safe
        public static void Add(ref CSession user_session)
        {
            var local_user_session = user_session;
            if (local_user_session != null)
            {
                mSessionId = Interlocked.Increment(ref mSessionId);
                lock(mSessionContainer)
                {
                    user_session.SetSessionID(mSessionId);
                }
                mSessionContainer.TryAdd(mSessionId, user_session);
            }
        }

        // 세션 아이디를 이용하여 대상 세션객체를 관리 컨테이너에서 삭제하는 함수
        // thread-safe
        public static bool Remove(long session_id)
        {
            var local_user_session = GetSessionByKey(session_id);
            return mSessionContainer.TryRemove(session_id, out local_user_session);
        }

        // 세션 아이디를 이용하여 대상 세션 객체를 가져오는 함수
        // thread-safe
        public static CSession GetSessionByKey(long session_id)
        {
            CSession local_user_session;
            if (mSessionContainer.TryGetValue(session_id, out local_user_session))
            {
                return local_user_session;
            }
            else
            {
                return null;
            }
        }

        public static bool IsExist(long session_id)
        {
            lock(mSessionLock)
            {
                return mSessionContainer.ContainsKey(session_id);
            }
        }

        // 현재 세션객체 총 갯수 반환 함수
        // thread-safe
        public static int Count()
        {
            return mSessionContainer.Count;
        }

        // 별도의 스레드에서 체크하는 세션 상태
        // 세션이 끊어져있다면 reconnect 진행, reconnect 시행횟수가 특정 수치를 넘어갈경우 socket disconnect 진행 
        public static void CheckSessionState()
        {
            foreach(var state in mSessionContainer)
            {
                var lSocket = state.Value.mTcpSocket;
                if (lSocket != null)
                {
                    if (lSocket.mSocketState != (int)eSocketState.CONNECTED)
                    {
                        lSocket.ReconnectTimer();
                    }
                }
            }
        }
        
    }
}
