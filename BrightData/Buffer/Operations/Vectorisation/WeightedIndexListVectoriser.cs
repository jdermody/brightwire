using System;
using BrightData.Types;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Weighted index vectorisation
    /// </summary>
    /// <param name="isOutput"></param>
    /// <param name="maxIndex"></param>
    internal class WeightedIndexListVectoriser(bool isOutput, uint maxIndex) : VectorisationBase<WeightedIndexList>(isOutput, maxIndex + 1)
    {
        protected override void Vectorise(in WeightedIndexList item, Span<float> buffer)
        {
            foreach (ref readonly var index in item.AsSpan())
                buffer[(int)index.Index] = index.Weight;
        }

        public override VectorisationType Type => VectorisationType.WeightedIndexList;
    }
}
