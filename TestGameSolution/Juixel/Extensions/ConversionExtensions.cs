using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Juixel.Extensions
{
    public static class ConversionExtensions
    {
        public static Vector2 ToVector2(this Location Location) => new Vector2((float)Location.X, (float)Location.Y);
    }
}
