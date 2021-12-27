using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace ProjectWaterMelon.Utility
{
    public sealed class CSendingQueue : IList<ArraySegment<byte>>
    {
        private readonly int mOffset;
        private readonly int mCapacity;

        private int mCurCount = 0;
        private int mUpdateCount = 0;
        public ushort mTrackID { get; private set; } = 1;

        private ArraySegment<byte>[] mSegmentContainer;

        private static ArraySegment<byte> mNull = default(ArraySegment<byte>);

        private int mInnerOffset = 0;
        public bool mReadOnly = false;
        public ArraySegment<byte>[] SegmentContainer => mSegmentContainer;
        public int Offset => mOffset;
        public int Capacity => mCapacity;
        public int Count => mCurCount - mInnerOffset;
        public int Position { get; private set; } = 0;
        public bool IsReadOnly => mReadOnly;

        public CSendingQueue(in ArraySegment<byte>[] queue, int offset, int capacity)
        {
            mSegmentContainer = queue;
            mOffset = offset;
            mCapacity = capacity;
        }

        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            for (var i = 0; i < mCurCount - mInnerOffset; ++i) 
            {
                yield return mSegmentContainer[mOffset + mInnerOffset + i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool TryEnqueue(in ArraySegment<byte> item, out bool conflict, ushort trackID)
        {
            conflict = false;

            var oldCount = mCurCount;

            // 큐에서 설정한 크기를 초과하여 세팅을 하려는 경우
            if (oldCount >= mCapacity)
                return false;

            if (mReadOnly)
                return false;

            if (mTrackID != trackID)
                return false;

            int compCount = Interlocked.CompareExchange(ref mCurCount, oldCount + 1, oldCount);

            if (compCount != oldCount)
            {
                conflict = true;
                return false;
            }

            // (초기위치 + 증가한 위치)에 새로운 데이터 할당
            mSegmentContainer[mOffset + oldCount] = item;

            return true;
        }

        public bool Enqueue(in ArraySegment<byte> item, ushort trackID)
        {
            if (mReadOnly)
                return false;

            Interlocked.Increment(ref mUpdateCount);

            while(!mReadOnly)
            {
                bool conflict = false;
                if (TryEnqueue(item, out conflict, trackID))
                {
                    Interlocked.Decrement(ref mUpdateCount);
                    return true;
                }

                if (!conflict)
                    break;
            }

            Interlocked.Decrement(ref mUpdateCount);
            return false;
        }

        public bool Enqueue(in IList<ArraySegment<byte>> items, ushort trackID)
        {
            if (mReadOnly)
                return false;

            Interlocked.Increment(ref mUpdateCount);

            bool conflict = false;

            while(!mReadOnly)
            {
                if (TryEnqueue(items, out conflict, trackID))
                {
                    Interlocked.Decrement(ref mUpdateCount);
                    return true;
                }

                if (!conflict)
                    return false;
            }

            Interlocked.Decrement(ref mUpdateCount);
            return false;
        }

        private bool TryEnqueue(in IList<ArraySegment<byte>> items, out bool conflict, ushort trackID)
        {
            conflict = false;

            if (mReadOnly)
                return false;

            if (mTrackID != trackID)
                return false;

            var oldcount = mCurCount;
            var newItemCount = items.Count;
            var expectedCount = oldcount + newItemCount;

            int compCount = Interlocked.CompareExchange(ref mCurCount, oldcount + 1, oldcount);

            if (compCount != oldcount)
            {
                conflict = true;
                return false;
            }

            var queue = mSegmentContainer;

            for(var i = 0; i < items.Count; ++i)
            {
                queue[mOffset + oldcount + i] = items[i];
            }

            return true;
        }

        public void InternalTrim(int offset)
        {
            var innerCount = mCurCount - mInnerOffset;
            var subTotal = 0;

            for(var i = mInnerOffset; i < innerCount; ++i)
            {
                var segment = mSegmentContainer[mOffset + i];
                subTotal += segment.Count;

                if (subTotal <= offset)
                    continue;

                mInnerOffset = i;

                var rest = subTotal - offset;
                mSegmentContainer[mOffset + i] = new ArraySegment<byte>(segment.Array, segment.Offset + segment.Count - rest, rest);

                break;
            }
        }

        public void StopEnqueue()
        {
            if (mReadOnly)
                return;

            mReadOnly = true;

            if (mUpdateCount <= 0)
                return;

            var spinWait = new SpinWait();

            spinWait.SpinOnce();

            //Wait until all insertings are finished
            while (mUpdateCount > 0)
            {
                spinWait.SpinOnce();
            }
        }

        public void StartEnqueue()
        {
            mReadOnly = false;
        }

        public ArraySegment<byte> this[int index]
        {
            get
            {
                var targetIndex = mOffset + mInnerOffset + index;
                var value = mSegmentContainer[targetIndex];

                if (value.Array != null)
                    return value;

                var spinWait = new SpinWait();

                while(true)
                {
                    spinWait.SpinOnce();
                    value = mSegmentContainer[targetIndex];

                    if (value.Array != null)
                        return value;

                    if (spinWait.Count > 50)
                        return value;
                }
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void CopyTo(ArraySegment<byte>[] item, int index)
        {
            for(var i = 0; i < Count; ++i)
            {
                item[index + i] = this[i];
            }
        }

        public void Clear()
        {
            if (mTrackID >= ushort.MaxValue)
                mTrackID = 1;
            else
                ++mTrackID;

            for (var i = 0; i < mCurCount; ++i) 
            {
                mSegmentContainer[mOffset + i] = mNull;
            }

            mCurCount = 0;
            mInnerOffset = 0;
            Position = 0;
        }

        public int IndexOf(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }
      
        public void Insert(int index, ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

    }


    /*public sealed class CSendingQueue : IList<ArraySegment<byte>>
{
    public int mOffset { get; private set; } = 0;
    private int mByteArrayLen = 0;
    public int mHaveToRead { get; private set; }

    // byte 배열 세그먼트 (byte[] array)
    public ArraySegment<byte> mByteArray { get; private set; }

    public CSendingQueue(in ArraySegment<byte> array)
    {
        mByteArray = array;
    }

    public CSendingQueue(in ArraySegment<byte> array, int offset, int length)
    {
        mByteArray = array;
        mOffset = offset;
        mByteArrayLen = length;
        mHaveToRead = mByteArrayLen - mOffset;
    }

    public IEnumerable<ArraySegment<byte>> GetEnumerator()
    {
        foreach(var segment in mByteArray)
        {
            yield return segment;
        }
    }

    public int indexOf(ArraySegment<byte> item)
    {
        throw new NotSupportedException();
    }

    public void insert(int index, ArraySegment<byte> item)
    {
        throw new NotSupportedException();
    }

}
*/


    /*
   public sealed class CSendingQueuePool : IList<ArraySegment<byte>>
    {
        // thread-safe list
        private ConcurrentBag<ArraySegment<byte>> mConBagContainer = new ConcurrentBag<ArraySegment<byte>>();

        public bool mIsReadOnly { get; private set; } = false;
        public int mCapacity { get; private set; } = 0;

        public CSendingQueuePool(int capacity)
        {
            mCapacity = capacity;

            Initialize();
        }

        public void Initialize()
        {
            for(var i = 0; i < mCapacity; ++i)
            {
                mConBagContainer.Add(new ArraySegment<byte>());
            }
        }

        public IEnumerable<ArraySegment<byte>> GetEnumerator()
        {
            foreach (var segment in mConBagContainer)
            {
                yield return segment;
            }
        }

        public void Add(in ArraySegment<byte> item)
        {
            if (item == null)
                return;

            mConBagContainer.Add(item);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public int Count => mConBagContainer.Count;

        public bool IsReadOnly => mIsReadOnly;

        public int indexOf(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void insert(int index, ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(ArraySegment<byte> item, int index)
        {
            throw new NotSupportedException();
        }
        
    }
 
     */
}
