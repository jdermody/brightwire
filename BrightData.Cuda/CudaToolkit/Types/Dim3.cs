using System.Globalization;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Dim3(uint xValue, uint yValue, uint zValue)
    {
        public uint X { get; } = xValue;
        public uint Y { get; } = yValue;
        public uint Z { get; } = zValue;

        public Dim3(uint xValue, uint yValue) : this(xValue, yValue, 1)
        {
        }
        public Dim3(uint val) : this(val, 1, 1)
        {
        }
        public Dim3(int xValue, int yValue, int zValue) : this((uint)xValue, (uint)yValue, (uint)zValue)
        {
        }
        public Dim3(int xValue, int yValue) : this((uint)xValue, (uint)yValue, 1)
        {
        }
        public Dim3(int val) : this((uint)val, 1, 1)
        {
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
        public uint Size => (uint)Marshal.SizeOf<Dim3>();
    }
}
