using System;
using BrightData.Types;

namespace BrightData.Buffer.Vectorisation
{
    /// <summary>
    /// Weighted index vectorisation
    /// </summary>
    /// <param name="maxIndex"></param>
    internal class WeightedIndexListVectoriser(uint maxIndex) : VectorisationBase<WeightedIndexList>(maxIndex + 1)
    {
        protected override void Vectorise(in WeightedIndexList item, Span<float> buffer)
        {
            foreach (ref readonly var index in item.AsSpan())
                buffer[(int)index.Index] = index.Weight;
        }

        public override VectorisationType Type => VectorisationType.WeightedIndexList;
    }
}
