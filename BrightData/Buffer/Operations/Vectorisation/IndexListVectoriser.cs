using System;
using BrightData.Types;

namespace BrightData.Operations.Vectorisation
{
    internal class IndexListVectoriser : VectorisationBase<IndexList>
    {
        public IndexListVectoriser(bool isOutput, uint maxIndex) : base(isOutput, maxIndex + 1)
        {
        }

        protected override void Vectorise(in IndexList item, Span<float> buffer)
        {
            foreach (var index in item.Indices)
                buffer[(int)index] = 1f;
        }

        public override VectorisationType Type => VectorisationType.IndexList;
    }
}
