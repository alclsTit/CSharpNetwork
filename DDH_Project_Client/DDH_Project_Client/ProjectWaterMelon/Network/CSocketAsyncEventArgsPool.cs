using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using static ConstModule.GSocketState;
using System.Threading;

namespace ProjectWaterMelon
{
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
            mSockEvtPools = type == eSockEvtType.CONCURRENT ?
                new CMemoryPool<SocketAsyncEventArgs>(BufferCount, true, () => new SocketAsyncEventArgs()) : new CMemoryPool<SocketAsyncEventArgs>(BufferCount, false, () => new SocketAsyncEventArgs());
        }

        // session id 
        internal int AssignTokenId()
        {
            return Interlocked.Increment(ref mTokenId);
        }

        internal void Push(SocketAsyncEventArgs data)
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
            //return ref mSockEvtPoolType == eSockEvtType.CONCURRENT ? mSockEvtPools.ConcurrentPop() : mSockEvtPools.Pop();
            if (mSockEvtPoolType == eSockEvtType.CONCURRENT)
                return mSockEvtPools.ConcurrentPop();
            else
                return mSockEvtPools.Pop();

        }
    }
}
