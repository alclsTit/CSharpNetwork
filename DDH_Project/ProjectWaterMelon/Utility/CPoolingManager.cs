using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using ProjectWaterMelon.Log;

namespace ProjectWaterMelon.Utility
{

    /// <summary>
    /// T 타입 파라미터에 해당하는 객체를 관리하는 풀링 매니저
    /// 사용되는 곳마다 인스턴스 생성해서 사용 (전역으로 구현X, 정의된 클래스에 종속되어 사용되게끔...)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CPoolingManager<T> : IPoolingManager<T> 
    {
        private ConcurrentStack<T> mContainer;
        public int maxPoolSize { get; private set; } = 0;
        public int MaxPoolSize => maxPoolSize;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CPoolingManager() { }

        public CPoolingManager(IPoolCreator<T> creator, int maxsize = 0, IEnumerable<T> items = null)
        {
            if (items == null)
            {
                mContainer = new ConcurrentStack<T>();

                // minsize of pool is zero 
                creator.Create(maxsize, out T[] poolitems);

                for (var i = 0; i < maxsize; ++i)
                {
                    mContainer.Push(poolitems[i]);
                }
            }
            else
            {
                mContainer = new ConcurrentStack<T>(items);
            }
        }

        public void Push(T item)
        {
            var maxsize = maxPoolSize;
            var count = mContainer.Count;

            if (maxsize < count)
            {
                GCLogger.Error(nameof(CPoolingManager<T>), $"Push", $"Push Error - pool size over!!!");
                return;
            }

            mContainer.Push(item);
        }

        public T Pop()
        {
            if (mContainer.TryPop(out T item))
            {
                return item;
            }
            else
            {
                return default(T);
            }
        }

        public bool TryPop()
        {
            return mContainer.TryPeek(out T item) == true ? true : false;
        }
    }
}
