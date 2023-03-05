using System.Runtime.InteropServices;

namespace BrightData.Cuda.CudaToolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CuDevice
    {
        public int Pointer;

        public CuUuid Uuid
        {
            get
            {
                var uuid = new CuUuid();
                var res = DriverApiNativeMethods.DeviceManagement.cuDeviceGetUuid_v2(ref uuid, this);
                
                if (res != CuResult.Success) throw new CudaException(res);
                return uuid;
            }
        }
        public static bool operator ==(CuDevice src, CuDevice value)
        {
            return src.Pointer == value.Pointer;
        }
        public static bool operator !=(CuDevice src, CuDevice value)
        {
            return src.Pointer != value.Pointer;
        }
        public override bool Equals(object? obj)
        {
            return obj is CuDevice device && this.Pointer.Equals(device.Pointer);
        }
        public override int GetHashCode()
        {
            return Pointer.GetHashCode();
        }
        public override string ToString()
        {
            return Pointer.ToString();
        }
    }
}
