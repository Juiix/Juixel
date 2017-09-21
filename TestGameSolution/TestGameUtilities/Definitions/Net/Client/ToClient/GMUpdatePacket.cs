using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client.ToClient
{
    public class GMUpdatePacket : GMPacket
    {
        public override GMPacketType Type => GMPacketType.Update;

        public int[] Ids;

        public override void Read(NetworkReader R)
        {
            Ids = new int[R.ReadInt()];
            for (int i = 0; i < Ids.Length; i++)
                Ids[i] = R.ReadInt();
        }

        public override void Write(NetworkWriter W)
        {
            W.Write(Ids.Length);
            for (int i = 0; i < Ids.Length; i++)
                W.Write(Ids[i]);
        }
    }
}
