using System.Globalization;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Dim3
    {
        public uint X { get; }
        public uint Y { get; }
        public uint Z { get; }

        public Dim3(uint xValue, uint yValue, uint zValue)
        {
            X = xValue;
            Y = yValue;
            Z = zValue;
        }
        public Dim3(uint xValue, uint yValue)
        {
            X = xValue;
            Y = yValue;
            Z = 1;
        }
        public Dim3(uint val)
        {
            X = val;
            Y = 1;
            Z = 1;
        }
        public Dim3(int xValue, int yValue, int zValue)
        {
            X = (uint)xValue;
            Y = (uint)yValue;
            Z = (uint)zValue;
        }
        public Dim3(int xValue, int yValue)
        {
            X = (uint)xValue;
            Y = (uint)yValue;
            Z = 1;
        }
        public Dim3(int val)
        {
            X = (uint)val;
            Y = 1;
            Z = 1;
        }

        public static Dim3 Add(Dim3 src, Dim3 value)      => new(src.X + value.X, src.Y + value.Y, src.Z + value.Z);
        public static Dim3 Add(Dim3 src, uint value)      => new(src.X + value, src.Y + value, src.Z + value);
        public static Dim3 Add(uint src, Dim3 value)      => new(src + value.X, src + value.Y, src + value.Z);
        public static Dim3 Subtract(Dim3 src, Dim3 value) => new(src.X - value.X, src.Y - value.Y, src.Z - value.Z);
        public static Dim3 Subtract(Dim3 src, uint value) => new(src.X - value, src.Y - value, src.Z - value);
        public static Dim3 Subtract(uint src, Dim3 value) => new(src - value.X, src - value.Y, src - value.Z);
        public static Dim3 Multiply(Dim3 src, Dim3 value) => new(src.X * value.X, src.Y * value.Y, src.Z * value.Z);
        public static Dim3 Multiply(Dim3 src, uint value) => new(src.X * value, src.Y * value, src.Z * value);
        public static Dim3 Multiply(uint src, Dim3 value) => new(src * value.X, src * value.Y, src * value.Z);
        public static Dim3 Divide(Dim3 src, Dim3 value)   => new(src.X / value.X, src.Y / value.Y, src.Z / value.Z);
        public static Dim3 Divide(Dim3 src, uint value)   => new(src.X / value, src.Y / value, src.Z / value);
        public static Dim3 Divide(uint src, Dim3 value)   => new(src / value.X, src / value.Y, src / value.Z);


        public static Dim3 operator +(Dim3 src, Dim3 value) => Add(src, value);

        public static Dim3 operator +(Dim3 src, uint value) => Add(src, value);

        public static Dim3 operator +(uint src, Dim3 value) => Add(src, value);

        public static Dim3 operator -(Dim3 src, Dim3 value) => Subtract(src, value);

        public static Dim3 operator -(Dim3 src, uint value) => Subtract(src, value);

        public static Dim3 operator -(uint src, Dim3 value) => Subtract(src, value);

        public static Dim3 operator *(Dim3 src, Dim3 value) => Multiply(src, value);

        public static Dim3 operator *(Dim3 src, uint value) => Multiply(src, value);

        public static Dim3 operator *(uint src, Dim3 value) => Multiply(src, value);

        public static Dim3 operator /(Dim3 src, Dim3 value) => Divide(src, value);

        public static Dim3 operator /(Dim3 src, uint value) => Divide(src, value);

        public static Dim3 operator /(uint src, Dim3 value) => Divide(src, value);

        public static bool operator ==(Dim3 src, Dim3 value) => src.Equals(value);

        public static bool operator !=(Dim3 src, Dim3 value) => !(src == value);

        public static implicit operator Dim3(int value) => new(value);

        public static implicit operator Dim3(uint value) => new(value);

        public override bool Equals(object? obj) => obj is Dim3 value && Equals(value);
        public bool Equals(Dim3 value) => X == value.X && Y == value.Y && Z == value.Z;
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        public override string ToString() => string.Format(CultureInfo.CurrentCulture, "({0}; {1}; {2})", X, Y, Z);

        public static Dim3 Min(Dim3 aValue, Dim3 bValue) => new(System.Math.Min(aValue.X, bValue.X), System.Math.Min(aValue.Y, bValue.Y), System.Math.Min(aValue.Z, bValue.Z));
        public static Dim3 Max(Dim3 aValue, Dim3 bValue) => new(System.Math.Max(aValue.X, bValue.X), System.Math.Max(aValue.Y, bValue.Y), System.Math.Max(aValue.Z, bValue.Z));
        public uint Size => (uint)Marshal.SizeOf(this);
    }
}
