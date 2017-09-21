using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestGameUtilities.Definitions.Net.Client;
using Utilities.Net;

namespace TestGameServer.Networking.Clients
{
    public class GMPacketHandler : PacketHandler<GMPacket, Client>
    {
        public override byte HandlesId => (byte)Handles;

        public virtual GMPacketType Handles => GMPacketType.Unknown;

        private static ThreadLocal<Dictionary<GMPacketType, GMPacketHandler>> _Handlers = new ThreadLocal<Dictionary<GMPacketType, GMPacketHandler>>(() =>
        {
            Type PType = typeof(GMPacketHandler);
            return PType.Assembly.GetTypes().Where(_ => _.IsSubclassOf(PType)).Select(_ => (GMPacketHandler)Activator.CreateInstance(_)).ToDictionary(_ => _.Handles);
        });

        public static void Handle(GMPacket Packet, Client Connection)
        {
            _Handlers.Value[Packet.Type].HandlePacket(Packet, Connection);
        }
    }
}
