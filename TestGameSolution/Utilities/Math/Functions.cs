using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.JMath
{
    public static class Functions
    {
        public static double Mod(double A, double B)
        {
            return (A % B + B) % B;
        }
    }
}
