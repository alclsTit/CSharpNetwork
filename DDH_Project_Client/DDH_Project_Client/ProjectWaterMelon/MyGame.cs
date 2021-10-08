using System;
using System.Threading;
// --- custom --- //
using ProjectWaterMelon.Network.Sytem;
using ProjectWaterMelon.Network.MessageWorker;
// -------------- //

namespace ProjectWaterMelon
{
    class MyGame
    {
        public static void Main()
        {
            // register MessageHandler
            CMessageReceiver.Init();

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
