using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Utility
{
    public interface IPoolingManager<T> : IPoolingManagerBase
    {
        /// <summary>
        /// Push Function
        /// </summary>
        /// <param name="item"></param>
        void Push(T item);
    }
}
