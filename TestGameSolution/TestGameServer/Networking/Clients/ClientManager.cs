using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestGameUtilities.Definitions.Net.Client;
using TestGameUtilities.Definitions.Net.Client.FromClient;
using Utilities.Logging;
using Utilities.Net;
using static Utilities.Logging.Logger;

namespace TestGameServer.Networking.Clients
{
    /// <summary>
    /// Used to manage and manipulate client connections
    /// </summary>
    public class ClientManager
    {
        /// <summary>
        /// The collection of connected clients
        /// </summary>
        private ConcurrentDictionary<string, Client> Clients = new ConcurrentDictionary<string, Client>();

        /// <summary>
        /// All connections currently awaiting validation
        /// </summary>
        private HashSet<NetworkConnection<GMPacket>> UnvalidatedConnections = new HashSet<NetworkConnection<GMPacket>>();

        private NetworkListener TcpListener;

        public LogType LogType = LogType.Verbose;

        public ClientManager()
        {
            TcpListener = new NetworkListener(SocketType.Stream, ProtocolType.Tcp);

            TcpListener.Run(2525, (Socket) =>
            {
                NetworkConnection<GMPacket> Connection = new NetworkConnection<GMPacket>(Socket, 2);
                Connection.OnReceivePacket += ReceivedUnvalidatedBytes;
                UnvalidatedConnections.Add(Connection);
            });
        }

        private void ReceivedUnvalidatedBytes(byte[] Data, NetworkConnection<GMPacket> Connection)
        {
            try
            {
                Connection.OnReceivePacket -= ReceivedUnvalidatedBytes;

                GMPacket Packet = GMPacket.Create(Data);
                if (Packet.Type == GMPacketType.Hello)
                {
                    GMHelloPacket Hello = (GMHelloPacket)Packet;
                    Client Client;
                    if (!Clients.TryGetValue(Hello.Username, out Client))
                    {
                        Client = new Client();
                        if (Clients.TryAdd(Hello.Username, Client))
                        {
                            // Authenticate here, then proceed

                            UnvalidatedConnections.Remove(Connection);
                            Client.Manager = this;
                            Client.AddTCPConnection(Connection, (Connected) =>
                            {
                                if (Connected)
                                {

                                    if (LogType == LogType.Verbose)
                                        Log($"{Hello.Username} connected");
                                }
                                else
                                {

                                    if (LogType == LogType.Verbose)
                                        Log($"{Hello.Username} failed to connect", true);
                                }
                            });
                        }
                        else
                        {
                            UnvalidatedConnections.Remove(Connection);
                            Connection.Dispose();
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Logger.Log(E, true);
                Connection.Dispose();
            }
        }
    }
}
