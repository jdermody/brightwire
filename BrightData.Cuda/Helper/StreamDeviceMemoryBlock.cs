using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Helper;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
    internal unsafe class StreamDeviceMemoryBlock : DeviceMemoryBlockBase
    {
        readonly CUstream _stream;

        public StreamDeviceMemoryBlock(CUstream stream, uint size)
        {
            _stream = stream;
            var ptr = new CUdeviceptr();
            var sizeInBytes = size * sizeof(float);
            DriverAPINativeMethods.MemoryManagement.cuMemAllocAsync(ref ptr, sizeInBytes, stream).CheckResult();
            _data = new CudaDeviceVariable<float>(ptr, false);
        }

        protected override void OnDispose()
        {
            DriverAPINativeMethods.MemoryManagement.cuMemFreeAsync(DevicePointer, _stream).CheckResult();
        }

        public override void CopyToDevice(float[] source)
        {
            fixed (float* ptr = &source[0]) {
                DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyHtoDAsync_v2(DevicePointer, (IntPtr)ptr, source.Length * sizeof(float), _stream).CheckResult();
            }
        }

        public override void CopyToDevice(IDeviceMemoryPtr source)
        {
            DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyDtoDAsync_v2(DevicePointer, source.DevicePointer, source.Size * sizeof(float), _stream).CheckResult();
        }

        public override void CopyToDevice(ReadOnlySpan<float> span, uint offsetSource = 0)
        {
            fixed (float* p = &MemoryMarshal.GetReference(span))
            {
                var ptr = p + offsetSource * sizeof(float);
                DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyHtoDAsync_v2(DevicePointer, (IntPtr)ptr, Size * sizeof(float), _stream).CheckResult();
            }
        }

        public override void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint size)
        {
            DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyHtoDAsync_v2(DevicePointer + targetOffset * sizeof(float), (IntPtr)(ptr + sourceOffset), size * sizeof(float), _stream).CheckResult();
        }

        public override void CopyToHost(float[] target)
        {
            fixed (float* ptr = &target[0]) {
                DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyDtoHAsync_v2((IntPtr)ptr, DevicePointer, target.Length * sizeof(float), _stream);
            }
        }

        public override void CopyToHost(ArraySegment<float> target)
        {
            fixed (float* p = &target.Array![0]) {
                var ptr = p + target.Offset * sizeof(float);
                DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyDtoHAsync_v2((IntPtr)ptr, DevicePointer, target.Count * sizeof(float), _stream);
            }
        }
    }
}
