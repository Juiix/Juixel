using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldBuilderLib
{
    public interface IImporter
    {
        void LoadFile(Stream Stream);

        int GetWidth();

        int GetHeight();

        string GetFileFilter();

        ushort[,] GetTiles();
    }
}
