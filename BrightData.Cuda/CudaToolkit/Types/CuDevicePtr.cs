﻿using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct CuDevicePtr(SizeT pointer)
    {
        public SizeT Pointer = pointer;
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

        public readonly override bool Equals(object? obj) => obj is CuDevicePtr value && Pointer.Equals(value.Pointer);
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public readonly override int GetHashCode() => base.GetHashCode();
        public readonly override string ToString() => Pointer.ToString();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
