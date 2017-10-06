using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;
using WorldBuilderLib.IO;

namespace WorldBuilderLib
{
    public class MapDataLoader
    {
        public int BlockWidth;
        public int BlockHeight;

        public MapDescriptorFile MapFile;

        public string FileDirectory;
        public string FileBaseName;

        public MapDataLoader(string FileName)
        {
            FileDirectory = Path.GetDirectoryName(FileName);
            FileBaseName = Path.GetFileNameWithoutExtension(FileName);

            MapDescriptorFile MapFile;
            using (Stream FileStream = File.Open(FileName, FileMode.Open))
            {
                NetworkReader r = new NetworkReader(FileStream);
                MapFile = new MapDescriptorFile();
                MapFile.Read(r);
            }
            Init(MapFile);
        }

        private void Init(MapDescriptorFile MapFile)
        {
            this.MapFile = MapFile;

            BlockWidth = (int)MapFile.BlockWidth;
            BlockHeight = (int)MapFile.BlockHeight;
        }

        public BlockFile LoadBlock(int X, int Y)
        {
            string FileName = Path.Combine(FileDirectory, FileBaseName + "_" + (Y * BlockWidth + X) + ".mdata");
            BlockFile Block;
            using (Stream FileStream = File.Open(FileName, FileMode.Open))
            {
                NetworkReader r = new NetworkReader(FileStream);
                Block = new BlockFile();
                Block.Read(r);
            }
            return Block;
        }
    }
}
