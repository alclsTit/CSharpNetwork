using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ProjectWaterMelon.GameLib
{
    public delegate void OnAsyncCallback(object sender, SocketAsyncEventArgs e);

    /// <summary>
    /// 비동기 소켓 프로그래밍에서 공통적으로 사용되는 기능 
    /// </summary>
    public static class AsyncSocketCommonFunc
    {
        /// <summary>
        /// 이벤트 핸들러만 지정해서 SocketAsyncEventArgs 객체 단독 생성 
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static SocketAsyncEventArgs CreateObjectOne(OnAsyncCallback callback)
        {
            var callbackEvent = new SocketAsyncEventArgs();
            callbackEvent.Completed += new EventHandler<SocketAsyncEventArgs>(callback);

            return callbackEvent;
        }

        /// <summary>
        /// 비동기 소켓통신 응답에 대한 콜백함수가 호출될 때, 체크사항 (1.통신 성공유무 2.수신된 데이터바이트 크기)
        /// 수신된 데이터바이트 크기가 0인 경우 TCP에서는 소켓통신이 종료된 것으로 인식
        /// </summary>
        /// <param name="error"></param>
        /// <param name="byteTransferred"></param>
        /// <returns></returns>
        public static bool CheckCallbackHandler(SocketError error, int byteTransferred)
        {
            if (CheckCallbackHandler(error))
            {
                if (byteTransferred > 0)
                    return true;
            }

            return false;
        }

        public static bool CheckCallbackHandler(SocketError error)
        {
            return error == SocketError.Success;
        }
    }
}
