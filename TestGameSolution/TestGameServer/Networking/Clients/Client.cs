using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestGameUtilities.Definitions.Net.Client;
using Utilities.Logging;
using Utilities.Net;

namespace TestGameServer.Networking.Clients
{
    /// <summary>
    /// Used to communicate with a connected player client
    /// </summary>
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

        /// <summary>
        /// The <see cref="ClientManager"/> that contains this <see cref="Client"/>
        /// </summary>
        public ClientManager Manager;

        #region Init

        public Client()
        {

        }

        public void AddTCPConnection(NetworkConnection<GMPacket> Connection, Action<bool> SuccessCallback)
        {
            if (ConnectionTCP != null)
                throw new InvalidOperationException("This Client already has a TCP connection established!");
            ConnectionTCP = Connection;
            ConnectionTCP.OnReceivePacket += ReceivedPacket;

            ConnectionUDP = new NetworkConnection<GMPacket>(SocketType.Dgram, ProtocolType.Udp, 2);
            ConnectionUDP.Connect(Connection.RemoteEndPoint, SuccessCallback);
        }

        #endregion

        #region Sending and Receiving

        private void ReceivedPacket(byte[] Data, NetworkConnection<GMPacket> Connection)
        {
            try
            {
                GMPacket Packet = GMPacket.Create(Data);
                Logger.Log("Received Packet: " + Packet.Type);
            }
            catch (Exception E)
            {
                Logger.Log(E, true);
                Dispose();
            }
        }

        #endregion

        #region Disposal

        public void Dispose()
        {
            if (ConnectionTCP != null)
            {
                ConnectionTCP.Dispose();
                ConnectionTCP = null;
            }

            if (ConnectionUDP != null)
            {
                ConnectionUDP.Dispose();
                ConnectionUDP = null;
            }

            Manager = null;
        }

        #endregion
    }
}
