using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Cuda
{
    internal class CudaTensorSegment : ITensorSegment
    {
        static readonly string CudaSegmentType = "cuda";

        public CudaTensorSegment(IDeviceMemoryPtr data)
        {
            DeviceMemory = data;
        }

        public void Dispose()
        {
            Release();
        }

        public static bool IsCuda(ITensorSegment segment, [NotNullWhen(true)]out CudaTensorSegment? cudaSegment)
        {
            if (segment.SegmentType == CudaSegmentType) {
                cudaSegment = (CudaTensorSegment)segment;
                return true;
            }
            cudaSegment = null;
            return false;
        }

        public int AddRef() => DeviceMemory.AddRef();
        public int Release() => DeviceMemory.Release();

        public IDeviceMemoryPtr DeviceMemory { get; }
        public bool IsValid => DeviceMemory.IsValid;
        public uint Size => DeviceMemory.Size;
        public string SegmentType => CudaSegmentType;
        public bool IsWrapper => false;

        public float this[int index]
        {
            get => DeviceMemory.DeviceVariable[index];
            set => DeviceMemory.DeviceVariable[index] = value;
        }

        public float this[uint index]
        {
            get => DeviceMemory.DeviceVariable[index];
            set => DeviceMemory.DeviceVariable[index] = value;
        }

        public float this[long index]
        {
            get => DeviceMemory.DeviceVariable[index];
            set => DeviceMemory.DeviceVariable[index] = value;
        }

        public float this[ulong index]
        {
            get => DeviceMemory.DeviceVariable[index];
            set => DeviceMemory.DeviceVariable[index] = value;
        }

        public IEnumerable<float> Values => ToNewArray();
        public float[]? GetArrayForLocalUseOnly() => null;

        public MemoryOwner<float> ToNewMemoryOwner()
        {
            var ret = MemoryOwner<float>.Allocate((int)Size);
            DeviceMemory.CopyToHost(ret.DangerousGetArray());
            return ret;
        }

        public float[] ToNewArray()
        {
            var ret = new float[Size];
            DeviceMemory.CopyToHost(ret);
            return ret;
        }

        public void CopyFrom(ReadOnlySpan<float> span)
        {
            DeviceMemory.CopyToDevice(span);
        }

        public void CopyTo(ITensorSegment segment)
        {
            if (segment.SegmentType == CudaSegmentType) {
                var other = (CudaTensorSegment)segment;
                other.DeviceMemory.CopyToDevice(DeviceMemory);
            }
            else {
                using var buffer = ToNewMemoryOwner();
                segment.CopyFrom(buffer.Span);
            }
        }

        public void CopyTo(Span<float> destination)
        {
            using var buffer = ToNewMemoryOwner();
            buffer.Span.CopyTo(destination);
        }
        public unsafe void CopyTo(float* destination, int offset, int stride, int count)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            DeviceMemory.Clear();
        }

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed)
        {
            wasTempUsed = true;
            temp = SpanOwner<float>.Allocate((int)Size);
            DeviceMemory.CopyToHost(temp.DangerousGetArray());
            return temp.Span;
        }

        public ReadOnlySpan<float> GetSpan()
        {
            throw new NotImplementedException();
        }

        public (float[] Array, uint Offset, uint Stride) GetUnderlyingArray()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{SegmentType} ({IsValid}): {Size:N0} floats";
        }
    }
}
