using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net.Tools;

namespace Utilities.Net
{
    public abstract class Packet : IPacket
    {
        protected virtual byte Id => 0;

        public virtual byte[] GetData()
        {
            var W = new NetworkWriter();
            W.Write(Id);
            Write(W);
            return ((MemoryStream)W.BaseStream).ToArray();
        }

        public virtual void Read(NetworkReader R)
        {
            
        }

        public virtual void Write(NetworkWriter W)
        {
            
        }
    }
}
