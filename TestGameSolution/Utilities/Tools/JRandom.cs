using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Tools
{
    public class JRandom
    {
        private static int Seed = Environment.TickCount;
        private static ThreadLocal<Random> RandomPool = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref Seed)));

        private static Random GetRandom()
        {
            return RandomPool.Value;
        }

        public static double NextDouble() => GetRandom().NextDouble();

        public static int Next(int Max) => GetRandom().Next(Max);

        public static double Next(double Max) => GetRandom().NextDouble() * Max;

        public static double Next(int Min, int Max) => GetRandom().Next(Min, Max);

        public static double Next(double Min, double Max) => Min + GetRandom().NextDouble() * (Max - Min);
    }
}
