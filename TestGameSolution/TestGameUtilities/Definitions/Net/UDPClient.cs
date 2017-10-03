using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TestGameUtilities.Definitions.Net.Client;
using TestGameUtilities.Definitions.Net.Client.FromClient;
using TestGameUtilities.Definitions.Net.Client.ToClient;
using Utilities.Logging;
using Utilities.Net;
using Utilities.Threading;

namespace TestGameUtilities.Definitions.Net
{
    /// <summary>
    /// Used to communicate with a connected player client
    /// </summary>
    public class UDPClient
    {
        private const int Sent_Packet_Queue_Length = 48;

        private const int Ack_Length = 32;

        private const double Timeout_Seconds = 500;

        private const int Sequence_Wrap_Count = 32768;

        private const double Ping_Interval_Seconds = 1;

        /// <summary>
        /// The <see cref="Socket"/> used to communicate with the client
        /// </summary>
        private Socket _Socket;

        /// <summary>
        /// <see cref="EndPoint"/> of the remote client
        /// </summary>
        private EndPoint _RemoteEndPoint;

        /// <summary>
        /// Used to ping the remote client to determine timeout
        /// </summary>
        private Timer _PingTimer;

        /// <summary>
        /// The last ping id sent
        /// </summary>
        private byte _LastPingId = 0;

        /// <summary>
        /// The last time a ping packet was sent
        /// </summary>
        private DateTime _LastPingTime = DateTime.Now;

        /// <summary>
        /// The average round trip time of a packet
        /// </summary>
        public double Latency = 0;

        /// <summary>
        /// The local packet sequence
        /// </summary>
        private ushort _LocalSequence = ushort.MaxValue - 10;

        /// <summary>
        /// The highest sequence received
        /// </summary>
        private ushort _RemoteSequence = ushort.MaxValue - 10;

        /// <summary>
        /// The received packet sequences
        /// </summary>
        private bool[] _RemoteReceived = new bool[Ack_Length + 1];

        /// <summary>
        /// The last 128 packets that were sent. Used to resend lost packets if needed
        /// </summary>
        private ConcurrentDictionary<ushort, Tuple<GMPacket, DateTime>> _SentPackets = new ConcurrentDictionary<ushort, Tuple<GMPacket, DateTime>>();

        /// <summary>
        /// The buffer to hold received network data
        /// </summary>
        //private NetworkBuffer _ReceiveBuffer;
        private byte[] _ReceiveBuffer;

        /// <summary>
        /// The timestamp of the last received bytes
        /// </summary>
        private DateTime _ReceiveTimeout = DateTime.Now;

        private bool _HasReceived = false;

        /// <summary>
        /// Gets a value that determines if this <see cref="UDPClient"/> is connected to a remote client
        /// </summary>
        public bool Connected => _Connected;

        /// <summary>
        /// Internal Connected value
        /// </summary>
        private bool _Connected = true;

        #region Init

        /// <summary>
        /// A client that will receive from a given <see cref="EndPoint"/> on a given Port
        /// </summary>
        /// <param name="RemoteEndPoint"></param>
        /// <param name="Port"></param>
        public UDPClient(EndPoint RemoteEndPoint, int Port)
        {
            _RemoteEndPoint = RemoteEndPoint;
            _ReceiveBuffer = new byte[1024];

            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _Socket.Bind(new IPEndPoint(IPAddress.Any, Port));

            _PingTimer = new Timer(Ping_Interval_Seconds * 1000);
            _PingTimer.Elapsed += OnTimeoutCheck;
            _PingTimer.Start();

            //BeginReceive(_ReceiveBuffer., _ReceiveBuffer.RemainingBytes);
            BeginReceive();
        }

        private void OnTimeoutCheck(object Sender, ElapsedEventArgs E)
        {
            if (_Connected)
            {
                if (_HasReceived)
                {
                    Send(new GMPingPacket()
                    {
                        PingId = 0
                    });
                }
                DidTimeout();
            }
        }

        #endregion

        #region Receiving

        /// <summary>
        /// Starts the receive loop
        /// </summary>
        /// <param name="Offset">Where to start storing data</param>
        /// <param name="Amount">The amount of data to read</param>
        private void BeginReceive()
        {
            _Socket.BeginReceiveFrom(_ReceiveBuffer, 0, _ReceiveBuffer.Length, SocketFlags.None, ref _RemoteEndPoint, OnReceiveFrom, null);
        }

        /// <summary>
        /// The receive callback
        /// </summary>
        /// <param name="AR"></param>
        private void OnReceiveFrom(IAsyncResult AR)
        {
            if (_Connected)
            {
                try
                {
                    //EndPoint ClientEP = new IPEndPoint(IPAddress.Any, 0);
                    int Length = _Socket.EndReceiveFrom(AR, ref _RemoteEndPoint);

                    _ReceiveTimeout = DateTime.Now;
                    _HasReceived = true;

                    if (Length <= 0)
                        Dispose("Networking Error, 0x03");
                    else
                    {
                        byte[] Data = new byte[Length];
                        Array.Copy(_ReceiveBuffer, Data, Length);
                        ReceivedPacket(GMPacket.Create(Data));

                        BeginReceive();
                    }
                }
                catch (Exception E) // Disconnected
                {
                    Logger.Log(E, true);
                    Dispose("Networking Error, 0x04");
                }
            }
        }

        #endregion

        #region Sending

        /// <summary>
        /// Packets waiting to be sent
        /// </summary>
        private LockingList<GMPacket> _SendQueue = new LockingList<GMPacket>();

        /// <summary>
        /// <see langword="True"/> if the socket is currently sending data
        /// </summary>
        private bool _Sending = false;

        /// <summary>
        /// Sends a packet to the set <see cref="EndPoint"/>
        /// </summary>
        /// <param name="Packet">The <see cref="GMPacket"/> to send</param>
        /// <param name="Bypass">Should this send bypass the <see cref="_Sending"/> variable</param>
        public void Send(GMPacket Packet, bool Bypass = false)
        {
            if (!_Connected && Packet.Type != GMPacketType.Disconnect)
                return;

            if (_Sending && !Bypass)
            {
                _SendQueue.Add(Packet);
                return;
            }
            _Sending = true;

            Packet.SequenceId = _LocalSequence++;
            Packet.SequenceAck = _RemoteSequence;
            Packet.AckBitfield = CreateAckBitfield();

            _SentPackets.TryAdd(Packet.SequenceId, new Tuple<GMPacket, DateTime>(Packet, DateTime.Now));

            Tuple<GMPacket, DateTime> Out;
            if (_SentPackets.TryRemove((ushort)(Packet.SequenceId - Sent_Packet_Queue_Length), out Out))
                PacketLost(Out.Item1);

            byte[] Bytes = Packet.GetData();
            _Socket.BeginSendTo(Bytes, 0, Bytes.Length, SocketFlags.None, _RemoteEndPoint, OnSendTo, Packet);
        }

        /// <summary>
        /// The <see cref="Socket"/> send callback
        /// </summary>
        /// <param name="AR"></param>
        private void OnSendTo(IAsyncResult AR)
        {
            try
            {
                int Sent = _Socket.EndSendTo(AR);
                if (_Connected)
                {
                    GMPacket InQueue = _SendQueue.TakeFirst();
                    if (InQueue != null)
                        Send(InQueue, true);
                    else
                        _Sending = false;
                }
                else
                {
                    _Socket.Close();
                    _Socket.Dispose();
                    _Socket = null;
                }
            }
            catch (Exception E)
            {
                Logger.Log(E, true);
                Dispose("Networking Error, 0x01");
            }
        }

        /// <summary>
        /// Creates the acknowledgement bitfield for the last 32 packets
        /// </summary>
        private uint CreateAckBitfield()
        {
            uint Bitfield = 0;
            for (int i = _RemoteReceived.Length - 1; i > 0; i--)
            {
                Bitfield = Bitfield << 1;
                if (_RemoteReceived[i])
                    Bitfield = Bitfield | 1;
            }
            return Bitfield;
        }

        #endregion

        #region Packets

        public Action<GMPacket> OnReceivedPacket;

        /// <summary>
        /// Called when a <see cref="GMPacket"/> is received from the remote client
        /// </summary>
        /// <param name="Packet">The <see cref="GMPacket"/> received</param>
        private void ReceivedPacket(GMPacket Packet)
        {
            try
            {
                int SeqDif = SequenceDifference(_RemoteSequence, Packet.SequenceId);
                if (SeqDif > 0) // If the packet is newer
                {
                    _RemoteSequence = Packet.SequenceId;
                    Array.Copy(_RemoteReceived, 0, _RemoteReceived, SeqDif, _RemoteReceived.Length - SeqDif); // Shift Array
                    _RemoteReceived[0] = true;

                    for (int i = 1; i < SeqDif; i++) // Reset the unshifted elements if the SeqDif is > 1
                        _RemoteReceived[i] = false;
                }
                else if (SeqDif < 0 && SeqDif > -(_RemoteReceived.Length - 1)) // An older packet, if the sequence is out of the receive array, drop it
                    _RemoteReceived[Math.Abs(SeqDif)] = true;

                ParseAcknowledegment(Packet.SequenceAck, Packet.AckBitfield);

                if (Packet.Type == GMPacketType.Ping)
                    Send(new GMPongPacket
                    {
                        PingId = ((GMPingPacket)Packet).PingId
                    });

                OnReceivedPacket?.Invoke(Packet);
            }
            catch (Exception E)
            {
                Logger.Log(E, true);
                Dispose("Networking Error, 0x02");
            }
        }

        /// <summary>
        /// Parses the Ack Bitfield and the starting Ack Id
        /// </summary>
        /// <param name="StartId">The ack Id sent with the packet</param>
        /// <param name="Bitfield">The ack bitfield sent with the packet</param>
        private void ParseAcknowledegment(ushort StartId, uint Bitfield)
        {
            AcknowledgePacket(StartId);
            for (int i = 1; i < 33; i++)
            {
                ushort Id = (ushort)(StartId - i);
                if ((Bitfield & 1) == 1)
                    AcknowledgePacket(Id);
                Bitfield = Bitfield >> 1;
            }
        }

        /// <summary>
        /// Acknowledges a packet id that was sent
        /// </summary>
        /// <param name="Id">The sequence id of the packet</param>
        private void AcknowledgePacket(ushort Id)
        {
            Tuple<GMPacket, DateTime> Out;
            if (_SentPackets.TryRemove(Id, out Out)) // Packet sent successfully
            {
                double RTT = (DateTime.Now - Out.Item2).TotalMilliseconds;

                if (Latency == 0)
                    Latency = RTT;
                else
                    Latency += (RTT - Latency) * 0.1;
            }
            else // Packet was already acknowledged
            {

            }
        }

        /// <summary>
        /// Called when a packet has been determined as lost
        /// </summary>
        /// <param name="Packet">The packet that was lost</param>
        private void PacketLost(GMPacket Packet)
        {
            if (!Packet.IgnoreLoss) // Only resend if needed
                Send(Packet);
        }

        /// <summary>
        /// Gets the difference between two sequence Ids, accounts for ushort wrapping
        /// </summary>
        /// <param name="Old"></param>
        /// <param name="New"></param>
        /// <returns></returns>
        private int SequenceDifference(ushort Old, ushort New)
        {
            if (New > Old)
            {
                if (New - Old <= Sequence_Wrap_Count)
                    return New - Old;
                else
                    return ushort.MaxValue - New + Old;
            }
            else if (Old - New > Sequence_Wrap_Count)
                return ushort.MaxValue - Old + New;
            else
                return New - Old;
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Checks if this <see cref="UDPClient"/> has timed out. Disconnects if true
        /// </summary>
        /// <returns></returns>
        public bool DidTimeout()
        {
            bool Timeout = _ReceiveTimeout.AddSeconds(Timeout_Seconds) < DateTime.Now;
            if (Timeout)
                Disconnect("Connection Timed Out");
            return Timeout;
        }

        public Action OnDisconnect;

        /// <summary>
        /// "Disconnects" from a remote <see cref="EndPoint"/>
        /// </summary>
        private void Disconnect(string Reason)
        {
            if (_Connected)
            {
                _Connected = false;
                Send(new GMDisconnectPacket()
                {
                    Reason = Reason
                });

                //Logger.Log("UDP Disconnected.\n" + Environment.StackTrace, true);

                OnDisconnect?.Invoke();
            }
        }

        public void Dispose(string Reason)
        {
            Disconnect(Reason);

            _PingTimer.Stop();
            _PingTimer.Dispose();
        }

        #endregion
    }
}
