using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TestGameUtilities.Definitions.Net;
using TestGameUtilities.Definitions.Net.Client;
using TestGameUtilities.Definitions.Net.Client.FromClient;
using TestGameUtilities.Definitions.Net.Client.ToClient;

namespace Clans.Networking
{
    public class ConnectionClient
    {
        private UDPClient _UDP;
        private Action<int> _OnConnect;

        private bool Done = false;

        public void Connect(Action<int> OnConnect)
        {
            _OnConnect = OnConnect;

            _UDP = new UDPClient(new IPEndPoint(IPAddress.Parse(Client.Game_IP), Ports.Game_Start), 0);
            _UDP.OnReceivedPacket = OnReceivePacket;
            _UDP.Send(new GMHelloPacket
            {
                Username = "Test123",
                Key = "KeyTest123",
                Version = "1.0"
            });
            _UDP.OnDisconnect = OnTimeout;
        }

        private void OnReceivePacket(GMPacket Packet)
        {
            if (Packet.Type == GMPacketType.Connect)
            {
                if (!Done)
                {
                    Done = true;
                    GMConnectPacket Connect = (GMConnectPacket)Packet;
                    _OnConnect?.Invoke(Connect.Port);
                }
            }
            else if(!Done)
            {
                Done = true;
                _OnConnect?.Invoke(0);
            }

        }

        private void OnTimeout()
        {
            if (!Done)
            {
                Done = true;
                _OnConnect?.Invoke(0);
            }
        }

        public void Dispose()
        {
            _UDP.Dispose("Disconnected");
        }
    }
}
