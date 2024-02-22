using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.LinearAlgebra.CostFunctions
{
    internal class QuadraticCostFunction<T> : CostFunctionBase, ICostFunction<T> where T: unmanaged, INumber<T>
    {
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            throw new NotImplementedException();
        }
    }
}
