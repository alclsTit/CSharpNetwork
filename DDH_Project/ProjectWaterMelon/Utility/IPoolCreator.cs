using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Utility
{
    /// <summary>
    /// PoolingManager에서 사용할 IPoolCreator
    /// </summary>
    public interface IPoolCreator<T>
    {
        void Create(int size, out T[] poolItems);
    }
}
