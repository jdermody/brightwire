using BrightWire.LinearAlgebra;
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
    internal class DeviceMemory : IDisposable
    {
        internal class Ptr
        {
            protected CudaDeviceVariable<float> _data;

            public Ptr(CudaDeviceVariable<float> data)
            {
                _data = data;
            }
            public virtual void Free()
            {
                // nop
            }
            public CudaDeviceVariable<float> DeviceVariable
            {
                get
                {
                    return _data;
                }
            }
            public CUdeviceptr DevicePointer
            {
                get
                {
                    return _data.DevicePointer;
                }
            }
            public int Size
            {
                get
                {
                    return _data.Size;
                }
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
            //public Ptr Clone()
            //{
            //    Debug.Assert(IsValid);
            //    var block = _cache.GetMemory(_data.Size);
            //    block.DeviceVariable.CopyToDevice(_data);
            //    return block;
            //}
            public void Clear()
            {
                _data.Memset(0);
            }
        }
        internal class Block : Ptr
        {
            readonly int _index;
            readonly DeviceMemory _cache;
            bool _disposed = false;

#if DEBUG
            public static int _badAlloc = -1;
            public static int _badDispose = -1;
            public bool IsValid => !_disposed;
#else
            public bool IsValid => true;
#endif

            public Block(DeviceMemory cache, int index, int size) 
                : base(new CudaDeviceVariable<float>(size))
            {
                _cache = cache;
                _index = index;

#if DEBUG
                if (_index == _badAlloc)
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
            public override void Free()
            {
                if(!_disposed)
                    _cache.OnFree(this);
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
                    item.Dispose();
                foreach (var item in _ptr)
                    item.Free();
            }
        }
        readonly int _maxSize;
        readonly ConcurrentStack<Layer> _layer = new ConcurrentStack<Layer>();
        readonly ConcurrentDictionary<int, ThreadSafeHashSet<Block>> _cache = new ConcurrentDictionary<int, ThreadSafeHashSet<Block>>();
        int _index = 0;

        public DeviceMemory(int maxSize)
        {
            _maxSize = maxSize;
            PushLayer();
        }
        ~DeviceMemory()
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
            Layer layer;
            while (_layer.TryPop(out layer)) {
                lock (layer) {
                    layer.Release();
                }
            }
            foreach(var item in _cache) {
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
            Layer layer;
            if (_layer.TryPop(out layer)) {
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
                    _cache[oldestItem.Size].Remove(oldestItem);
                    oldestItem.Destroy();
                }
            }
        }

        public Ptr GetMemory(int size)
        {
            Block ret;

            if (_maxSize > 0) {
                ThreadSafeHashSet<Block> temp;
                if (_cache.TryGetValue(size, out temp)) {
                    if(temp.TryPop(out ret))
                        return ret;
                }
            }

            ret = new Block(this, _GetNextIndex(), size);
            Layer layer;
            if (_layer.TryPeek(out layer)) {
                lock (layer) {
                    layer.Add(ret);
                }
            }

            return ret;
        }

        public void Add(IDisposable disposable)
        {
            Layer layer;
            if (_layer.TryPeek(out layer)) {
                lock (layer) {
                    layer.Add(disposable);
                }
            }
        }

        int _GetNextIndex()
        {
            return Interlocked.Increment(ref _index);
        }
    }
}
