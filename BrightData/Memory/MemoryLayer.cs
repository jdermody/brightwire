using System;

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
