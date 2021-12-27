using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
// --- custom --- //
using ProjectWaterMelon.Log;
using static ProjectWaterMelon.GSocketState;
// -------------- //

namespace ProjectWaterMelon.Network.SystemLib
{
    public sealed class CSocketAsyncEventArgsPool
    {
        /// <summary>
        /// SocketAsyncEventArgs 메모리 풀 객체
        /// </summary>
        private CMemoryPoolConcurrent<SocketAsyncEventArgs> mConcurrentSocketPool;

        /// <summary>
        /// 메모리풀에서 관리하는 총 SocketAsyncEventArgs 객체 수
        /// </summary>
        public int GetMaxPoolingCount => mConcurrentSocketPool.GetMaxPoolingCount;

        /// <summary>
        /// 현재 메모리풀에서 관리되는 SocketAsyncEventArgs 객체 수
        /// </summary>
        public int GetCurPoolingCount => mConcurrentSocketPool.GetCurPoolingCount;

        public CSocketAsyncEventArgsPool(int maxPoolCount)
        {
            mConcurrentSocketPool = new CMemoryPoolConcurrent<SocketAsyncEventArgs>(maxPoolCount, () => new SocketAsyncEventArgs());
        }

        /// <summary>
        /// ThreadSafe Stack Push
        /// </summary>
        /// <param name="sockObj"></param>
        public void Push(in SocketAsyncEventArgs sockObj)
        {
            if (sockObj == null)
                throw new ArgumentException("Items added to a SocketAsyncEventArgsPool cannot be null");

            mConcurrentSocketPool.Push(sockObj);
        }

        /// <summary>
        /// ThreadSafe Stack Pop
        /// </summary>
        /// <returns></returns>
        public SocketAsyncEventArgs Pop()
        {
            return mConcurrentSocketPool.Pop();
        }
    }
}

/*
  internal sealed class CSocketAsyncEventArgsPool
    {
        private int mTokenId;
        private eSockEvtType mSockEvtPoolType;
        private CMemoryPool<SocketAsyncEventArgs> mSockEvtPools;
        internal int CurrentCount => mSockEvtPools.CurrentCount();
        internal int Count => mSockEvtPools.TotalCount();

        internal CSocketAsyncEventArgsPool(int BufferCount, eSockEvtType type)
        {
            mSockEvtPoolType = type;
            //mSockEvtPools = type == eSockEvtType.CONCURRENT ?
            //    new CMemoryPool<SocketAsyncEventArgs>(BufferCount, true, () => new SocketAsyncEventArgs()) : new CMemoryPool<SocketAsyncEventArgs>(BufferCount, false, () => new SocketAsyncEventArgs());
        }

        // session id 
        internal int AssignTokenId()
        {
            return Interlocked.Increment(ref mTokenId);
        }

        internal void Push(in SocketAsyncEventArgs data)
        {
            if (data == null)
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");

            //ConcurrentQueue or Queue에 Push 진행           
            if (mSockEvtPoolType == eSockEvtType.CONCURRENT)
                mSockEvtPools.ConcurrentPush(data);
            else
                mSockEvtPools.Push(data);
        }


        internal SocketAsyncEventArgs Pop()
        {
            //ConcurrentQueue or Queue에 Pop 진행
            if (mSockEvtPoolType == eSockEvtType.CONCURRENT)
                return mSockEvtPools.ConcurrentPop();
            else
                return mSockEvtPools.Pop();
        }

        internal bool TryPop()
        {

        }
    }
 */