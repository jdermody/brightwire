using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra.CostFunctions
{
    internal class MeanSquaredErrorCostFunction : ICostFunction<float>
    {
        public float Cost(IReadOnlyNumericSegment<float> predicted, IReadOnlyNumericSegment<float> expected)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyNumericSegment<float> Gradient(IReadOnlyNumericSegment<float> predicted, IReadOnlyNumericSegment<float> expected)
        {
            throw new NotImplementedException();
        }
    }
}
