using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juixel
{
    public enum DeviceType
    {
        iOS,
        Android,
        PC,
        Mac
    }

    public class JuixelTime
    {
        public double ElapsedMS;
        public double ElapsedSec;

        public double TotalMS;
        public double TotalSec;
    }
}
