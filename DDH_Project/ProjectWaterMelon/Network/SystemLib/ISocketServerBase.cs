using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ProjectWaterMelon.Network.SystemLib
{
    public interface ISocketServerBase
    {
        /// <summary>
        /// 서버객체 상태 (false = 비활성화 / true = 활성화)
        /// </summary>
        bool isRunning { get; }

        /// <summary>
        /// 서버 객체 초기화 (팩토리 패턴)
        /// </summary>
        void Initialize();

        /// <summary>
        /// 서버 객체 작업 시작 
        /// </summary>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// 서버 객체 작업 중단 
        /// </summary>
        void Stop();

    }
}
