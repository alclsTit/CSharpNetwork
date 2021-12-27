using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Extensions
{
    /// <summary>
    /// 확장메서드 전용 클래스
    /// 사용되는 곳에 using ProjectWaterMelon.Extensions 선언하여 사용
    /// </summary>
    public static class Extensions
    {
        public static void DoNotWait(this Task task)
        { 
        }

    }
}
