using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juixel
{
    public enum DeviceType
    {
        iOS = 1,
        Android = 2,
        Mobile = 3,
        PC = 4,
        Mac = 8,
        Desktop = 12
    }

    public class JuixelTime
    {
        public double ElapsedMS;
        public double ElapsedSec;

        public double TotalMS;
        public double TotalSec;
    }
}
