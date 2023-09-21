using System;

namespace BrightData.Operation.Vectorisation
{
    internal class IndexListVectoriser : VectorisationBase<IndexList>
    {
        public IndexListVectoriser(uint maxIndex) : base(maxIndex + 1)
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
