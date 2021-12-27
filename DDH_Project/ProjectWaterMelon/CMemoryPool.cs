using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Permissions;
// --- custom --- //
using ProjectWaterMelon.Log;
// -------------- //

/// <summary>
/// 이론은 간단. 객체를 매번 할당, 해제하는것으로 인해 생기는 GC오버헤드 및 메모리 파편화를 막고자
/// 미리 큰 메모리 공간을 할당해놓고 재사용
/// 소켓통신에 사용하는 socketasyncEventargs 풀링으로 재사용을 통한 성능 향상
/// </summary>
namespace ProjectWaterMelon
{
    /// <summary>
    /// ThreadSafe Memorypool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CMemoryPoolConcurrent<T> where T: IDisposable, new()
    {
        private readonly int mMaxPoolingCount = 0; 
        private Func<T> mCreateFunc;
        /// <summary>
        /// ThreadSafe한 병렬 컨테이너 사용(push, pop, count, isEmpty)
        /// </summary>
        private ConcurrentStack<T> mConcurrentStack = new ConcurrentStack<T>();

        /// <summary>
        /// 크리티컬 섹션, 해당 부분은 프로세스간 동기화가 필요없고 하나의 프로세스의 다중 스레드간 lock 필요
        /// </summary>
        private object mCriticalLock = new object();

        /// <summary>
        /// 메모리풀에서 관리하는 총 객체 수
        /// </summary>
        public int GetMaxPoolingCount => mMaxPoolingCount;

        /// <summary>
        /// 현재 메모리풀에서 관리되는 객체 수
        /// </summary>
        public int GetCurPoolingCount => mConcurrentStack.Count;

        /// <summary>
        /// 메모리풀이 비었는지 확인(메모리풀 관리 객체 수 = 0)
        /// </summary>
        public bool IsPoolEmpty => mConcurrentStack.IsEmpty;

        /// <summary>
        /// 제네릭객체 생성하여 ConcurrentStack에 maxPoolCount 만큼 저장 
        /// </summary>
        /// <param name="maxPoolCount"></param>
        /// <param name="func"></param>
        public CMemoryPoolConcurrent(int maxPoolCount, Func<T> func = null)
        {
            mMaxPoolingCount = maxPoolCount;

            if (func != null)
            {
                var hasDispose = mCreateFunc as IDisposable;
                if (hasDispose != null)
                    mCreateFunc = () => new T();
                else
                    mCreateFunc = func;

                for(var idx = 0; idx < maxPoolCount; ++idx)
                    mConcurrentStack.Push(mCreateFunc()); 
            }
        }

        public void ContainerClear()
        {
            mConcurrentStack.Clear();
        }

        public void Push(T data)
        {
            var tmpData = data;
            if (tmpData == null)
                return;

            if (mConcurrentStack.Count > mMaxPoolingCount)
                return;

            mConcurrentStack.Push(tmpData);
        }

        public T Pop()
        {
            if (mConcurrentStack.Count <= 0)
            {
                lock(mCriticalLock){
                    GCLogger.Warn(nameof(CMemoryPoolConcurrent<T>), "Pop", "ConcurrentStack count zero");
                }
                return mCreateFunc();
            }

            if (mConcurrentStack.TryPop(out T data))
                return data;
            else
            {
                lock (mCriticalLock){
                    GCLogger.Warn(nameof(CMemoryPoolConcurrent<T>), "Pop", "ConcurrentStack TryPop error");
                }
                return mCreateFunc();
            }              
        }

        public void Dispose()
        {
            for (var idx = 0; idx < this.GetCurPoolingCount; ++idx)
                this.Pop().Dispose();

            mConcurrentStack = null;
        }
    }

    // 그냥 stack 에 저장이 되는 부분과 concurrentstack 사용해쓸 때의 차이확인 필요
    // concurrentstack 사용이 확정이 되면 기존 stack 는 삭제 
    // [param] 
    // BuffCount - 풀링할 객체의 전체 크기 (accept, send/recv 크기가 다름) 
    /// <summary>
    /// Non-Concurrent Memory Pool (lock 사용)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CMemoryPool<T> where T : IDisposable, new()
    {
        private readonly int mMaxPoolingCount = 0;
        private Func<T> mCreateFunc;
        private Stack<T> mStack = new Stack<T>();

        /// <summary>
        /// 크리티컬 섹션, 해당 부분은 프로세스간 동기화가 필요없고 하나의 프로세스의 다중 스레드간 lock 필요
        /// </summary>
        private object mCriticalLock = new object();

        /// <summary>
        /// 메모리풀에서 관리하는 총 객체 수
        /// </summary>
        public int GetMaxPoolingCount => mMaxPoolingCount;

        /// <summary>
        /// 현재 메모리풀에서 관리되는 객체 수
        /// </summary>
        public int GetCurPoolingCount()
        {
            var count = mStack.Count;
            return count;
        }

        /// <summary>
        /// 메모리풀이 비었는지 확인(메모리풀 관리 객체 수 = 0)
        /// </summary>
        public bool IsPoolEmpty()
        {
            return GetCurPoolingCount() == 0 ? true : false;
        }

        public CMemoryPool(int maxPoolCount, Func<T> func = null)
        {
            mMaxPoolingCount = maxPoolCount;
          
            if (func != null)
            {
                var hasDispose = func as IDisposable;
                if (hasDispose != null)
                    mCreateFunc = () => new T();
                else
                    mCreateFunc = func;

                for (var idx = 0; idx < maxPoolCount; ++idx)
                    mStack.Push(mCreateFunc());
            }
        }

        public void ContainerClear()
        {
            lock(mCriticalLock)
            {
                mStack.Clear();
            }
        }

        public void Push(T data)
        {
            if (this.GetCurPoolingCount() >= mMaxPoolingCount)
                return;

            lock(mCriticalLock)
            {
                mStack.Push(data);
            }
        }

        public T Pop()
        {
            if (this.GetCurPoolingCount() <= 0)
            {
                lock (mCriticalLock) {
                    GCLogger.Warn(nameof(CMemoryPool<T>), "Pop", "Stack Pop error");
                }
                return mCreateFunc();
            }
            else
            {
                var result = mStack.Pop();
                return result;
            }
        } 

        public void Dispose()
        {
            for(var idx = 0; idx < this.GetCurPoolingCount(); ++idx)
                this.Pop().Dispose();
            mStack = null;
        }
    }
}


/*public void Init(int BufferCount, bool AsyncOption = true, Func<T> func = null)
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
        */

/*public void ClearContainer(bool AsyncOption, int BufferCount)
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
*/

/*
 *  Push => 메모리풀에 객체 반납
 *  만약, 객체 반납 시 메모리풀이 꽉차 있다면 그대로 소멸 
 */
/*public void ConcurrentPush(T data)
{
    if (mAsyncPools.Count() < mMaxBufferCount)
        mAsyncPools.Enqueue(data);
    else
        data.Dispose();
}*/

/*
 *  ConCurrentPop => 메모리풀에서 객체 사용 
 *  만약, 메모리풀에 사용할 수 있는 객체가 없을 경우 객체 생성 후 반환
 */
/*public T ConcurrentPop()
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
*/

/*
//Push => 일반 Queue 컨테이너에 데이터 저장 
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

//Pop =>일반 Queue 컨테이너에서 데이터 가져옴 
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
 */

/*public void Dispose()
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
*/