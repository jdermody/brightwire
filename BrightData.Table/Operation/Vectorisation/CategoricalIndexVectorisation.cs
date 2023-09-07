using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Operation.Vectorisation
{
    internal class CategoricalIndexVectorisation<T> : OneHotVectoriser<T> where T: notnull
    {
        public CategoricalIndexVectorisation(IReadOnlyBuffer<T> buffer) : base(buffer, 1)
        {
        }

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            var str = item.ToString() ?? string.Empty;
            if (!_table.TryGetValue(str, out var index))
                _table.Add(str, index = (uint)_table.Count);
            buffer[0] = index;
        }
    }
}
