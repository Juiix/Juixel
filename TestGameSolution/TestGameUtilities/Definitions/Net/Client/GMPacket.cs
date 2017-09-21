using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Net;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client
{
    public class GMPacket : Packet
    {
        public virtual GMPacketType Type => GMPacketType.Unknown;

        protected override byte Id => (byte)Type;

        private static ThreadLocal<Dictionary<byte, Type>> _PacketTypes = new ThreadLocal<Dictionary<byte, Type>>(() =>
        {
            Type PType = typeof(GMPacket);
            return PType.Assembly.GetTypes().Where(_ => _.IsSubclassOf(PType)).ToDictionary(_ => ((GMPacket)(Activator.CreateInstance(_))).Id);
        });

        public static GMPacket Create(byte[] Data)
        {
            NetworkReader R = new NetworkReader(new MemoryStream(Data));
            GMPacket Packet = (GMPacket)(Activator.CreateInstance(_PacketTypes.Value[R.ReadByte()]));
            Packet.Read(R);
            return Packet;
        }
    }
}
