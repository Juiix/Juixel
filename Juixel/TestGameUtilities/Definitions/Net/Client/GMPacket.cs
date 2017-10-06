using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Net;
using Utilities.Net.Tools;

namespace TestGameUtilities.Definitions.Net.Client
{
    public class GMPacket : Packet
    {
        private const ushort Protocol_Id = 61174;

        public const ushort Header_Length = 10;

        public virtual GMPacketType Type => GMPacketType.Unknown;

        protected override byte Id => (byte)Type;

        private static ThreadLocal<Dictionary<byte, Type>> _PacketTypes = new ThreadLocal<Dictionary<byte, Type>>(() =>
        {
            Type PType = typeof(GMPacket);
            return PType.Assembly.GetTypes().Where(_ => _.IsSubclassOf(PType)).ToDictionary(_ => ((GMPacket)(Activator.CreateInstance(_))).Id);
        });

        public virtual bool IgnoreLoss => true;

        public ushort SequenceId;

        public ushort SequenceAck;

        public uint AckBitfield;

        public override byte[] GetData()
        {
            var W = new NetworkWriter();
            //W.Write((ushort)0); // Space for payload size
            W.Write(Protocol_Id);
            W.Write(Id);
            W.Write(SequenceId);
            W.Write(SequenceAck);
            W.Write(AckBitfield);
            Write(W);

            byte[] Bytes = ((MemoryStream)W.BaseStream).ToArray();
            /*
            ushort PayloadLength = (ushort)(Bytes.Length - 2); // Add the size of the payload
            byte[] SizeData = BitConverter.GetBytes(PayloadLength);
            Bytes[0] = SizeData[0];
            Bytes[1] = SizeData[1];
            */
            return Bytes;
        }

        public static GMPacket Create(byte[] Data)
        {
            using (var Stream = new MemoryStream(Data))
            {
                NetworkReader R = new NetworkReader(Stream);
                if (R.ReadUShort() != Protocol_Id)
                    throw new InvalidDataException("The received packet does not have the correct Protocol Id");
                GMPacket Packet = (GMPacket)(Activator.CreateInstance(_PacketTypes.Value[R.ReadByte()]));
                Packet.SequenceId = R.ReadUShort();
                Packet.SequenceAck = R.ReadUShort();
                Packet.AckBitfield = R.ReadUInt();
                Packet.Read(R);
                return Packet;
            }
        }
    }
}
