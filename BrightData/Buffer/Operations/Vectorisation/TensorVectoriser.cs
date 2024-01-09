using System;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Tensor vectorisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isOutput"></param>
    /// <param name="outputSize"></param>
    internal class TensorVectoriser<T>(bool isOutput, uint outputSize) : VectorisationBase<T>(isOutput, outputSize)
        where T : IHaveReadOnlyContiguousSpan<float>
    {
        protected override void Vectorise(in T item, Span<float> buffer)
        {
            item.ReadOnlySpan.CopyTo(buffer);
        }

        public override VectorisationType Type => VectorisationType.Tensor;
    }
}
