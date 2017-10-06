using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client.ToClient
{
    public class GMConnectPacket : GMPacket
    {
        public override GMPacketType Type => GMPacketType.Connect;

        public int Port;

        public override void Read(NetworkReader R)
        {
            Port = R.ReadInt();
        }

        public override void Write(NetworkWriter W)
        {
            W.Write(Port);
        }
    }
}
