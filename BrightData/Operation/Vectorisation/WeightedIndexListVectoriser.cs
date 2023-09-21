using System;

namespace BrightData.Operation.Vectorisation
{
    internal class WeightedIndexListVectoriser : VectorisationBase<WeightedIndexList>
    {
        public WeightedIndexListVectoriser(uint maxIndex) : base(maxIndex + 1)
        {
        }

        protected override void Vectorise(in WeightedIndexList item, Span<float> buffer)
        {
            foreach (ref readonly var index in item.AsSpan())
                buffer[(int)index.Index] = index.Weight;
        }

        public override VectorisationType Type => VectorisationType.WeightedIndexList;
    }
}
