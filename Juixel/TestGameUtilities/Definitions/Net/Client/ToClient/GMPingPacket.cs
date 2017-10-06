using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client.ToClient
{
    public class GMPingPacket : GMPacket
    {
        public override GMPacketType Type => GMPacketType.Ping;

        public byte PingId;

        public override void Read(NetworkReader R)
        {
            PingId = R.ReadByte();
        }

        public override void Write(NetworkWriter W)
        {
            W.Write(PingId);
        }
    }
}
