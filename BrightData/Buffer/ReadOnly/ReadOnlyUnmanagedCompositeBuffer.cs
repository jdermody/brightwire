using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Buffer.ReadOnly
{
    internal class ReadOnlyUnmanagedCompositeBuffer<T> : ReadOnlyCompositeBufferBase<T> where T : unmanaged
    {
        public ReadOnlyUnmanagedCompositeBuffer(Stream stream) : base(stream)
        {
        }

        protected override ReadOnlyMemory<T> Get(ReadOnlyMemory<byte> byteData)
        {
            return byteData.Cast<byte, T>();
        }
    }
}
