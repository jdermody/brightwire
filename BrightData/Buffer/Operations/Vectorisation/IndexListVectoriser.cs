using System;
using BrightData.Types;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Vectorisation of index lists
    /// </summary>
    /// <param name="isOutput"></param>
    /// <param name="maxIndex"></param>
    internal class IndexListVectoriser(bool isOutput, uint maxIndex) : VectorisationBase<IndexList>(isOutput, maxIndex + 1)
    {
        protected override void Vectorise(in IndexList item, Span<float> buffer)
        {
            foreach (var index in item.Indices)
                buffer[(int)index] = 1f;
        }

        public override VectorisationType Type => VectorisationType.IndexList;
    }
}
