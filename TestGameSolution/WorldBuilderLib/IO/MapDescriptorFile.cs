using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net;
using Utilities.Net.Tools;

namespace WorldBuilderLib.IO
{
    public class MapDescriptorFile : IWritable, IReadable
    {
        public const int Block_Size = 4096;
        public const uint Current_Version = 1;

        public uint Version;
        public uint BlockSize;
        public uint BlockWidth;
        public uint BlockHeight;
        public string[,] Blocks;

        public void Read(NetworkReader R)
        {
            Version = R.ReadUInt();
            BlockSize = R.ReadUInt();
            BlockWidth = R.ReadUInt();
            BlockHeight = R.ReadUInt();

            Blocks = new string[BlockWidth, BlockHeight];
            for (int y = 0; y < BlockHeight; y++)
                for (int x = 0; x < BlockWidth; x++)
                    Blocks[x, y] = R.ReadUTF32();
        }

        public void Write(NetworkWriter W)
        {
            W.Write(Current_Version);
            W.Write(Block_Size);
            W.Write(BlockWidth);
            W.Write(BlockHeight);

            for (int y = 0; y < BlockHeight; y++)
                for (int x = 0; x < BlockWidth; x++)
                    W.WriteUTF32(Blocks[x, y]);
        }
    }

    /*
     * public class BlockInfo : IWritable, IReadable
    {
        public string FileName;

        public void Read(NetworkReader R)
        {
            
        }

        public void Write(NetworkWriter W)
        {
            
        }
    }
    */

    public class BlockFile : IWritable, IReadable
    {
        public ushort Width;
        public ushort Height;
        public ushort[,] Tiles;

        public void Read(NetworkReader R)
        {
            Width = R.ReadUShort();
            Height = R.ReadUShort();
            Tiles = new ushort[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Tiles[x, y] = R.ReadUShort();
        }

        public void Write(NetworkWriter W)
        {
            W.Write(Width);
            W.Write(Height);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    W.Write(Tiles[x, y]);
        }
    }
}
