using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Utility
{
    public sealed class CSendingQueueCreator : IPoolCreator<CSendingQueue>
    {
        private int mSendingQueueSize = 0;
        private CSendingQueue[] mSendingQueuePool;
        public CSendingQueue[] SendingQueuePool => mSendingQueuePool;

        public CSendingQueueCreator(int queuePerSize)
        {
            mSendingQueueSize = queuePerSize;
        }

        /// <summary>
        /// CSendingQueue Pool Create Function
        /// mSendingQueueSize = 5 (Default)
        /// </summary>
        /// <param name="size"></param>
        /// <param name="poolItems"></param>
        public void Create(int size, out CSendingQueue[] poolItems)
        {
            var segment = new ArraySegment<byte>[size * mSendingQueueSize];

            poolItems = new CSendingQueue[size];

            for (var i = 0; i < size; ++i)
            {
                poolItems[i] = new CSendingQueue(segment, i * mSendingQueueSize, mSendingQueueSize);
            }
        }
    }
}
