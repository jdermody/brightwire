using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Table.Operation.Vectorisation
{
    internal class WeightedIndexListVectoriser : VectorisationBase<WeightedIndexList>
    {
        public WeightedIndexListVectoriser(IReadOnlyBuffer<WeightedIndexList> buffer, uint maxIndex) : base(buffer, maxIndex + 1)
        {
        }

        protected override void Vectorise(in WeightedIndexList item, Span<float> buffer)
        {
            foreach (ref readonly var index in item.AsSpan())
                buffer[(int)index.Index] = index.Weight;
        }
    }
}
