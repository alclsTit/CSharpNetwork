using System;
using System.Threading;
// --- custom --- //
using ProjectWaterMelon.Network.Sytem;  
// -------------- //

namespace ProjectWaterMelon
{
    class MyGame
    {
        public static void Main()
        {
            CConnector lConnector = new CConnector();
            lConnector.Init(8800);

            Thread lAsyncConnectThread = new Thread(lConnector.Start);
            lAsyncConnectThread.Start();

            while(true)
            {
                Thread.Sleep(1000);
            }

        }

    }
}
