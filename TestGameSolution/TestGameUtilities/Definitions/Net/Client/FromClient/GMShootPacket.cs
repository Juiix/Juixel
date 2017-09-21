using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client.FromClient
{
    public class GMShootPacket : GMPacket
    {
        public override GMPacketType Type => GMPacketType.Shoot;
        
        public Angle Angle;

        public override void Read(NetworkReader R)
        {
            Angle = R.ReadObject<Angle>();
        }

        public override void Write(NetworkWriter W)
        {
            Angle.Write(W);
        }
    }
}
