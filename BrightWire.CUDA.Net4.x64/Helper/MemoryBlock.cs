using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrightWire.CUDA.Helper
{
    internal class MemoryBlock : IDisposable
    {
        internal class Ptr : ICountReferences
        {
            readonly int _index;
            readonly MemoryBlock _cache;
            CudaDeviceVariable<float> _data;
            bool _disposed = false;
            int _refCount = 1;

#if DEBUG
            public static int _badAlloc = -1;
            public static int _badDispose = -1;
            public static int _badAddRef = -1;
#endif

            public Ptr(MemoryBlock cache, int index, int size)
            {
                _cache = cache;
                _index = index;
                _data = new CudaDeviceVariable<float>(size);
#if DEBUG
                if (index == _badAlloc)
                    Debugger.Break();
#endif
            }
#if DEBUG
            ~Ptr()
            {
                if (!_disposed)
                    Debug.WriteLine("\tMemory Block {0} was not disposed !!", _index);
            }
#endif
            public CudaDeviceVariable<float> DeviceVariable => _data;
            public CUdeviceptr DevicePointer => _data.DevicePointer;
            public int Size => _data.Size;
            public int Index => _index;
            public void Destroy()
            {
#if DEBUG
                if (_index == _badDispose)
                    Debugger.Break();
#endif
                if (!_disposed) {
                    _data.Dispose();
                    _disposed = true;
                }
#if DEBUG
                GC.SuppressFinalize(this);
#endif
            }
            public int AddRef()
            {
#if DEBUG
                if (_index == _badAddRef)
                    Debugger.Break();
#endif
                return Interlocked.Increment(ref _refCount);
            }
            public int Release()
            {
#if DEBUG
                if (_index == _badAddRef)
                    Debugger.Break();
#endif
                if (Interlocked.Decrement(ref _refCount) == 0)
                    _cache.OnFree(this);
                return _refCount;
            }
            public void CopyToDevice(float[] source)
            {
                _data.CopyToDevice(source);
            }
            public void CopyToDevice(Ptr source)
            {
                _data.CopyToDevice(source.DeviceVariable);
            }
            public void CopyToHost(float[] target)
            {
                _data.CopyToHost(target);
            }
            public Ptr Clone()
            {
                var block = _cache.GetMemory(_data.Size);
                block.DeviceVariable.CopyToDevice(_data);
                return block;
            }
            public void Clear()
            {
                _data.Memset(0);
            }
        }
        readonly int _maxSize;
        readonly ConcurrentStack<List<Ptr>> _layer = new ConcurrentStack<List<Ptr>>();
        ConcurrentDictionary<int, List<Ptr>> _cache = new ConcurrentDictionary<int, List<Ptr>>();
        int _index = 0;

        public MemoryBlock(int maxSize = 1048576)
        {
            _maxSize = maxSize;
            PushLayer();
        }
        ~MemoryBlock()
        {
            _Dispose();
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _Dispose();
        }
        void _Dispose()
        {
            List<Ptr> layer;
            while (_layer.TryPop(out layer)) {
                lock (layer) {
                    foreach (var item in layer)
                        item.Destroy();
                }
            }
            _cache.Clear();
        }

        public void PushLayer()
        {
            _layer.Push(new List<Ptr>());
        }

        public void PopLayer()
        {
            List<Ptr> layer;
            if (_layer.TryPop(out layer)) {
                lock (layer) {
                    foreach (var item in layer)
                        item.Release();
                }
            }
        }

        void OnFree(Ptr item)
        {
            // add the new item
            var temp = _cache.GetOrAdd(item.Size, kv => new List<Ptr>());
            lock (temp) {
                temp.Add(item);
            }

            // check if we need to delete old items
            while (_cache.Sum(kv => kv.Key * kv.Value.Count) > _maxSize) {
                var oldItem = _cache.SelectMany(kv => kv.Value).OrderBy(d => d.Index).First();
                _RemoveFromCache(oldItem);
            }
        }

        void _RemoveFromCache(Ptr item)
        {
            item.Destroy();
            List<Ptr> temp;

            if(_cache.TryGetValue(item.Size, out temp)) {
                lock (temp) {
                    var wasRemoved = temp.Remove(item);
                    Debug.Assert(wasRemoved);
                }
            }
        }

        public Ptr GetMemory(int size)
        {
            Ptr ret;
            List<Ptr> temp;
            if (_cache.TryGetValue(size, out temp)) {
                lock (temp) {
                    ret = temp.FirstOrDefault();
                    if (ret != null) {
                        temp.RemoveAt(0);
                        ret.AddRef();
                        return ret;
                    }
                }
            }

            ret = new Ptr(this, _GetNextIndex(), size);
            if (_layer.TryPeek(out temp)) {
                lock (temp) {
                    temp.Add(ret);
                }
            }

            return ret;
        }

        int _GetNextIndex()
        {
            return Interlocked.Increment(ref _index);
        }
    }
}
