using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestGameUtilities.Definitions.Net.Client;
using Utilities.Net;

namespace Clans.Networking
{
    public class Client
    {
        /// <summary>
        /// The <see cref="NetworkConnection{TPacket}"/> used to send TCP packets to the client
        /// </summary>
        private NetworkConnection<GMPacket> ConnectionTCP;

        /// <summary>
        /// The <see cref="NetworkConnection{TPacket}"/> used to send UDP packets to the client
        /// </summary>
        private NetworkConnection<GMPacket> ConnectionUDP;

        public Client()
        {
            ConnectionTCP = new NetworkConnection<GMPacket>(SocketType.Stream, ProtocolType.Tcp, 2);
            ConnectionUDP = new NetworkConnection<GMPacket>(SocketType.Dgram, ProtocolType.Udp, 2);
        }

        public void Connect(string Host, int Port, Action<bool> Callback)
        {
            ConnectionTCP.Connect(Host, Port, (TcpConnected) =>
            {
                if (TcpConnected)
                    ConnectionUDP.Connect(Host, Port, Callback);
                else
                    Callback(TcpConnected);
            });
        }
    }
}
