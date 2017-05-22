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
    internal class MemoryCache : IDisposable
    {
        internal class Item : IDisposable
        {
            readonly int _index;
            readonly MemoryCache _cache;
            CudaDeviceVariable<float> _data;
            int _refCount = 1;

#if DEBUG
            public static int _badAlloc = -1;
            public static int _badDispose = -1;
#endif

            public Item(MemoryCache cache, int index, int size)
            {
                _cache = cache;
                _index = index;
                _data = new CudaDeviceVariable<float>(size);
#if DEBUG
                if (index == _badAlloc)
                    Debugger.Break();
#endif
            }
            public CudaDeviceVariable<float> DeviceVariable => _data;
            public CUdeviceptr DevicePointer => _data.DevicePointer;
            public int Size => _data.Size;
            public int Index => _index;
            public void Dispose()
            {
#if DEBUG
                if (_index == _badDispose)
                    Debugger.Break();
#endif
                _data.Dispose();
            }
            public int AddRef()
            {
                return Interlocked.Increment(ref _refCount);
            }
            public int Release()
            {
                if (Interlocked.Decrement(ref _refCount) == 0)
                    _cache.OnFree(this);
                return _refCount;
            }
            public void CopyToDevice(float[] source)
            {
                _data.CopyToDevice(source);
            }
            public void CopyToDevice(Item source)
            {
                _data.CopyToDevice(source.DeviceVariable);
            }
            public void CopyToHost(float[] target)
            {
                _data.CopyToHost(target);
            }
            public Item Clone()
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
        readonly ConcurrentStack<List<Item>> _layer = new ConcurrentStack<List<Item>>();
        ConcurrentDictionary<int, List<Item>> _cache = new ConcurrentDictionary<int, List<Item>>();
        int _index = 0;

        public MemoryCache(int maxSize = 1048576)
        {
            _maxSize = maxSize;
            PushLayer();
        }
        ~MemoryCache()
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
            List<Item> layer;
            while (_layer.TryPop(out layer)) {
                lock (layer) {
                    foreach (var item in layer)
                        item.Dispose();
                }
            }
            _cache.Clear();
        }

        public void PushLayer()
        {
            _layer.Push(new List<Item>());
        }

        public void PopLayer()
        {
            List<Item> layer;
            if (_layer.TryPop(out layer)) {
                lock (layer) {
                    foreach (var item in layer)
                        item.Release();
                }
            }
        }

        void OnFree(Item item)
        {
            // add the new item
            var temp = _cache.GetOrAdd(item.Size, kv => new List<Item>());
            lock (temp) {
                temp.Add(item);
            }

            // check if we need to delete old items
            while (_cache.Sum(kv => kv.Key * kv.Value.Count) > _maxSize) {
                var oldItem = _cache.SelectMany(kv => kv.Value).OrderBy(d => d.Index).First();
                _RemoveFromCache(oldItem);
            }
        }

        void _RemoveFromCache(Item item)
        {
            item.Dispose();
            List<Item> temp;

            if(_cache.TryGetValue(item.Size, out temp)) {
                lock (temp) {
                    var wasRemoved = temp.Remove(item);
                    Debug.Assert(wasRemoved);
                }
            }
        }

        public Item GetMemory(int size)
        {
            Item ret;
            List<Item> temp;
            if (_cache.TryGetValue(size, out temp)) {
                lock (temp) {
                    ret = temp.FirstOrDefault();
                    if (ret != null) {
                        temp.RemoveAt(0);
                        return ret;
                    }
                }
            }

            ret = new Item(this, _GetNextIndex(), size);
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
