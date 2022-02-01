using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.CustomSocket
{
    /// <summary>
    /// [0]         [1 ~ 15]          [16 ~ 31] 
    /// no use     state(past)      state(current)
    /// 0x0000 - NotInitalized
    /// 0x0001 - Initialized
    /// 0x0010 - InConnecting
    /// 0x0020 - Connected
    /// 0x0030 - InClosing
    /// 0x0040 - Disconnected
    /// 0x0100 - InSending
    /// 0x1000 - InReceving
    /// </summary>
    /// 
    public static class GSocketStateMask
    {
        public const int EMPTY_MASK = 0x00000000;
        public const int OLD_MASK = 0x7FFF0000;
        public const int NEW_MASK = 0x0000FFFF;
    }

    public static class GSocketState
    {
        public const int NotInitialized = 0;

        public const int Initialized = 1;   //0001

        public const int InClosing = 48;     //0110
        
        public const int Connected = 2;     //0010

        public const int Disconnected = 3;  //0011

        public const int Sending = 4;       //0100

        public const int Receiving = 5;     //0101

    }
}   

/*
    /// <summary>
    /// Socket State
    /// Interlocked.CompareExchange 사용을 위해서는 소켓 상태값이 참조형식이 되어야한다
    /// 따라서 enum이 아닌 전역클래스로 정의
    /// </summary>
    public static class GSocketCondition
    {
        public const int Initialized = 0;

        public const int Connected = 1;

        public const int Disconnected = 2;

        public const int Sending = 3;

        public const int Receiving = 4;

        public const int InClosing = 5;
    }
 */