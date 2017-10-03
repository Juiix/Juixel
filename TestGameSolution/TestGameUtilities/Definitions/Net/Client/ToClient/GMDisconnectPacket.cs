using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client.ToClient
{
    public class GMDisconnectPacket : GMPacket
    {
        public override GMPacketType Type => GMPacketType.Disconnect;

        public string Reason;

        public override void Read(NetworkReader R)
        {
            Reason = R.ReadUTF8();
        }

        public override void Write(NetworkWriter W)
        {
            W.WriteUTF8(Reason);
        }
    }
}
