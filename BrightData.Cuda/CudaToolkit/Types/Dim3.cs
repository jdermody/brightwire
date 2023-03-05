using System.Globalization;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Dim3
    {
        public uint x;
        public uint y;
        public uint z;
        public static Dim3 Add(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x + value.x, src.y + value.y, src.z + value.z);
            return ret;
        }
        public static Dim3 Add(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x + value, src.y + value, src.z + value);
            return ret;
        }
        public static Dim3 Add(uint src, Dim3 value)
        {
            var ret = new Dim3(src + value.x, src + value.y, src + value.z);
            return ret;
        }
        public static Dim3 Subtract(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x - value.x, src.y - value.y, src.z - value.z);
            return ret;
        }
        public static Dim3 Subtract(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x - value, src.y - value, src.z - value);
            return ret;
        }
        public static Dim3 Subtract(uint src, Dim3 value)
        {
            var ret = new Dim3(src - value.x, src - value.y, src - value.z);
            return ret;
        }
        public static Dim3 Multiply(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x * value.x, src.y * value.y, src.z * value.z);
            return ret;
        }
        public static Dim3 Multiply(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x * value, src.y * value, src.z * value);
            return ret;
        }
        public static Dim3 Multiply(uint src, Dim3 value)
        {
            var ret = new Dim3(src * value.x, src * value.y, src * value.z);
            return ret;
        }
        public static Dim3 Divide(Dim3 src, Dim3 value)
        {
            var ret = new Dim3(src.x / value.x, src.y / value.y, src.z / value.z);
            return ret;
        }
        public static Dim3 Divide(Dim3 src, uint value)
        {
            var ret = new Dim3(src.x / value, src.y / value, src.z / value);
            return ret;
        }
        public static Dim3 Divide(uint src, Dim3 value)
        {
            var ret = new Dim3(src / value.x, src / value.y, src / value.z);
            return ret;
        }
        public static Dim3 operator +(Dim3 src, Dim3 value)
        {
            return Add(src, value);
        }
        public static Dim3 operator +(Dim3 src, uint value)
        {
            return Add(src, value);
        }
        public static Dim3 operator +(uint src, Dim3 value)
        {
            return Add(src, value);
        }
        public static Dim3 operator -(Dim3 src, Dim3 value)
        {
            return Subtract(src, value);
        }
        public static Dim3 operator -(Dim3 src, uint value)
        {
            return Subtract(src, value);
        }
        public static Dim3 operator -(uint src, Dim3 value)
        {
            return Subtract(src, value);
        }
        public static Dim3 operator *(Dim3 src, Dim3 value)
        {
            return Multiply(src, value);
        }
        public static Dim3 operator *(Dim3 src, uint value)
        {
            return Multiply(src, value);
        }
        public static Dim3 operator *(uint src, Dim3 value)
        {
            return Multiply(src, value);
        }
        public static Dim3 operator /(Dim3 src, Dim3 value)
        {
            return Divide(src, value);
        }
        public static Dim3 operator /(Dim3 src, uint value)
        {
            return Divide(src, value);
        }
        public static Dim3 operator /(uint src, Dim3 value)
        {
            return Divide(src, value);
        }
        public static bool operator ==(Dim3 src, Dim3 value)
        {
            return src.Equals(value);
        }
        public static bool operator !=(Dim3 src, Dim3 value)
        {
            return !(src == value);
        }
        public static implicit operator Dim3(int value)
        {
            return new Dim3(value);
        }
        public static implicit operator Dim3(uint value)
        {
            return new Dim3(value);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Dim3 value) 
                return false;

            var ret = true;
            ret &= this.x == value.x;
            ret &= this.y == value.y;
            ret &= this.z == value.z;
            return ret;
        }
        public bool Equals(Dim3 value)
        {
            var ret = true;
            ret &= this.x == value.x;
            ret &= this.y == value.y;
            ret &= this.z == value.z;
            return ret;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}; {1}; {2})", this.x, this.y, this.z);
        }
        public Dim3(uint xValue, uint yValue, uint zValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = zValue;
        }
        public Dim3(uint xValue, uint yValue)
        {
            this.x = xValue;
            this.y = yValue;
            this.z = 1;
        }
        public Dim3(uint val)
        {
            this.x = val;
            this.y = 1;
            this.z = 1;
        }
        public Dim3(int xValue, int yValue, int zValue)
        {
            this.x = (uint)xValue;
            this.y = (uint)yValue;
            this.z = (uint)zValue;
        }
        public Dim3(int xValue, int yValue)
        {
            this.x = (uint)xValue;
            this.y = (uint)yValue;
            this.z = 1;
        }
        public Dim3(int val)
        {
            this.x = (uint)val;
            this.y = 1;
            this.z = 1;
        }
        public static Dim3 Min(Dim3 aValue, Dim3 bValue)
        {
            return new Dim3(System.Math.Min(aValue.x, bValue.x), System.Math.Min(aValue.y, bValue.y), System.Math.Min(aValue.z, bValue.z));
        }
        public static Dim3 Max(Dim3 aValue, Dim3 bValue)
        {
            return new Dim3(System.Math.Max(aValue.x, bValue.x), System.Math.Max(aValue.y, bValue.y), System.Math.Max(aValue.z, bValue.z));
        }
        public uint Size => (uint)Marshal.SizeOf(this);
    }
}
