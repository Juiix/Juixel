using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldBuilderLib
{
    public interface IExporter
    {
        void ProcessTile(uint X, uint Y, ushort Type);

        string GetFileExtension();

        void Save(string FilePath);
    }
}
