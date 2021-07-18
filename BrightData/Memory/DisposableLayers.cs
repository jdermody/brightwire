using System;
using System.Collections.Concurrent;

namespace BrightData.Memory
{
    /// <summary>
    /// Collects allocations within a "layer" that can all be released at once
    /// </summary>
    internal class DisposableLayers : IDisposableLayers
    {
        readonly ConcurrentStack<ConcurrentBag<IDisposable>> _layers = new();

        public void Add(IDisposable disposable)
        {
            if (_layers.TryPeek(out var layer))
                layer.Add(disposable);
        }

        public IDisposable Push()
        {
            _layers.Push(new ConcurrentBag<IDisposable>());
            return new MemoryLayer(this);
        }

        public void Pop()
        {
            if (_layers.TryPop(out var bag)) {
                foreach (var item in bag)
                    item.Dispose();
            }
        }
    }
}
