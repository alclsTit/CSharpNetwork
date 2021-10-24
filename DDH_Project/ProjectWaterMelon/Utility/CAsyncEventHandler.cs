using System;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Utility
{
    public delegate ValueTask AsyncEventHandler(object sender, EventArgs e);

    public delegate ValueTask AsyncEventHandler<TEventArgs>(object sender, EventArgs e) 
        where TEventArgs : EventArgs;

    public delegate ValueTask AsyncEventHandler<TSender, TEventArgs>(TSender sender, EventArgs e)
        where TSender : class
        where TEventArgs : EventArgs;
    
}
