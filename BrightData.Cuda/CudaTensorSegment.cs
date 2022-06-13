using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightData.Cuda;
using BrightData2;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.Cuda
{
    internal class CudaTensorSegment : IDisposableTensorSegment
    {
        int _refCount = 0;

        public CudaTensorSegment(IDeviceMemoryPtr data)
        {
            DeviceMemory = data;
            IsValid = true;
        }

        public void Dispose()
        {
            Release();
        }

        public int AddRef() => DeviceMemory.AddRef();
        public int Release()
        {
            var ret = Interlocked.Decrement(ref _refCount);
            if (IsValid && ret <= 0) {
                DeviceMemory.Release();
                IsValid = false;
            }

            return ret;
        }

        public IDeviceMemoryPtr DeviceMemory { get; }
        public bool IsValid { get; private set; }
        public uint Size => DeviceMemory.Size;
        public string SegmentType => "cuda";

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

        public float[] ToNewArray()
        {
            var ret = new float[Size];
            DeviceMemory.CopyToHost(ret);
            return ret;
        }

        public void CopyFrom(Span<float> span)
        {
            DeviceMemory.CopyToDevice(span);
        }

        public void Clear()
        {
            DeviceMemory.Clear();
        }
    }
}
