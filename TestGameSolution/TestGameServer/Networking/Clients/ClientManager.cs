using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestGameUtilities.Definitions.Net;
using TestGameUtilities.Definitions.Net.Client;
using TestGameUtilities.Definitions.Net.Client.FromClient;
using TestGameUtilities.Definitions.Net.Client.ToClient;
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
        private const int Connections_Till_Block = 64;

        /// <summary>
        /// The collection of connected clients
        /// </summary>
        private ConcurrentDictionary<string, Client> Clients = new ConcurrentDictionary<string, Client>();

        /// <summary>
        /// Connection count per ip
        /// </summary>
        private ConcurrentDictionary<string, int> ConnectionCounts = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// All connections currently being blocked
        /// </summary>
        private HashSet<string> BlockedConnections = new HashSet<string>();

        /// <summary>
        /// Currently used ports
        /// </summary>
        private HashSet<int> UsedPorts = new HashSet<int>();

        /// <summary>
        /// The UDP "catcher". Used to establish a connection with clients
        /// </summary>
        private UDPDispatcher UdpDispatcher;

        public LogType LogType = LogType.Verbose;

        public ClientManager()
        {
            UdpDispatcher = new UDPDispatcher(Ports.Game_Start);
            UdpDispatcher.Start(ClientConnected);

            Log("Awaiting Connections on Port " + Ports.Game_Start);
        }

        /// <summary>
        /// Called when data is received in the dispatcher
        /// </summary>
        /// <param name="EndPoint"></param>
        /// <param name="Received"></param>
        /// <returns></returns>
        private byte[] ClientConnected(IPEndPoint EndPoint, byte[] Received)
        {
            string IP = EndPoint.Address.ToString();
            if (!BlockedConnections.Contains(IP))
            {
                int Count = 0;
                if (ConnectionCounts.TryGetValue(IP, out Count))
                {
                    Count++;
                    if (Count >= Connections_Till_Block)
                    {
                        BlockedConnections.Add(IP);
                        return null;
                    }
                    else
                        ConnectionCounts.TryUpdate(IP, Count, Count - 1);
                }
                else
                    ConnectionCounts.TryAdd(IP, 1);
            }
            else
                return new GMDisconnectPacket() { Reason = "Protocol Error" }.GetData();

            try
            {
                GMPacket Packet = GMPacket.Create(Received);
                if (Packet.Type == GMPacketType.Hello)
                {
                    GMHelloPacket Hello = (GMHelloPacket)Packet;
                    int Port = GetPort();

                    Log("Received Hello: " + Hello.Username);

                    if (Port != -1)
                    {
                        Client C = new Client(Hello.Username, EndPoint, Port);
                        C.Manager = this;
                        if (Clients.TryAdd(Hello.Username, C))
                            return new GMConnectPacket() { Port = Port }.GetData();
                        else
                        {
                            C.Dispose("Account In Use");
                            return new GMDisconnectPacket() { Reason = "Account In Use" }.GetData();
                        }
                    }
                    else
                        return new GMDisconnectPacket() { Reason = "Server is Full" }.GetData();
                }
                else
                    return new GMDisconnectPacket() { Reason = "Protocol Error" }.GetData();
            }
            catch (Exception E)
            {
                Log(E, true);
                return new GMDisconnectPacket() { Reason = "Protocol Error" }.GetData();
            }
        }

        public void BroadcastPacket(GMPacket Packet)
        {
            Client[] Clients = this.Clients.Values.ToArray();
            if (Clients.Length >= 20)
                Parallel.ForEach(Clients, (Client) => Client.Send(Packet));
            else
                for (int i = 0; i < Clients.Length; i++)
                    Clients[i].Send(Packet);
        }

        public void ClientDisconnected(Client Client)
        {
            Log(Client.Username + " Disconnected");

            Client.Dispose("Disconnected");
            if (Clients.TryRemove(Client.Username, out Client))
            {

            }
        }

        #region Ports

        private int _CurrentPort = Ports.Game_Min;

        private object _PortLock = new object();

        private int GetPort()
        {
            lock (_PortLock)
            {
                int Port;
                int BreakCount = 800;

                do
                {
                    Port = _CurrentPort++;
                    if (_CurrentPort > Ports.Game_Max)
                        _CurrentPort = Ports.Game_Min;
                    BreakCount--;
                }
                while (UsedPorts.Contains(Port) && BreakCount > 0);

                if (BreakCount <= 0)
                    return -1;

                UsedPorts.Add(Port);
                return Port;
            }
        }

        public void FreePort(int Port)
        {
            UsedPorts.Remove(Port);
        }

        #endregion
    }
}
