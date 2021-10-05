using System.Collections.Generic;
using System.Net.Sockets;
using System.Collections.Concurrent;
// --- custom --- //
// -------------- //

namespace ProjectWaterMelon.Network.Sytem
{
    internal class CBufferManager
    {
        private bool mConcurrentFlag;
        // 전체 메모리 풀
        private byte[] mTotalBuffer;
        private int mTackBufferSize;
        private int mNumBytes;
        private int mCurrentIndexPos;

        // 사용한 버퍼 저장 공간
        private ConcurrentBag<int> mFreeIndexPool_ThreadSafe;
        private Stack<int> mFreeIndexPool_NoThreadSafe;

        // 이론은 간단 
        // 특정 객체를 매번 할당, 해제하는것으로 인해 생기는 메모리 파편화를 막고자
        // 미리 큰 메모리 공간을 할당해놓고 재사용 
        internal CBufferManager(int BufferCount, int BufferSize, bool ConCurrentFlag = true, bool AutoIncrease = true)
        {
            // 전송용, 수신용 버퍼 2개 
            mConcurrentFlag = ConCurrentFlag;
            mNumBytes = BufferCount * BufferSize * 2;
            mTackBufferSize = BufferSize;
            mCurrentIndexPos = 0;
            mTotalBuffer = new byte[mNumBytes];

            if (ConCurrentFlag)
                mFreeIndexPool_ThreadSafe = new ConcurrentBag<int>();
            else
                mFreeIndexPool_NoThreadSafe = new Stack<int>();
        }

        internal bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (mConcurrentFlag)
            {
                if (mFreeIndexPool_ThreadSafe.Count > 0)
                {
                    if (mFreeIndexPool_ThreadSafe.TryTake(out var index))
                        args.SetBuffer(mTotalBuffer, index, mTackBufferSize);
                }
                else
                {
                    if (mNumBytes < mCurrentIndexPos + mTackBufferSize)
                        return false;

                    args.SetBuffer(mTotalBuffer, mCurrentIndexPos, mTackBufferSize);
                    mCurrentIndexPos += mTackBufferSize;
                }

                return true;
            }
            else
            {
                if (mFreeIndexPool_NoThreadSafe.Count > 0)
                {
                    args.SetBuffer(mTotalBuffer, mFreeIndexPool_NoThreadSafe.Pop(), mTackBufferSize);
                }
                else
                {
                    if (mNumBytes < mCurrentIndexPos + mTackBufferSize)
                        return false;

                    args.SetBuffer(mTotalBuffer, mCurrentIndexPos, mTackBufferSize);
                    mCurrentIndexPos += mTackBufferSize;
                }

                return true;
            }
        }

        // 사용한 버퍼는 메모리 풀에 반환
        internal void FreeBuffer(SocketAsyncEventArgs args)
        {
            if (mConcurrentFlag)
                mFreeIndexPool_ThreadSafe.Add(args.Offset);
            else
                mFreeIndexPool_NoThreadSafe.Push(args.Offset);

            args.SetBuffer(null, 0, 0);
            args.Dispose();
        }
    }
}
