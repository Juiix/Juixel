using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Net.Tools
{
    public class NetworkWriter
    {

#region Properties

        /// <summary>
        /// The inner <see cref="BinaryWriter"/> used to write to the stream
        /// </summary>
        private BinaryWriter _InnerWriter;

        /// <summary>
        /// The inner <see cref="Stream"/> that is being written to
        /// </summary>
        public Stream BaseStream => _InnerWriter.BaseStream;

        #endregion

#region Initialization

        /// <summary>
        /// Initialize <see cref="NetworkWriter"/> with a new <see cref="MemoryStream"/>
        /// </summary>
        public NetworkWriter()
        {
            Initialize(new MemoryStream());
        }

        /// <summary>
        /// Initialize <see cref="NetworkWriter"/> with a provided <see cref="Stream"/>
        /// </summary>
        /// <param name="Stream">The <see cref="Stream"/> to write to</param>
        public NetworkWriter(Stream Stream)
        {
            Initialize(Stream);
        }

        /// <summary>
        /// Initializes the <see cref="NetworkWriter"/> with a stream
        /// </summary>
        /// <param name="Stream">The <see cref="Stream"/> to write to</param>
        private void Initialize(Stream Stream)
        {
            _InnerWriter = new BinaryWriter(Stream);
        }

#endregion

#region Write Methods

        public void Write(double Value)
        {
            _InnerWriter.Write(Value);
        }

        public void Write(float Value)
        {
            _InnerWriter.Write(Value);
        }

        public void Write(int Value)
        {
            _InnerWriter.Write(Value);
        }

        public void Write(short Value)
        {
            _InnerWriter.Write(Value);
        }

        public void Write(byte Value)
        {
            _InnerWriter.Write(Value);
        }

        public void Write(uint Value)
        {
            _InnerWriter.Write(Value);
        }

        public void Write(ushort Value)
        {
            _InnerWriter.Write(Value);
        }

        public void Write(sbyte Value)
        {
            _InnerWriter.Write(Value);
        }

        public void WriteBytes32(byte[] Bytes)
        {
            Write(Bytes.Length);
            Write(Bytes);
        }

        public void WriteBytes16(byte[] Bytes)
        {
            Write((short)Bytes.Length);
            Write(Bytes);
        }

        public void WriteBytes8(byte[] Bytes)
        {
            Write((byte)Bytes.Length);
            Write(Bytes);
        }

        public void Write(byte[] Bytes)
        {
            _InnerWriter.Write(Bytes);
        }

        public void Write(IWritable Value)
        {
            Value.Write(this);
        }

        public void WriteObjects32(IWritable[] Values)
        {
            Write(Values.Length);
            WriteObjects(Values);
        }

        public void WriteObjects16(IWritable[] Values)
        {
            Write((short)Values.Length);
            WriteObjects(Values);
        }

        public void WriteObjects8(IWritable[] Values)
        {
            Write((byte)Values.Length);
            WriteObjects(Values);
        }
        
        private void WriteObjects(IWritable[] Values)
        {
            for (int i = 0; i < Values.Length; i++)
                Write(Values[i]);
        }

        public void WriteUTF32(string Value)
        {
            Write(Value.Length);
            _InnerWriter.Write(Encoding.UTF8.GetBytes(Value));
        }

        public void WriteUTF16(string Value)
        {
            Write((short)Value.Length);
            _InnerWriter.Write(Encoding.UTF8.GetBytes(Value));
        }

        public void WriteUTF8(string Value)
        {
            Write((byte)Value.Length);
            _InnerWriter.Write(Encoding.UTF8.GetBytes(Value));
        }

        #endregion
    }
}
