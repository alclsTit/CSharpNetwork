using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Utility
{
    public interface IPoolingManagerBase
    {
        /// <summary>
        /// max pool size
        /// </summary>
        int maxPoolSize { get; }
    }
}
