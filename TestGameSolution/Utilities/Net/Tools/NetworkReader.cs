using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Net.Tools
{
    public class NetworkReader
    {
        #region Properties

        /// <summary>
        /// The inner <see cref="BinaryReader"/> used to read from the stream
        /// </summary>
        private BinaryReader _InnerReader;

        /// <summary>
        /// The inner <see cref="Stream"/> that is being read
        /// </summary>
        public Stream BaseStream => _InnerReader.BaseStream;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize <see cref="NetworkReader"/> with a provided <see cref="Stream"/>
        /// </summary>
        /// <param name="Stream">The <see cref="Stream"/> to write to</param>
        public NetworkReader(Stream Stream)
        {
            Initialize(Stream);
        }

        /// <summary>
        /// Initializes the <see cref="NetworkWriter"/> with a stream
        /// </summary>
        /// <param name="Stream">The <see cref="Stream"/> to write to</param>
        private void Initialize(Stream Stream)
        {
            _InnerReader = new BinaryReader(Stream);
        }

        #endregion

        #region Read Methods

        public double ReadDouble() => _InnerReader.ReadDouble();

        public float ReadFloat() => _InnerReader.ReadSingle();

        public int ReadInt() => _InnerReader.ReadInt32();

        public short ReadShort() => _InnerReader.ReadInt16();

        public byte ReadByte() => _InnerReader.ReadByte();

        public uint ReadUInt() => _InnerReader.ReadUInt32();

        public ushort ReadUShort() => _InnerReader.ReadUInt16();

        public sbyte ReadSByte() => _InnerReader.ReadSByte();

        public byte[] ReadBytes32() => _InnerReader.ReadBytes(_InnerReader.ReadInt32());

        public byte[] ReadBytes16() => _InnerReader.ReadBytes(_InnerReader.ReadInt16());

        public byte[] ReadBytes8() => _InnerReader.ReadBytes(_InnerReader.ReadByte());

        public T ReadObject<T>() where T : IReadable => Init.ReadObject<T>(this);

        public T[] ReadObjects32<T>() where T : IReadable => ReadObjects<T>(ReadInt());

        public T[] ReadObjects16<T>() where T : IReadable => ReadObjects<T>(ReadShort());

        public T[] ReadObjects8<T>() where T : IReadable => ReadObjects<T>(ReadByte());

        public T[] ReadObjects<T>(int Count) where T : IReadable
        {
            T[] Array = new T[Count];
            for (int i = 0; i < Count; i++)
                Array[i] = ReadObject<T>();
            return Array;
        }

        public string ReadUTF32() => Encoding.UTF8.GetString(ReadBytes32());

        public string ReadUTF16() => Encoding.UTF8.GetString(ReadBytes16());

        public string ReadUTF8() => Encoding.UTF8.GetString(ReadBytes8());

        #endregion

    }
}
