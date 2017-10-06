using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utilities.Crypto;
using Utilities.Logging;
using Utilities.Threading;
using static Utilities.Logging.Logger;

namespace Utilities.Net
{
    public class NetworkConnection<TPacket> where TPacket : IPacket
    {
        #region Properties

        private const int Send_Timeout = 10_000;
        private const int Receive_Timeout = 10_000;

        /// <summary>
        /// The base <see cref="Socket"/> of this connection
        /// </summary>
        private Socket _Socket;

        /// <summary>
        /// <see cref="RC4"/> algorithm used to encrypt sent data
        /// </summary>
        private RC4 _SendCrypt;
        /// <summary>
        /// <see cref="RC4"/> algorithm used to decrypt received data
        /// </summary>
        private RC4 _ReceiveCrypt;

        /// <summary>
        /// The buffer to hold received network data
        /// </summary>
        private NetworkBuffer _Buffer;

        /// <summary>
        /// When this <see cref="NetworkConnection{TPacket}"/> last received bytes
        /// </summary>
        public DateTime LastReceived = DateTime.Now;

        /// <summary>
        /// The type of Logging this object will do
        /// </summary>
        public virtual LogType LogType => LogType.Minimal;

        /// <summary>
        /// The <see cref="ProtocolType"/> that the base <see cref="Socket"/> uses
        /// </summary>
        public ProtocolType Protocol;

        public EndPoint RemoteEndPoint => _Socket.RemoteEndPoint;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new <see cref="NetworkConnection{TPacket}"/>, without connecting
        /// </summary>
        /// <param name="Type">The <see cref="SocketType"/> of the underlying <see cref="Socket"/></param>
        /// <param name="Protocol">The <see cref="ProtocolType"/> of the underlying <see cref="Socket"/></param>
        /// <param name="PacketSizeLength">The amount of <see langword="bytes"/> used to represent packet size</param>
        public NetworkConnection(SocketType Type, ProtocolType Protocol, int PacketSizeLength)
        {
            this.Protocol = Protocol;

            _Socket = new Socket(Type, Protocol);

            _Socket.SendTimeout = Send_Timeout;
            _Socket.ReceiveTimeout = Receive_Timeout;

            _Buffer = new NetworkBuffer(BufferState.Size, 2);
        }

        /// <summary>
        /// Initializes a new <see cref="NetworkConnection{TPacket}"/> with an established socket
        /// </summary>
        /// <param name="Socket">The connected <see cref="Socket"/></param>
        /// <param name="PacketSizeLength">The amount of <see langword="bytes"/> used to represent packet size</param>
        public NetworkConnection(Socket Socket, int PacketSizeLength)
        {
            _Socket = Socket;

            _Socket.SendTimeout = Send_Timeout;
            _Socket.ReceiveTimeout = Receive_Timeout;

            _Socket.NoDelay = true;

            _Buffer = new NetworkBuffer(BufferState.Size, 2);
        }

        /// <summary>
        /// Adds encryption to this <see cref="NetworkConnection{TPacket}"/>. <see langword="Null"/> value will add no encryption
        /// </summary>
        /// <param name="SendKey"><see cref="RC4Key"/> for sending encryption</param>
        /// <param name="ReceiveKey"><see cref="RC4Key"/> for decrypting received data</param>
        public void AddRC4(RC4Key SendKey = null, RC4Key ReceiveKey = null)
        {
            if (SendKey != null)
                _SendCrypt = new RC4(SendKey);
            if (ReceiveKey != null)
                _ReceiveCrypt = new RC4(ReceiveKey);
        }

        #endregion

        #region Connection

        public void Connect(string Host, int Port, Action<bool> Callback)
        {
            try
            {
                _Socket.BeginConnect(Host, Port, ConnectCallback, Callback);
            }
            catch (Exception E) // Failed to connect
            {
                Logger.Log(E, true);
                Callback(false);
            }
        }

        public void Connect(EndPoint EndPoint, Action<bool> Callback)
        {
            try
            {
                _Socket.BeginConnect(EndPoint, ConnectCallback, Callback);
            }
            catch (Exception E) // Failed to connect
            {
                Logger.Log(E, true);
                Callback(false);
            }
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            Action<bool> Callback = (Action<bool>)AR.AsyncState;
            try
            {
                _Socket.EndConnect(AR);
                Callback(true);
            }
            catch (Exception E)
            {
                Log(E, true);
                Callback(false);
            }
        }

        #endregion

        #region Receive

        private void BeginReceive(int Offset, int Amount)
        {
            try
            {
                _Socket.BeginReceive(_Buffer.Buffer, Offset, Amount, SocketFlags.None, ReceiveCallback, null);
            }
            catch (Exception E)
            {
                Log(E, true);
                Dispose();
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int Read = _Socket.EndReceive(AR);
                _Buffer.Position += Read;
                LastReceived = DateTime.Now;

                if (Read <= 0)
                    Dispose();
                else if (_Buffer.RemainingBytes > 0)
                    BeginReceive(_Buffer.Position, _Buffer.RemainingBytes);
                else if (_Buffer.State == BufferState.Size)
                {
                    byte[] Size = new byte[4];
                    Array.Copy(_Buffer.Buffer, 0, Size, 0, _Buffer.Buffer.Length);
                    _Buffer.Reset(BufferState.Data, BitConverter.ToInt32(Size, 0));
                    BeginReceive(_Buffer.Position, _Buffer.RemainingBytes);
                }
                else
                {
                    OnReceivePacket(_Buffer.GetData(), this);
                    _Buffer.Reset(BufferState.Size, 2);

                    BeginReceive(_Buffer.Position, _Buffer.RemainingBytes);
                }
            }
            catch (Exception E)
            {
                Log(E, true);
                Dispose();
            }
        }

        #endregion

        #region Sending

        private ConcurrentQueue<TPacket> _PacketsToSend = new ConcurrentQueue<TPacket>();
        private bool _Sending = false;

        public void Send(TPacket Packet)
        {
            if (_Sending)
                _PacketsToSend.Enqueue(Packet);
            else
            {
                _Sending = true;
                SendBytes(Packet.GetData());
            }
        }

        private void SendBytes(byte[] Bytes)
        {
            _Socket.BeginSend(Bytes, 0, Bytes.Length, SocketFlags.None, SendCallback, null);
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                int SentBytes = _Socket.EndSend(AR);
                _Sending = false;
            }
            catch (Exception E)
            {
                Log(E, true);
                Dispose();
            }
        }

        #endregion

        #region Events

        public event Action<byte[], NetworkConnection<TPacket>> OnReceivePacket;
        public event Action<NetworkConnection<TPacket>> OnDisconnect;

        #endregion

        #region Disposal

        public void Dispose()
        {
            if (_Socket != null)
            {
                _Socket.Dispose();
                _Socket = null;

                OnDisconnect(this);
            }
        }

        #endregion

    }

    public class NetworkBuffer
    {
        private const int Max_Buffer_Size = 65536;

        public byte[] Buffer = new byte[Max_Buffer_Size];
        public int Position = 0;
        public int Size = 2;
        public BufferState State = BufferState.Size;

        public int RemainingBytes => Size - Position;

        public NetworkBuffer(BufferState State, int Size)
        {
            Reset(State, Size);
        }

        public void Reset(BufferState State, int Size)
        {
            if (Size > Max_Buffer_Size)
                throw new InvalidOperationException(Size + " is larger than the preset buffer size");
            this.Size = Size;
            this.State = State;
            Position = 0;
        }

        public byte[] GetData()
        {
            byte[] Data = new byte[Position];
            Array.Copy(Buffer, 0, Data, 0, Position);
            return Data;
        }
    }

    public enum BufferState
    {
        Header,
        Size,
        Data
    }

}
