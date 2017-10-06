using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestGameUtilities.Definitions.Net;
using TestGameUtilities.Definitions.Net.Client;
using TestGameUtilities.Definitions.Net.Client.FromClient;
using Utilities.Net;

namespace Clans.Networking
{
    public class Client
    {
        public const string Game_IP = "127.0.0.1";

        public double Latency => _UDP.Latency;
        private UDPClient _UDP;

        public Client(int Port)
        {
            _UDP = new UDPClient(new IPEndPoint(IPAddress.Parse(Game_IP), Port), 0);
            _UDP.Send(new GMHelloPacket
            {
                Username = "Test123",
                Key = "KeyTest123",
                Version = "1.0"
            });
        }
    }
}
