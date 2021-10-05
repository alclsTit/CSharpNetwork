using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CModule.Network
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class CPacketData
    {
        public CPacketData()
        {

        }
        public byte[] Serialize()
        {
            var size = Marshal.SizeOf
        }
    }
}
