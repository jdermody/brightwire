using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Operation.Vectorisation
{
    internal class IndexListVectoriser : VectorisationBase<IndexList>
    {
        public IndexListVectoriser(IReadOnlyBuffer<IndexList> buffer, uint maxIndex) : base(buffer, maxIndex + 1)
        {
        }

        protected override void Vectorise(in IndexList item, Span<float> buffer)
        {
            foreach (var index in item.Indices)
                buffer[(int)index] = 1f;
        }
    }
}
