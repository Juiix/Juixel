using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utilities.Crypto;
using Utilities.Logging;
using static Utilities.Logging.Logger;

namespace Utilities.Net
{
    public class NetworkConnection<TPacket>
    {
        #region Properties

        private const int Send_Timeout = 4000;
        private const int Receive_Timeout = 4000;

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
        /// The type of Logging this object will do
        /// </summary>
        public virtual LogType LogType => LogType.Minimal;

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
            _Socket = new Socket(Type, Protocol);

            _Socket.SendTimeout = Send_Timeout;
            _Socket.ReceiveTimeout = Receive_Timeout;

            _Buffer = new NetworkBuffer(PacketSizeLength);
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
                _Buffer.Position += (ushort)Read;

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
                    OnReceivePacket(_Buffer.GetData());
                    _Buffer.Reset(BufferState.Size);

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



        #endregion

        #region Events

        public event Action<byte[]> OnReceivePacket;
        public event Action OnDisconnect;

        #endregion

        #region Disposal

        private void Dispose()
        {
            if (_Socket != null)
            {
                _Socket.Dispose();
                _Socket = null;

                OnDisconnect();
            }
        }

        #endregion

        private class NetworkBuffer
        {
            private const int Max_Buffer_Size = 65536;

            private int _SizeLength;

            public byte[] Buffer = new byte[Max_Buffer_Size];
            public int Position = 0;
            public int Size = 2;
            public BufferState State = BufferState.Size;

            public int RemainingBytes => Size - Position;

            public NetworkBuffer(int SizeLength)
            {
                _SizeLength = SizeLength;
                Size = SizeLength;
            }

            public void Reset(BufferState State, int? Size = null)
            {
                if (Size == null)
                    Size = _SizeLength;
                int S = Size.Value;

                if (S > Max_Buffer_Size)
                    throw new InvalidOperationException(Size + " is larger than the preset buffer size");
                this.Size = S;
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

        private enum BufferState
        {
            Size,
            Data
        }
    }
}
