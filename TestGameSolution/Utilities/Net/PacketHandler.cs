using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Net
{
    /// <summary>
    /// Handles a received <see cref="Packet"/>
    /// </summary>
    /// <typeparam name="TPacket">The <see cref="Packet"/> that is received</typeparam>
    /// <typeparam name="TConnection">The type of <see cref="NetworkConnection{TPacket}"/> used</typeparam>
    public abstract class PacketHandler<TPacket, TConnection> 
        where TPacket : Packet
    {
        public virtual byte HandlesId => 0;

        public virtual void HandlePacket(TPacket Packet, TConnection Connection)
        {

        }
    }
}
