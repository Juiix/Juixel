using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Utilities.Threading;

namespace TestGameServer.Networking
{
    public class UDPDispatcher
    {
        private int _Port;
        private Socket _Socket;
        private byte[] _ReceiveByteData;

        public UDPDispatcher(int Port)
        {
            _Port = Port;
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Start(Func<IPEndPoint, byte[], byte[]> OnReceive)
        {
            Tuple<Socket, Func<IPEndPoint, byte[], byte[]>> State = new Tuple<Socket, Func<IPEndPoint, byte[], byte[]>>(_Socket, OnReceive);

            _ReceiveByteData = new byte[1024];
            _Socket.Bind(new IPEndPoint(IPAddress.Any, _Port));

            EndPoint NewClientEP = new IPEndPoint(IPAddress.Any, 0);
            _Socket.BeginReceiveFrom(_ReceiveByteData, 0, _ReceiveByteData.Length, SocketFlags.None, ref NewClientEP, DoReceiveFrom, State);
        }

        private void DoReceiveFrom(IAsyncResult AR)
        {
            if (_Socket != null)
            {
                Tuple<Socket, Func<IPEndPoint, byte[], byte[]>> State = (Tuple<Socket, Func<IPEndPoint, byte[], byte[]>>)AR.AsyncState;
                Socket RecvSocket = State.Item1;

                try
                {
                    EndPoint ClientEP = new IPEndPoint(IPAddress.Any, 0);
                    int Length = RecvSocket.EndReceiveFrom(AR, ref ClientEP);

                    byte[] Data = new byte[Length];
                    Array.Copy(_ReceiveByteData, Data, Length);

                    EndPoint NewClientEP = new IPEndPoint(IPAddress.Any, 0);
                    _Socket.BeginReceiveFrom(_ReceiveByteData, 0, _ReceiveByteData.Length, SocketFlags.None, ref NewClientEP, DoReceiveFrom, State);

                    byte[] Send = State.Item2.Invoke((IPEndPoint)ClientEP, Data);
                    if (Send != null)
                        this.Send(ClientEP, Send);
                }
                catch (Exception E)
                {
                    Logger.Log(E, true);
                }
            }
        }

        /// <summary>
        /// Packets waiting to be sent
        /// </summary>
        private LockingList<Tuple<EndPoint, byte[]>> _SendQueue = new LockingList<Tuple<EndPoint, byte[]>>();

        /// <summary>
        /// <see langword="True"/> if the socket is currently sending data
        /// </summary>
        private bool _Sending = false;

        /// <summary>
        /// Sends a packet to the set <see cref="EndPoint"/>
        /// </summary>
        /// <param name="Packet">The <see cref="GMPacket"/> to send</param>
        /// <param name="Bypass">Should this send bypass the <see cref="_Sending"/> variable</param>
        public void Send(EndPoint EndPoint, byte[] Data, bool Bypass = false)
        {
            if (_Socket != null)
            {
                if (_Sending && !Bypass)
                {
                    _SendQueue.Add(new Tuple<EndPoint, byte[]>(EndPoint, Data));
                    return;
                }
                _Sending = true;
                _Socket.BeginSendTo(Data, 0, Data.Length, SocketFlags.None, EndPoint, OnSendTo, null);
            }
        }

        /// <summary>
        /// The <see cref="Socket"/> send callback
        /// </summary>
        /// <param name="AR"></param>
        private void OnSendTo(IAsyncResult AR)
        {
            if (_Socket != null)
            {
                try
                {
                    int Sent = _Socket.EndSendTo(AR);
                    Tuple<EndPoint, byte[]> InQueue = _SendQueue.TakeFirst();
                    if (InQueue != null)
                        Send(InQueue.Item1, InQueue.Item2, true);
                    else
                        _Sending = false;
                }
                catch (Exception E)
                {
                    Logger.Log(E, true);
                }
            }
        }

        public void Dispose()
        {
            if (_Socket != null)
            {
                _Socket.Close();
                _Socket.Dispose();
                _Socket = null;
            }

            Logger.Log("[UDP Dispatcher] Closed", true);
        }
    }
}
