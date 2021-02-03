using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace BrightData.Cuda.Helper
{
	/// <summary>
	/// Maintains a cache of available device memory
	/// </summary>
    internal class DeviceMemory : IDisposable
    {
        class Block : IDeviceMemoryPtr
        {
            readonly int _index;
            readonly DeviceMemory _cache;
            readonly CudaDeviceVariable<float> _data;
            bool _disposed = false;
	        int _refCount = 1;

#if DEBUG
            static readonly int _badAlloc = -1;
            static readonly int _badDispose = -1;
            public bool IsValid => !_disposed;
#else
            public bool IsValid => true;
#endif

            public Block(DeviceMemory cache, int index, uint size) 
            {
                _cache = cache;
                _index = index;

	            var sizeInBytes = size * CudaProvider.FloatSize;
	            var ptr = new CUdeviceptr();
	            var result = DriverAPINativeMethods.MemoryManagement.cuMemAlloc_v2(ref ptr, sizeInBytes);
	            CudaProvider.CheckForError(result);
                _data = new CudaDeviceVariable<float>(ptr, true, sizeInBytes);

#if DEBUG
                if (_index == _badAlloc)
                    Debugger.Break();
#endif
            }
#if DEBUG
            ~Block()
            {
                if (!_disposed)
                    Debug.WriteLine($"\tMemory Block {_index} was not disposed - {_data.SizeInBytes} bytes leaked in the GPU !!");
            }
#endif
            public override string ToString()
            {
                var valid = IsValid ? "" : " (invalid)";
                return $"{_index}, {_data.SizeInBytes} bytes {valid}";
            }

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
		        return Interlocked.Increment(ref _refCount);
	        }
            public void Free()
            {
                if(Interlocked.Decrement(ref _refCount) <= 0 && !_disposed)
                    _cache.OnFree(this);
            }

            public CudaDeviceVariable<float> DeviceVariable => _data;
	        public CUdeviceptr DevicePointer => _data.DevicePointer;
	        public uint Size => _data.Size;

	        public void CopyToDevice(float[] source)
            {
                _data.CopyToDevice(source);
            }
            public void CopyToDevice(IDeviceMemoryPtr source)
            {
                _data.CopyToDevice(source.DeviceVariable);
            }
            public void CopyToHost(float[] target)
            {
                _data.CopyToHost(target);
            }
            public void Clear()
            {
                _data.Memset(0);
            }
        }
        class Layer
        {
            readonly List<IDisposable> _disposable = new List<IDisposable>();
            readonly List<Block> _ptr = new List<Block>();

            public void Add(IDisposable disposable) => _disposable.Add(disposable);
            public void Add(Block ptr) => _ptr.Add(ptr);
            public void Release()
            {
                foreach (var item in _disposable)
                    item?.Dispose();
                foreach (var item in _ptr)
                    item.Free();
            }
        }
        readonly int _maxSize;
        readonly ConcurrentStack<Layer> _layer = new ConcurrentStack<Layer>();
        readonly ConcurrentDictionary<uint, ThreadSafeHashSet<Block>> _cache = new ConcurrentDictionary<uint, ThreadSafeHashSet<Block>>();
        int _index = 0;

        public DeviceMemory(int maxSize)
        {
            _maxSize = maxSize;
            PushLayer();
        }
        ~DeviceMemory()
        {
            DisposeMemory();
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeMemory();
        }
        void DisposeMemory()
        {
            while (_layer.TryPop(out Layer layer)) {
                lock (layer) {
                    layer.Release();
                }
            }
            foreach (var item in _cache) {
                item.Value.ForEach(d => d.Destroy());
                item.Value.Clear();
            }
            _cache.Clear();
        }

        public void PushLayer()
        {
            _layer.Push(new Layer());
        }

        public void PopLayer()
        {
            if (_layer.TryPop(out Layer layer)) {
                lock (layer) {
                    layer.Release();
                }
            }
        }

        void OnFree(Block item)
        {
            if (_maxSize == 0)
                item.Destroy();
            else {
                // add the new item
                var temp = _cache.GetOrAdd(item.Size, kv => new ThreadSafeHashSet<Block>());
                temp.Add(item);

                // check if we need to delete old items
                while (_cache.Sum(kv => kv.Key * kv.Value.Count) > _maxSize) {
                    Block oldestItem = null;
                    foreach(var block in _cache) {
                        block.Value.ForEach(b => {
                            if (oldestItem == null || oldestItem.Index < b.Index)
                                oldestItem = b;
                        });
                    }

	                if (oldestItem != null)
	                {
		                _cache[oldestItem.Size].Remove(oldestItem);
		                oldestItem.Destroy();
	                }
                }
            }
        }

        public IDeviceMemoryPtr GetMemory(uint size)
        {
            Block ret;

            if (_maxSize > 0) {
                if (_cache.TryGetValue(size, out ThreadSafeHashSet<Block> temp)) {
                    if (temp.TryPop(out ret))
                        return ret;
                }
            }

            ret = new Block(this, GetNextIndex(), size);
            if (_layer.TryPeek(out Layer layer)) {
                lock (layer) {
                    layer.Add(ret);
                }
            }

            return ret;
        }

        public void Add(IDisposable disposable)
        {
            if (_layer.TryPeek(out Layer layer)) {
                lock (layer) {
                    layer.Add(disposable);
                }
            }
        }

        int GetNextIndex()
        {
            return Interlocked.Increment(ref _index);
        }
    }
}
