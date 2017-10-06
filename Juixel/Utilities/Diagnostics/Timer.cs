using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Diagnostics
{
    public class Timer
    {
        private Stopwatch _Watch = new Stopwatch();

        public void Start()
        {
            _Watch.Start();
        }

        public double ElapsedReset
        {
            get
            {
                double MS = Elapsed;
                _Watch.Restart();
                return MS;
            }
        }

        public double Elapsed => (double)_Watch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

        public void Stop()
        {
            _Watch.Stop();
        }

        public void Reset()
        {
            _Watch.Reset();
        }

        public void Restart()
        {
            _Watch.Restart();
        }
    }
}
