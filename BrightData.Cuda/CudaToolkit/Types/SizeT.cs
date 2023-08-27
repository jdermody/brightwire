using System;
using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [StructLayout(LayoutKind.Sequential)]

    public readonly struct SizeT
    {
        readonly nuint _value;

        public SizeT(int value)   => this._value = new nuint((uint)value);
        public SizeT(uint value)  => this._value = new nuint(value);
        public SizeT(long value)  => this._value = new nuint((ulong)value);
        public SizeT(ulong value) => this._value = new nuint(value);
        public SizeT(nuint value) => this._value = value;
        public SizeT(nint value)  => this._value = new nuint((ulong)value.ToInt64());

        public static implicit operator int(SizeT t)       => (int)t._value.ToUInt32();
        public static implicit operator uint(SizeT t)      => (t._value.ToUInt32());
        public static implicit operator long(SizeT t)      => (long)t._value.ToUInt64();
        public static implicit operator ulong(SizeT t)     => (t._value.ToUInt64());
        public static implicit operator nuint(SizeT t)     => t._value;
        public static implicit operator nint(SizeT t)      => new((long)t._value.ToUInt64());
        public static implicit operator SizeT(int value)   => new(value);
        public static implicit operator SizeT(uint value)  => new(value);
        public static implicit operator SizeT(long value)  => new(value);
        public static implicit operator SizeT(ulong value) => new(value);
        public static implicit operator SizeT(nint value)  => new(value);
        public static implicit operator SizeT(nuint value) => new(value);

        public static bool operator !=(SizeT val1, SizeT val2) => (val1._value != val2._value);
        public static bool operator ==(SizeT val1, SizeT val2) => (val1._value == val2._value);
        public static SizeT operator +(SizeT val1, SizeT val2) => new(val1._value.ToUInt64() + val2._value.ToUInt64());
        public static SizeT operator +(SizeT val1, int val2)   => new(val1._value.ToUInt64() + (ulong)val2);
        public static SizeT operator +(int val1, SizeT val2)   => new((ulong)val1 + val2._value.ToUInt64());
        public static SizeT operator +(uint val1, SizeT val2)  => new(val1 + val2._value.ToUInt64());
        public static SizeT operator +(SizeT val1, uint val2)  => new(val1._value.ToUInt64() + val2);
        public static SizeT operator -(SizeT val1, SizeT val2) => new(val1._value.ToUInt64() - val2._value.ToUInt64());
        public static SizeT operator -(SizeT val1, int val2)   => new(val1._value.ToUInt64() - (ulong)val2);
        public static SizeT operator -(int val1, SizeT val2)   => new((ulong)val1 - val2._value.ToUInt64());
        public static SizeT operator -(SizeT val1, uint val2)  => new(val1._value.ToUInt64() - val2);
        public static SizeT operator -(uint val1, SizeT val2)  => new(val1 - val2._value.ToUInt64());
        public static SizeT operator *(SizeT val1, SizeT val2) => new(val1._value.ToUInt64() * val2._value.ToUInt64());
        public static SizeT operator *(SizeT val1, int val2)   => new(val1._value.ToUInt64() * (ulong)val2);
        public static SizeT operator *(int val1, SizeT val2)   => new((ulong)val1 * val2._value.ToUInt64());
        public static SizeT operator *(SizeT val1, uint val2)  => new(val1._value.ToUInt64() * val2);
        public static SizeT operator *(uint val1, SizeT val2)  => new(val1 * val2._value.ToUInt64());
        public static SizeT operator /(SizeT val1, SizeT val2) => new(val1._value.ToUInt64() / val2._value.ToUInt64());
        public static SizeT operator /(SizeT val1, int val2)   => new(val1._value.ToUInt64() / (ulong)val2);
        public static SizeT operator /(int val1, SizeT val2)   => new((ulong)val1 / val2._value.ToUInt64());
        public static SizeT operator /(SizeT val1, uint val2)  => new(val1._value.ToUInt64() / val2);
        public static SizeT operator /(uint val1, SizeT val2)  => new(val1 / val2._value.ToUInt64());
        public static bool operator >(SizeT val1, SizeT val2)  => val1._value.ToUInt64() > val2._value.ToUInt64();
        public static bool operator >(SizeT val1, int val2)    => val1._value.ToUInt64() > (ulong)val2;
        public static bool operator >(int val1, SizeT val2)    => (ulong)val1 > val2._value.ToUInt64();
        public static bool operator >(SizeT val1, uint val2)   => val1._value.ToUInt64() > val2;
        public static bool operator >(uint val1, SizeT val2)   => val1 > val2._value.ToUInt64();
        public static bool operator <(SizeT val1, SizeT val2)  => val1._value.ToUInt64() < val2._value.ToUInt64();
        public static bool operator <(SizeT val1, int val2)    => val1._value.ToUInt64() < (ulong)val2;
        public static bool operator <(int val1, SizeT val2)    => (ulong)val1 < val2._value.ToUInt64();
        public static bool operator <(SizeT val1, uint val2)   => val1._value.ToUInt64() < val2;
        public static bool operator <(uint val1, SizeT val2)   => val1 < val2._value.ToUInt64();

        public override bool Equals(object? obj)               => obj is SizeT sizeT && _value.Equals(sizeT._value);
        public override string ToString() => IntPtr.Size == 4 ? this._value.ToUInt32().ToString() : this._value.ToUInt64().ToString();
        public override int GetHashCode() => this._value.GetHashCode();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}