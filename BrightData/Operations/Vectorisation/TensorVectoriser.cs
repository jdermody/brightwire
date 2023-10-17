﻿using System;

namespace BrightData.Operations.Vectorisation
{
    internal class TensorVectoriser<T> : VectorisationBase<T> where T: IHaveReadOnlyContiguousSpan<float>
    {
        public TensorVectoriser(bool isOutput, uint outputSize) : base(isOutput, outputSize)
        {
        }

        protected override void Vectorise(in T item, Span<float> buffer)
        {
            item.ReadOnlySpan.CopyTo(buffer);
        }

        public override VectorisationType Type => VectorisationType.Tensor;
    }
}
