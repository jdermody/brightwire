using System;

namespace BrightData.LinearAlgebra.Memory
{
    /// <summary>
    /// A section of memory blocks that will be released when the layer is disposed
    /// </summary>
    internal class MemoryLayer : IDisposable
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
