using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGameUtilities.Definitions.Net.Client
{
    public enum GMPacketType : byte
    {
        Unknown = 0,
        Tick = 1,
        Update = 2,
        Move = 3,
        Shoot = 4,
        Hello = 5,
        Disconnect = 6,
        Connect = 7,
        Ping = 8,
        Pong = 9
    }
}
