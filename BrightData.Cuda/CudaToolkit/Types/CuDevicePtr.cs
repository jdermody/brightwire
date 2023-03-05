using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CuDevicePtr
    {
        public SizeT Pointer;
        public static implicit operator ulong(CuDevicePtr src)
        {
            return src.Pointer;
        }
        public static explicit operator CuDevicePtr(SizeT src)
        {
            var devicePtr = new CuDevicePtr {
                Pointer = src
            };
            return devicePtr;
        }
        public static CuDevicePtr operator +(CuDevicePtr src, SizeT value)
        {
            var devicePtr = new CuDevicePtr {
                Pointer = src.Pointer + value
            };
            return devicePtr;
        }
        public static CuDevicePtr operator -(CuDevicePtr src, SizeT value)
        {
            var devicePtr = new CuDevicePtr {
                Pointer = src.Pointer - value
            };
            return devicePtr;
        }
        public static bool operator ==(CuDevicePtr src, CuDevicePtr value)
        {
            return src.Pointer == value.Pointer;
        }
        public static bool operator !=(CuDevicePtr src, CuDevicePtr value)
        {
            return src.Pointer != value.Pointer;
        }

        public override bool Equals(object? obj) => obj is CuDevicePtr value && Pointer.Equals(value.Pointer);
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString()
        {
            return Pointer.ToString();
        }
        public CuDevicePtr(SizeT pointer)
        {
            Pointer = pointer;
        }
    }
}
