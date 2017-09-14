using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Net;
using Utilities.Net.Tools;

namespace Utilities
{
    public static class Init
    {
        public static T ReadObject<T>(NetworkReader R) where T : IReadable
        {
            T Object = (T)Activator.CreateInstance(typeof(T));
            Object.Read(R);
            return Object;
        }
    }
}
