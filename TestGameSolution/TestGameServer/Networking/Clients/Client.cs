using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestGameUtilities.Definitions.Net;
using TestGameUtilities.Definitions.Net.Client;
using Utilities.Logging;

namespace TestGameServer.Networking.Clients
{
    public class Client
    {
        public ClientManager Manager;
        public string Username;

        private UDPClient _UDP;
        private int _Port;

        #region Init

        public Client(string Username, EndPoint EndPoint, int Port)
        {
            this.Username = Username;

            _Port = Port;
            _UDP = new UDPClient(EndPoint, Port);
            _UDP.OnReceivedPacket = OnReceivePacket;
            _UDP.OnDisconnect = OnDisconnect;
        }

        #endregion

        #region Packets

        public void Send(GMPacket Packet)
        {
            _UDP.Send(Packet);
        }

        private void OnReceivePacket(GMPacket Packet)
        {
            //Logger.Log($"Received {Packet.Type} Packet");
        }

        private void OnDisconnect()
        {
            Manager.ClientDisconnected(this);
        }

        #endregion

        #region Disposal

        public void Dispose(string Reason)
        {
            _UDP.Dispose(Reason);
            Manager.FreePort(_Port);
            Manager = null;
        }

        #endregion
    }
}
