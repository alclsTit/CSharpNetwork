using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace ProjectWaterMelon.Network.Session
{
    // Listen 서버당 1개, 해당 서버에서 세션을 관리하는 매니저 클래스 
    // *서버-클라 정상연결 시 세션이 생성되고 이를 관리하는 매니저 클래스, 세션의 접속상태 또한 관리한다 
    public class CSessionManager<TAppSession> where TAppSession : class, ISessionBase
    {
        /// <summary>
        /// 세션 컨테이너, ThreadSafe Dictionary 사용 (key = 세션 아이디, value = 세션 객체)
        /// </summary>
        //private ConcurrentDictionary<string, TAppSession> mSessionContainer = new ConcurrentDictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, TAppSession> mConcurrentSessionDic = new ConcurrentDictionary<string, TAppSession>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 현재 관리중인 세션 총 갯수 반환
        /// </summary>
        public int GetSessionCount => mConcurrentSessionDic.Count;

        /// <summary>
        /// 요청한 id에 해당하는 값(세션) 반환 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public TAppSession GetSessionByID(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(id);

            if (mConcurrentSessionDic.TryGetValue(id, out TAppSession session))
                return session;
            else
                return default(TAppSession);
        }

        /// <summary>
        /// Session 추가 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool TryAdd(string key, TAppSession session)
        {
            if (key == null)
                throw new ArgumentException(nameof(key));

            if (session == null)
                throw new ArgumentException(nameof(session));

            if (mConcurrentSessionDic.TryAdd(key, session))
                return true;

            return false;
        }

        /// <summary>
        /// key 값에 해당하는 Session 이 있는지 확인
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contain(string key)
        {
            return mConcurrentSessionDic.ContainsKey(key);
        }

        /// <summary>
        /// key 값에 해당하는 session 삭제
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return mConcurrentSessionDic.TryRemove(key, out TAppSession session);
        }

        /// <summary>
        /// key 값에 해당하는 session 삭제 및 값 반환
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RemoveAndGetValue(string key, out TAppSession value)
        {
            if (mConcurrentSessionDic.TryRemove(key, out TAppSession session))
            {
                value = session;
                return true;
            }
            else
            {
                value = default(TAppSession);
                return false;
            }
        }

        /// <summary>
        /// Session 컨테이너 시퀀스 반환, 크기가 큰 경우 시퀀스로 반환하는게 유리(필요할 때 컨테이너에서 항목을 가져오는 방식)
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TAppSession> GetEnumerator()
        {
            var lEnumerator = mConcurrentSessionDic.GetEnumerator();
            while(lEnumerator.MoveNext())
            {
                var value = lEnumerator.Current.Value;

                if (value == null)
                    continue;

                yield return value;
            }
        }

        public void OnClose()
        {
           
        }

    }
}

/*public bool Add(CSessionTest session)
{
    if (mSessionContainer.TryAdd(session.GetSessionID, session))
        return true;

    //TODO: 옵션 true 일 때만 로그찍히도록 설정 
    GCLogger.Error($"{nameof(CSessionManager)}", "Add", $"{session.GetSessionID} was not added session container successfully");

    return false;
}

public CSessionTest Contains(string sessionID)
{
    if (string.IsNullOrEmpty(sessionID))
        return null;

    if (mSessionContainer.TryGetValue(sessionID, out var sessionObject))
        return sessionObject;
    else
        return null;
}


public bool Remove(string sessionID)
{
    if (mSessionContainer.ContainsKey(sessionID))
    {
        if (mSessionContainer.TryRemove(sessionID, out var sessionObject))
            return true;
    }

    return false;
}

public IEnumerable<T> GetSessionSequence<T>() where T : CSessionTest
{
    var enumerator = mSessionContainer.GetEnumerator();

    while(!enumerator.MoveNext())
    {
        if (enumerator.Current.Value == null)
            continue;

        if (enumerator.Current.Value is T result)
            yield return result;
    }
}

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
            user_session.OnCloseEvent += OnCloseEventHandler;
            user_session.SetSessionID(mSessionId);
        }
        mSessionContainer.TryAdd(mSessionId, user_session);
    }
}

// 세션 아이디를 이용하여 대상 세션객체를 관리 컨테이너에서 삭제하는 함수
// thread-safe
public static ValueTask OnCloseEventHandler(object sender, EventArgs e)
{
    var session = sender as CSession;

    session.OnCloseEvent -= OnCloseEventHandler;
    mSessionContainer.TryRemove(session.mSessionID, out CSession removedSession);

    return new ValueTask();
}

// 세션 아이디를 이용하여 대상 세션객체를 관리 컨테이너에서 삭제하는 함수 (동기)
public static bool Remove(in CSession session)
{
    return mSessionContainer.TryRemove(session.mSessionID, out CSession removedSession);
}

// 세션 아이디를 이용하여 대상 세션 객체를 가져오는 함수
// thread-safe
public static CSession GetSessionByKey(long session_id)
{
    mSessionContainer.TryGetValue(session_id, out CSession result);
    return result;
}

public static bool IsExist(long session_id)
{
    lock (mSessionLock)
    {
        return mSessionContainer.ContainsKey(session_id);
    }
}

// 별도의 스레드에서 체크하는 세션 상태
// 세션이 끊어져있다면 reconnect 진행, reconnect 시행횟수가 특정 수치를 넘어갈경우 socket disconnect 진행 
public static void CheckSessionState()
{
    foreach (var state in mSessionContainer)
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

// 현재 세션객체 총 갯수 반환 함수
// thread-safe
public static int Count() { return mSessionContainer.Count; }

// 시퀀스로 사용될 대상은 단일객체 리턴보다 시퀀스 리턴이 좋다
public static IEnumerable<TCSession> GetSessionSeq<TCSession>() where TCSession : CSession
{
    var lEnumerator = mSessionContainer.GetEnumerator();

    while (lEnumerator.MoveNext())
    {
        if (lEnumerator.Current.Value == null || lEnumerator.Current.Value.mTcpSocket == null)
            continue;

        if (lEnumerator.Current.Value is TCSession ret)
            yield return ret;
    }
}

public static long GetCurSessionId() { return mSessionId; }
*/