using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Cuda.CudaToolkit;
using BrightData.Cuda.CudaToolkit.Types;
using BrightData.Helper;

namespace BrightData.Cuda.Helper
{
    internal unsafe class StreamDeviceMemoryBlock : DeviceMemoryBlockBase
    {
        readonly CuStream _stream;
        readonly bool _isStreamOwner;

        public StreamDeviceMemoryBlock(CuStream stream, uint size, bool isStreamOwner) : base(Create(stream, size))
        {
            _stream = stream;
            _isStreamOwner = isStreamOwner;
        }

        static CudaDeviceVariable<float> Create(CuStream stream, uint size)
        {
            var ptr = new CuDevicePtr();
            var sizeInBytes = size * sizeof(float);
            DriverApiNativeMethods.MemoryManagement.cuMemAllocAsync(ref ptr, sizeInBytes, stream).CheckResult();
            return new CudaDeviceVariable<float>(ptr, false);
        }

        protected override void OnDispose()
        {
            DriverApiNativeMethods.MemoryManagement.cuMemFreeAsync(DevicePointer, _stream).CheckResult();
            if(_isStreamOwner)
                DriverApiNativeMethods.Streams.cuStreamDestroy_v2(_stream).CheckResult();
        }

        public override void CopyToDevice(float[] source)
        {
            fixed (float* ptr = &source[0]) {
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyHtoDAsync_v2(DevicePointer, (IntPtr)ptr, source.Length * sizeof(float), _stream).CheckResult();
            }
        }

        public override void CopyToDevice(IDeviceMemoryPtr source)
        {
            DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoDAsync_v2(DevicePointer, source.DevicePointer, source.Size * sizeof(float), _stream).CheckResult();
        }

        public override void CopyToDevice(ReadOnlySpan<float> span, uint targetOffset = 0)
        {
            fixed (float* ptr = &MemoryMarshal.GetReference(span))
            {
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyHtoDAsync_v2(DevicePointer + targetOffset * sizeof(float), (IntPtr)ptr, Size * sizeof(float), _stream).CheckResult();
            }
        }

        public override void CopyToDevice(float* ptr, uint sourceOffset, uint targetOffset, uint size)
        {
            DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyHtoDAsync_v2(DevicePointer + targetOffset * sizeof(float), (IntPtr)(ptr + sourceOffset), size * sizeof(float), _stream).CheckResult();
        }

        public override void CopyToHost(float[] target)
        {
            fixed (float* ptr = &target[0]) {
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoHAsync_v2((IntPtr)ptr, DevicePointer, target.Length * sizeof(float), _stream);
            }
        }

        public override void CopyToHost(ArraySegment<float> target)
        {
            fixed (float* p = &target.Array![0]) {
                var ptr = p + target.Offset * sizeof(float);
                DriverApiNativeMethods.AsynchronousMemcpyV2.cuMemcpyDtoHAsync_v2((IntPtr)ptr, DevicePointer, target.Count * sizeof(float), _stream);
            }
        }

        public override void Clear()
        {
            DriverApiNativeMethods.MemsetAsync.cuMemsetD8Async(DevicePointer, 0, Size * sizeof(float), _stream);
            //if (Size % 2 == 0)
            //    DriverAPINativeMethods.MemsetAsync.cuMemsetD32Async(DevicePointer, 0, Size / 2, _stream).CheckResult();
            //else
            //    DriverAPINativeMethods.MemsetAsync.cuMemsetD16Async(DevicePointer, 0, Size, _stream).CheckResult();
        }
    }
}
