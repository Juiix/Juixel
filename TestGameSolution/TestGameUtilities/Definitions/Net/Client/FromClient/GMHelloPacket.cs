using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client.FromClient
{
    public class GMHelloPacket : GMPacket
    {
        public override GMPacketType Type => GMPacketType.Hello;

        public string Username;
        public string Key;
        public string Version;

        public override void Read(NetworkReader R)
        {
            Username = R.ReadUTF16();
            Key = R.ReadUTF16();
            Version = R.ReadUTF8();
        }

        public override void Write(NetworkWriter W)
        {
            W.WriteUTF16(Username);
            W.WriteUTF16(Key);
            W.WriteUTF8(Version);
        }
    }
}
