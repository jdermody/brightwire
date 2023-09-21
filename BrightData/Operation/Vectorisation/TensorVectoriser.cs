using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.Operation.Vectorisation
{
    internal class TensorVectoriser<T> : VectorisationBase<T> where T: IHaveReadOnlyContiguousSpan<float>
    {
        public TensorVectoriser(uint outputSize) : base(outputSize)
        {
        }

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            item.ReadOnlySpan.CopyTo(buffer);
        }

        public override VectorisationType Type => VectorisationType.Tensor;
    }
}
