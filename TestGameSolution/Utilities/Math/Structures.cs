using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.JMath;
using Utilities.Net;
using Utilities.Net.Tools;
using Utilities.Tools;

namespace Utilities
{
    public struct Location : IReadable, IWritable
    {
        public double X;
        public double Y;

        /// <summary>
        /// Initialize a <see cref="Point"/> with X and Y values
        /// </summary>
        /// <param name="X">The X value of the point</param>
        /// <param name="Y">The Y value of the point</param>
        public Location(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Initilize a <see cref="Point"/> with a value for X and Y
        /// </summary>
        /// <param name="XY">The X and Y value</param>
        public Location(double XY)
        {
            X = XY;
            Y = XY;
        }

        /// <summary>
        /// A Point with (0, 0)
        /// </summary>
        public static Location Zero = new Location();

        /// <summary>
        /// A Point with (1, 1)
        /// </summary>
        public static Location One = new Location(1);

        /// <summary>
        /// A random <see cref="Location"/> with X and Y values between 0.0 and 1.0
        /// </summary>
        public static Location Random => new Location(JRandom.NextDouble(), JRandom.NextDouble());

        public double Length => System.Math.Sqrt(X * X + Y * Y);

        public Angle Angle => new Angle(System.Math.Atan2(Y, X));

        public Location Difference(Location From) => new Location(X - From.X, Y - From.Y);

        public double Distance(Location To) => To.Difference(this).Length;

        public Location Add(Location O) => new Location(X + O.X, Y + O.Y);

        public Location Add(double O) => new Location(X + O, Y + O);

        public Location Multiply(Location O) => new Location(X * O.X, Y * O.Y);

        public Location Divide(Location O) => new Location(X / O.X, Y / O.Y);

        public Location Scale(double O) => new Location(X * O, Y * O);

        public Location Lerp(Location To, double Scalar) => Add((To - this) * Scalar);

        public Location Floor() => new Location((int)X, (int)Y);

        public Location Round() => new Location((int)(X + 0.5), (int)(Y + 0.5));

        public IntLocation Int => new IntLocation((int)(X + 0.5), (int)(Y + 0.5));

        public IntLocation IntFloor => new IntLocation((int)X, (int)Y);

        public void Read(NetworkReader R)
        {
            X = R.ReadDouble();
            Y = R.ReadDouble();
        }

        public void Write(NetworkWriter W)
        {
            W.Write(X);
            W.Write(Y);
        }

        public static Location operator +(Location L, Location R) => L.Add(R);
        public static Location operator -(Location L, Location R) => L.Difference(R);
        public static Location operator *(Location L, Location R) => L.Multiply(R);
        public static Location operator /(Location L, Location R) => L.Divide(R);

        public static Location operator +(Location L, double R) => L.Add(R);
        public static Location operator -(Location L, double R) => L.Add(-R);
        public static Location operator *(Location L, double R) => L.Scale(R);
        public static Location operator /(Location L, double R) => L.Scale(1 / R);

        public static bool operator ==(Location L, Location R) => (L.X == R.X && L.Y == R.Y);
        public static bool operator !=(Location L, Location R) => (L.X != R.X || L.Y != R.Y);

        public static implicit operator Location(double Value) => new Location(Value, Value);

        public override string ToString() => $"{{{X}, {Y}}}";
    }

    public struct IntLocation
    {
        public int X;
        public int Y;

        /// <summary>
        /// Initialize an <see cref="IntLocation"/> with X and Y values
        /// </summary>
        /// <param name="X">The X value of the point</param>
        /// <param name="Y">The Y value of the point</param>
        public IntLocation(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Initilize an <see cref="IntLocation"/> with a value for X and Y
        /// </summary>
        /// <param name="XY">The X and Y value</param>
        public IntLocation(int XY)
        {
            X = XY;
            Y = XY;
        }

        public static IntLocation Zero = new IntLocation();
        public static IntLocation One = new IntLocation(1);

        public override string ToString() => $"{{{X}, {Y}}}";
    }

    public struct Angle: IReadable, IWritable
    {
        private double _Radians;
        public double Radians
        {
            get
            {
                return _Radians;
            }
            set
            {
                _Radians = Normalize(value);
            }
        }

        public double Degrees
        {
            get => Radians * 180.0 / System.Math.PI;
            set => Radians = value * System.Math.PI / 180.0;
        }

        public Angle(double Value, bool Radians = true)
        {
            if (Radians)
                _Radians = Value;
            else
                _Radians = ToRadians(Value);
            _Radians = Normalize(_Radians);
        }

        public Angle Add(double Value, bool Radians = true)
        {
            if (!Radians)
                Value = ToRadians(Value);
            return new Angle(this.Radians + Value);
        }

        public Location Location => new Location(System.Math.Cos(Radians), System.Math.Sin(Radians));
        public double Difference(Angle Other)
        {
            double A = Other.Radians - Radians;
            return Math.Abs(Functions.Mod((A + System.Math.PI * 2), System.Math.PI * 2) - System.Math.PI);
        }

        public static Angle Random => new Angle(System.Math.PI * 2 * JRandom.NextDouble());
        public static double ToRadians(double Degrees) => Degrees * System.Math.PI / 180.0;
        public static double ToDegrees(double Radians) => Radians * 180.0 / System.Math.PI;
        public static double Normalize(double Radians)
        {
            return Functions.Mod(Radians, System.Math.PI * 2);
        }

        public void Write(NetworkWriter W)
        {
            W.Write(Radians);
        }

        public void Read(NetworkReader R)
        {
            Radians = R.ReadDouble();
        }

        public static Angle operator +(Angle Left, Angle Right) => Left.Add(Right.Radians, true);
        public static Angle operator +(Angle Left, double Right) => Left.Add(Right, false);
        public static Angle operator -(Angle Left, double Right) => Left.Add(-Right, false);
        public static Angle operator /(Angle Left, double Right) => new Angle(Left.Radians / Right);


        public static implicit operator Angle(double Degrees) => new Angle(Degrees, false);
    }
}
