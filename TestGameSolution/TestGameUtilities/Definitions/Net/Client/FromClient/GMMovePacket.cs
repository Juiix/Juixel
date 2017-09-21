using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client.FromClient
{
    public class GMMovePacket : GMPacket
    {
        public override GMPacketType Type => GMPacketType.Move;
        
        public Location Position;

        public override void Read(NetworkReader R)
        {
            Position = R.ReadObject<Location>();
        }

        public override void Write(NetworkWriter W)
        {
            Position.Write(W);
        }
    }
}
