using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Buffer2
{
    //public class ReadOnlyReferenceEnumerator<T> : IReadOnlyEnumerator<T> where T: unmanaged
    //{
    //    int _index = -1;
    //    readonly ReadOnlyMemory<T> _memory;

    //    public ReadOnlyReferenceEnumerator(ReadOnlyMemory<T> memory)
    //    {
    //        _memory = memory;
    //    }

    //    public bool MoveNext() => ++_index < _memory.Length;
    //    public void Reset() => _index = -1;
    //    public ref readonly T Current => ref _memory.Span[_index];
    //}

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
