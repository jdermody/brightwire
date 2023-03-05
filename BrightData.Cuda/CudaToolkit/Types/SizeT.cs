using System;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SizeT
    {
        readonly nuint value;
        public SizeT(int value)
        {
            this.value = new nuint((uint)value);
        }
        public SizeT(uint value)
        {
            this.value = new nuint(value);
        }
        public SizeT(long value)
        {
            this.value = new nuint((ulong)value);
        }
        public SizeT(ulong value)
        {
            this.value = new nuint(value);
        }
        public SizeT(nuint value)
        {
            this.value = value;
        }
        public SizeT(nint value)
        {
            this.value = new nuint((ulong)value.ToInt64());
        }
        public static implicit operator int(SizeT t)
        {
            return (int)t.value.ToUInt32();
        }
        public static implicit operator uint(SizeT t)
        {
            return (t.value.ToUInt32());
        }
        public static implicit operator long(SizeT t)
        {
            return (long)t.value.ToUInt64();
        }
        public static implicit operator ulong(SizeT t)
        {
            return (t.value.ToUInt64());
        }
        public static implicit operator nuint(SizeT t)
        {
            return t.value;
        }
        public static implicit operator nint(SizeT t)
        {
            return new nint((long)t.value.ToUInt64());
        }
        public static implicit operator SizeT(int value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(uint value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(long value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(ulong value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(nint value)
        {
            return new SizeT(value);
        }
        public static implicit operator SizeT(nuint value)
        {
            return new SizeT(value);
        }
        public static bool operator !=(SizeT val1, SizeT val2)
        {
            return (val1.value != val2.value);
        }
        public static bool operator ==(SizeT val1, SizeT val2)
        {
            return (val1.value == val2.value);
        }
        public static SizeT operator +(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() + val2.value.ToUInt64());
        }
        public static SizeT operator +(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() + (ulong)val2);
        }
        public static SizeT operator +(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 + val2.value.ToUInt64());
        }
        public static SizeT operator +(uint val1, SizeT val2)
        {
            return new SizeT(val1 + val2.value.ToUInt64());
        }
        public static SizeT operator +(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() + val2);
        }
        public static SizeT operator -(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() - val2.value.ToUInt64());
        }
        public static SizeT operator -(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() - (ulong)val2);
        }
        public static SizeT operator -(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 - val2.value.ToUInt64());
        }
        public static SizeT operator -(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() - val2);
        }
        public static SizeT operator -(uint val1, SizeT val2)
        {
            return new SizeT(val1 - val2.value.ToUInt64());
        }
        public static SizeT operator *(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() * val2.value.ToUInt64());
        }
        public static SizeT operator *(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() * (ulong)val2);
        }
        public static SizeT operator *(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 * val2.value.ToUInt64());
        }
        public static SizeT operator *(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() * val2);
        }
        public static SizeT operator *(uint val1, SizeT val2)
        {
            return new SizeT(val1 * val2.value.ToUInt64());
        }
        public static SizeT operator /(SizeT val1, SizeT val2)
        {
            return new SizeT(val1.value.ToUInt64() / val2.value.ToUInt64());
        }
        public static SizeT operator /(SizeT val1, int val2)
        {
            return new SizeT(val1.value.ToUInt64() / (ulong)val2);
        }
        public static SizeT operator /(int val1, SizeT val2)
        {
            return new SizeT((ulong)val1 / val2.value.ToUInt64());
        }
        public static SizeT operator /(SizeT val1, uint val2)
        {
            return new SizeT(val1.value.ToUInt64() / val2);
        }
        public static SizeT operator /(uint val1, SizeT val2)
        {
            return new SizeT(val1 / val2.value.ToUInt64());
        }
        public static bool operator >(SizeT val1, SizeT val2)
        {
            return val1.value.ToUInt64() > val2.value.ToUInt64();
        }
        public static bool operator >(SizeT val1, int val2)
        {
            return val1.value.ToUInt64() > (ulong)val2;
        }
        public static bool operator >(int val1, SizeT val2)
        {
            return (ulong)val1 > val2.value.ToUInt64();
        }
        public static bool operator >(SizeT val1, uint val2)
        {
            return val1.value.ToUInt64() > val2;
        }
        public static bool operator >(uint val1, SizeT val2)
        {
            return val1 > val2.value.ToUInt64();
        }
        public static bool operator <(SizeT val1, SizeT val2)
        {
            return val1.value.ToUInt64() < val2.value.ToUInt64();
        }
        public static bool operator <(SizeT val1, int val2)
        {
            return val1.value.ToUInt64() < (ulong)val2;
        }
        public static bool operator <(int val1, SizeT val2)
        {
            return (ulong)val1 < val2.value.ToUInt64();
        }
        public static bool operator <(SizeT val1, uint val2)
        {
            return val1.value.ToUInt64() < val2;
        }
        public static bool operator <(uint val1, SizeT val2)
        {
            return val1 < val2.value.ToUInt64();
        }
        public override bool Equals(object? obj) => obj is SizeT sizeT && value.Equals(sizeT.value);
        public override string ToString()
        {
            if (IntPtr.Size == 4)
                return this.value.ToUInt32().ToString();
            else
                return this.value.ToUInt64().ToString();
        }
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
    }
}
