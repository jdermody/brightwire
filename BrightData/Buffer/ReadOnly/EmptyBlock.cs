using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Buffer.ReadOnly
{
    public class EmptyBlock<T> : ICanRandomlyAccessData<T> where T : unmanaged
    {
        public void Dispose()
        {
            // nop
        }

        public T this[int index] => default;
        public T this[uint index] => default;
        public ReadOnlySpan<T> GetSpan(uint startIndex, uint count) => new(null);
    }
}
