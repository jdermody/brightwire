using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance;
using System;

namespace BrightData.Buffer.Operations.Vectorisation
{
    /// <summary>
    /// Tensor vectorisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="outputSize"></param>
    internal class TensorVectoriser<T>(uint outputSize) : VectorisationBase<T>(outputSize)
        where T : IHaveReadOnlyTensorSegment<float>
    {
        protected override void Vectorise(in T item, Span<float> buffer)
        {
            var segment = item.ReadOnlySegment;
            var contiguous = segment.Contiguous;
            if(contiguous is not null)
                contiguous.ReadOnlySpan.CopyTo(buffer);
            else {
                var temp = SpanOwner<float>.Empty;
                var wasTempUsed = false;
                try {
                    var span = segment.GetSpan(ref temp, out wasTempUsed);
                    span.CopyTo(buffer);
                }
                finally {
                    if (wasTempUsed)
                        temp.Dispose();
                }
            }
        }

        public override VectorisationType Type => VectorisationType.Tensor;
    }
}
