using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.JMath
{
    public static class Geometry
    {
        public static Location RotateAroundOrigin(Angle Rotation, Location Location)
        {
            if (Rotation.Radians == 0) return Location;
            double oX = Location.X;
            double oY = Location.Y;
            double sin = System.Math.Sin(Rotation.Radians);
            double cos = System.Math.Cos(Rotation.Radians);
            return new Location(cos * oX - sin * oY, sin * oX + cos * oY);
        }
    }
}
