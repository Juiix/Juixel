using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;

namespace Utilities.Net
{
    public interface IWritable
    {
        void Write(NetworkWriter W);
    }

    public interface IReadable
    {
        void Read(NetworkReader R);
    }

    public interface IPacket : IWritable, IReadable
    {
        byte[] GetData();
    }
}
