using System;
using BrightData.Types;

namespace BrightData.Buffer.Vectorisation
{
    /// <summary>
    /// Vectorisation of index lists
    /// </summary>
    /// <param name="maxIndex"></param>
    internal class IndexListVectoriser(uint maxIndex) : VectorisationBase<IndexList>(maxIndex + 1)
    {
        protected override void Vectorise(in IndexList item, Span<float> buffer)
        {
            foreach (var index in item.Indices)
                buffer[(int)index] = 1f;
        }

        public override VectorisationType Type => VectorisationType.IndexList;
    }
}
