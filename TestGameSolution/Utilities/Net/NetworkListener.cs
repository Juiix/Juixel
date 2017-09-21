using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Utilities.Net
{
    public class NetworkListener
    {
        public Socket Socket;

        private bool _Running = false;
        private Action<Socket> _OnReceive;

        private int _Port;

        public NetworkListener(SocketType SocketType, ProtocolType ProtocolType)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType, ProtocolType);
        }

        public void Run(int Port, Action<Socket> OnReceive)
        {
            _Port = Port;
            _OnReceive = OnReceive;
            _Running = true;
            Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            Socket.Listen(5);
            Socket.BeginAccept(AcceptCallback, null);

            Logger.Log($"Listening for Connections on Port: " + Port);
        }

        public void AcceptCallback(IAsyncResult AR)
        {
            if (_Running)
            {
                Socket Socket = this.Socket.EndAccept(AR);
                Logger.Log($"Connection ({((IPEndPoint)Socket.RemoteEndPoint).Address.ToString()}) Received on {_Port} Port");
                _OnReceive?.Invoke(Socket);
                this.Socket.BeginAccept(AcceptCallback, null);
            }
        }

        public void Stop()
        {
            _Running = false;
            try
            {
                Socket.Disconnect(true);
            }
            catch { }
        }

        public virtual void Dispose()
        {
            Socket.Dispose();
            _OnReceive = null;
        }
    }
}
