using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
// --- custom --- //
// -------------- //

namespace ProjectWaterMelon.Network.SystemLib
{
    public class CBufferManager
    {
        private int mBufferSize = 0;
        private int mTotalBytes = 0;
        private int mCurIndex = 0;
        private byte[] mTotalByteArray;
        private Stack<int> mBufferStack = new Stack<int>();

        public CBufferManager(int bufferSize, int totalSize)
        {
            mBufferSize = bufferSize;
            mTotalBytes = totalSize;
            mTotalByteArray = new byte[totalSize];
        }

        /// <summary>
        /// SocketAsyncEventArgs 객체풀에서 버퍼 세팅용도로 사용
        /// 전체 버퍼를 크게 할당하고 일정량의 chunk(buffersize)를 잘라서 사용하는 식 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool SetBuffer(ref SocketAsyncEventArgs e)
        {
            if (mBufferStack.Count > 0)
            {
                e.SetBuffer(mTotalByteArray, mBufferStack.Pop(), mBufferSize);
            }
            else
            {
                // 전체 버퍼에서 현재 인덱스 + 버퍼 하나당 사이즈 > 전체 버퍼 사이즈, 더 이상 할당할 수 없는 상태
                if (mCurIndex + mBufferSize > mTotalBytes)
                    return false;

                e.SetBuffer(mTotalByteArray, mCurIndex, mBufferSize);
                mCurIndex += mBufferSize;
            }

            return true;
        }

        /// <summary>
        /// 전체 버퍼에 사용한 SocketAsyncEventArgs 객체 위치 반납 및 객체 초기화
        /// </summary>
        /// <param name="e"></param>
        public void FreeBuffer(SocketAsyncEventArgs e)
        {
            mBufferStack.Push(e.Offset);
            e.SetBuffer(null, 0, 0);
        }
    }


    /*internal class CBufferManager
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

        /// <summary>
        ///  이론은 간단 
        ///  특정 객체를 매번 할당, 해제하는것으로 인해 생기는 메모리 파편화를 막고자</summary>
        ///  미리 큰 메모리 공간을 할당해놓고 재사용 <param name="BufferCount"></param>
        ///  mNumBytes = 1000(동접 수) * 1024(버퍼크기) * 2<param name="BufferSize"></param>
        ///  => ([1024][1024][1024]....[1024])<param name="ConCurrentFlag"></param>
        /// <param name="AutoIncrease"></param>
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

        internal bool SetBuffer(SocketAsyncEventArgs e)
        {
            if (mConcurrentFlag)
            {
                if (mFreeIndexPool_ThreadSafe.Count > 0)
                {
                    if (mFreeIndexPool_ThreadSafe.TryTake(out var index))
                        e.SetBuffer(mTotalBuffer, index, mTackBufferSize);
                }
                else
                {
                    if (mNumBytes < mCurrentIndexPos + mTackBufferSize)
                        return false;

                    e.SetBuffer(mTotalBuffer, mCurrentIndexPos, mTackBufferSize);
                    mCurrentIndexPos += mTackBufferSize;
                }

                return true;
            }
            else
            {
                if (mFreeIndexPool_NoThreadSafe.Count > 0)
                {
                    e.SetBuffer(mTotalBuffer, mFreeIndexPool_NoThreadSafe.Pop(), mTackBufferSize);
                }
                else
                {
                    if (mNumBytes < mCurrentIndexPos + mTackBufferSize)
                        return false;

                    e.SetBuffer(mTotalBuffer, mCurrentIndexPos, mTackBufferSize);
                    mCurrentIndexPos += mTackBufferSize;
                }

                return true;
            }


        }

        // 사용한 버퍼는 메모리 풀에 반환
        internal void FreeBuffer(SocketAsyncEventArgs e)
        {
            if (mConcurrentFlag)
                mFreeIndexPool_ThreadSafe.Add(e.Offset);
            else
                mFreeIndexPool_NoThreadSafe.Push(e.Offset);

            e.SetBuffer(null, 0, 0);
            e.Dispose();
        }
    }
    */
}
