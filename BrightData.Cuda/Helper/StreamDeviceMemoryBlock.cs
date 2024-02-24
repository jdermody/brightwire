using System;
using System.Runtime.InteropServices;
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;

namespace BrightData.Cuda.Helper
{
    internal unsafe class StreamDeviceMemoryBlock(CuStream stream, uint size, bool isStreamOwner) : DeviceMemoryBlockBase(Create(stream, size))
    {
        static CudaDeviceVariable<float> Create(CuStream stream, uint size)
        {
            var ptr = new CuDevicePtr();
            var sizeInBytes = size * sizeof(float);
            DriverApiNativeMethods.MemoryManagement.cuMemAllocAsync(ref ptr, sizeInBytes, stream).CheckResult();
            return new CudaDeviceVariable<float>(ptr, false);
        }

        protected override void OnDispose()
        {
            DriverApiNativeMethods.MemoryManagement.cuMemFreeAsync(DevicePointer, stream).CheckResult();
            if(isStreamOwner)
                DriverApiNativeMethods.Streams.cuStreamDestroy_v2(stream).CheckResult();
        }

        public override void CopyToDevice(float[] source)
        {
            fixed (float* ptr = &source[0]) {
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyHtoDAsync_v2(DevicePointer, (IntPtr)ptr, source.Length * sizeof(float), stream).CheckResult();
            }
        }

        public override void CopyToDevice(IDeviceMemoryPtr source)
        {
            DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoDAsync_v2(DevicePointer, source.DevicePointer, source.Size * sizeof(float), stream).CheckResult();
        }

        public override void CopyToDevice(ReadOnlySpan<float> span, uint targetOffset = 0)
        {
            fixed (float* ptr = span)
            {
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyHtoDAsync_v2(DevicePointer + targetOffset * sizeof(float), (IntPtr)ptr, Size * sizeof(float), stream).CheckResult();
            }
        }

        public override void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint size)
        {
            DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyHtoDAsync_v2(DevicePointer + targetOffset * sizeof(float), (IntPtr)(ptr + sourceOffset), size * sizeof(float), stream).CheckResult();
        }

        public override void CopyToHost(float[] target)
        {
            fixed (float* ptr = &target[0]) {
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoHAsync_v2((IntPtr)ptr, DevicePointer, target.Length * sizeof(float), stream);
            }
        }

        public override void CopyToHost(ArraySegment<float> target)
        {
            fixed (float* p = &target.Array![0]) {
                var ptr = p + target.Offset * sizeof(float);
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoHAsync_v2((IntPtr)ptr, DevicePointer, target.Count * sizeof(float), stream);
            }
        }

        public override void Clear()
        {
            DriverApiNativeMethods.MemsetAsync.cuMemsetD8Async(DevicePointer, 0, Size * sizeof(float), stream);
            //if (Size % 2 == 0)
            //    DriverAPINativeMethods.MemsetAsync.cuMemsetD32Async(DevicePointer, 0, Size / 2, _stream).CheckResult();
            //else
            //    DriverAPINativeMethods.MemsetAsync.cuMemsetD16Async(DevicePointer, 0, Size, _stream).CheckResult();
        }
    }
}
