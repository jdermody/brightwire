using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Memory
{
    class DisposableLayers : IDisposableLayers
    {
        readonly ConcurrentStack<ConcurrentBag<IDisposable>> _layers = new ConcurrentStack<ConcurrentBag<IDisposable>>();

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
