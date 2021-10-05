using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
// --- custom --- //
// -------------- //

namespace ProjectWaterMelon.Network.Session
{
    class CSessionManager
    {
        // 세션 매니저에서 관리하는 세션아이디 
        private static long mSessionId;
        // 세션 컨테이너 - 스레드 세이프한 Dictionary 사용 (key = 세션 아이디, value = 세션 객체)
        private ConcurrentDictionary<long, CSession> mSessionList = new ConcurrentDictionary<long, CSession>();
        // 해당 클래스에서 관리하는 대상에 접근하는 다중 스레드의 동시성제어를 위한 Lock 객체 
        private object mSessionLock = new object();
        public CSessionManager()
        {     
        }

        // 세션객체를 관리 컨테이너에 추가하는 함수
        // thread-safe
        public void Add(ref CSession user_session)
        {
            var local_user_session = user_session;
            if (local_user_session != null)
            {
                mSessionId = Interlocked.Increment(ref mSessionId);
                lock(mSessionLock){
                    user_session.SetSessionID(mSessionId);
                }
                mSessionList.TryAdd(mSessionId, user_session);
            }
        }

        // 세션 아이디를 이용하여 대상 세션객체를 관리 컨테이너에서 삭제하는 함수
        // thread-safe
        public bool Remove(long session_id)
        {
            var local_user_session = GetSessionByKey(session_id);
            return mSessionList.TryRemove(session_id, out local_user_session);
        }

        // 세션 아이디를 이용하여 대상 세션 객체를 가져오는 함수
        // thread-safe
        public CSession GetSessionByKey(long session_id)
        {
            CSession local_user_session;
            if (mSessionList.TryGetValue(session_id, out local_user_session))
            {
                return local_user_session;
            }
            else
            {
                return null;
            }
        }

        public bool IsExist(long session_id)
        {
            lock(mSessionLock)
            {
                return mSessionList.ContainsKey(session_id);
            }
        }

        // 현재 세션객체 총 갯수 반환 함수
        // thread-safe
        public int Count()
        {
            return mSessionList.Count;
        }
        
    }
}
