using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Permissions;

/*
* 이론은 간단 
* 특정 객체를 매번 할당, 해제하는것으로 인해 생기는 메모리 파편화를 막고자
* 미리 큰 메모리 공간을 할당해놓고 재사용 
*/
namespace CModule
{
    // 소켓통신에 사용하는 socketasyncEventargs 풀링으로 재사용을 통한 성능 향상
    // 스레드 세이프 해야한다.

    public class CMemoryPool<T> where T : IDisposable, new()
    {
        // 메모리풀 변경 
        private Queue<T> mSyncPools;
        private ConcurrentQueue<T> mAsyncPools;
        private Func<T> m_CreateFunc;
        private int mMaxBufferCount;
        private bool mAsyncOption;
        private object mMemoryLock = new object();

        public int CurrentCount() => mAsyncOption == true ? mAsyncPools.Count() : mSyncPools.Count();
        public int TotalCount() => mMaxBufferCount;

        // 그냥 queue 에 저장이 되는 부분과 concurrentqueue를 사용해쓸 때의 차이확인 필요
        // concurrentqueue 사용이 확정이 되면 기존 queue 는 삭제 
        // [param] 
        // BuffCount - 풀링할 객체의 전체 크기 (accept, send/recv 크기가 다름) 
        public CMemoryPool(int BufferCount, bool AsyncOption = true, Func<T> func = null) 
        {
            mAsyncOption = AsyncOption;
            mMaxBufferCount = BufferCount;

            if (AsyncOption)
                mAsyncPools = new ConcurrentQueue<T>();
            else
                mSyncPools = new Queue<T>(BufferCount);
        }

        public void Init(int BufferCount, bool AsyncOption = true, Func<T> func = null)
        {
            lock (mMemoryLock)
            {
                mAsyncOption = AsyncOption;
                mMaxBufferCount = BufferCount;
                if (m_CreateFunc is IDisposable)
                {
                    using (m_CreateFunc as IDisposable)
                    {
                        m_CreateFunc = () => new T();
                    }
                }
                else
                {
                    m_CreateFunc = func;
                }
            }

            ClearContainer(AsyncOption, BufferCount);

            if (mAsyncOption)
            {
                for (var i = 0; i < BufferCount; ++i)
                    mAsyncPools.Enqueue(m_CreateFunc());
            }
            else
            {
                for (var i = 0; i < BufferCount; ++i)
                    mSyncPools.Enqueue(m_CreateFunc());
            }
        }

        public void ClearContainer(bool AsyncOption, int BufferCount)
        {
            if (AsyncOption)
            {
                if (mAsyncPools.Count > 0)
                {
                    mAsyncPools = null;
                    mAsyncPools = new ConcurrentQueue<T>();
                }
            }
            else
            {
                lock (mMemoryLock)
                {
                    if (mSyncPools.Count > 0)
                    {
                        mSyncPools = null;
                        mSyncPools = new Queue<T>(BufferCount);
                    }
                }
            }
        }

        /*
         *  Push => 메모리풀에 객체 반납
         *  만약, 객체 반납 시 메모리풀이 꽉차 있다면 그대로 소멸 
         */
        public void ConcurrentPush(T data)
        {
            if (mAsyncPools.Count() < mMaxBufferCount)
                mAsyncPools.Enqueue(data);
            else
                data.Dispose();
        }

        /*
         *  ConCurrentPop => 메모리풀에서 객체 사용 
         *  만약, 메모리풀에 사용할 수 있는 객체가 없을 경우 객체 생성 후 반환
         */
        public T ConcurrentPop()
        {
            if (mAsyncPools.Count() > 0)
            {
                T result;
                if (mAsyncPools.TryDequeue(out result))
                {
                    return result;
                }
                else
                {
                    CLog4Net.LogError($"Error in CMemoryPool.ConcurrentPop!!! - TryDequeue Error...");
                    return m_CreateFunc();
                }
            }
            else
            {
                CLog4Net.LogError($"Error in CMemoryPool.ConcurrentPop!!! - Not Enough Memory Pool Space");
                return m_CreateFunc();
            }
        }


        /*
         * Push => 일반 Queue 컨테이너에 데이터 저장 
         */
        public void Push(T data)
        {
            lock (mMemoryLock)
            {
                if (mSyncPools.Count() < mMaxBufferCount)
                    mSyncPools.Enqueue(data);      
                else
                    data.Dispose();
            }
        }

        /*
         * Pop =>일반 Queue 컨테이너에서 데이터 가져옴 
         */
        public T Pop()
        {
            lock (mMemoryLock)
            {
                if (mSyncPools.Count() > 0)
                {
                    return mSyncPools.Dequeue();
                }
                else
                {
                    CLog4Net.LogError($"Error in CMemoryPool.Pop!!! - Not Enough Memory Pool Space");
                    return m_CreateFunc();
                }
            }
        }

        public void Dispose()
        {
            if (mAsyncOption)
            {
                lock (mMemoryLock)
                {
                    for (var i = 0; i < mMaxBufferCount; ++i)
                    {
                        T result;
                        if (mAsyncPools.TryDequeue(out result))
                            result.Dispose();
                    }
                    mAsyncPools = null;
                }
            }
            else
            {
                lock (mMemoryLock)
                {
                    for (var i = 0; i < mMaxBufferCount; ++i)
                        mSyncPools.Dequeue().Dispose();       
                    mSyncPools = null;
                }
            }     
        }
    }
}
