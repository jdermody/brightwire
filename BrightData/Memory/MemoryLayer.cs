using System;
using System.Collections.Generic;
using System.Text;

namespace BrightData.Memory
{
    class MemoryLayer : IDisposable
    {
        readonly IDisposableLayers _layers;

        public MemoryLayer(IDisposableLayers layers)
        {
            _layers = layers;
        }

        public void Dispose()
        {
            _layers.Pop();
        }
    }
}
