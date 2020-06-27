using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Helper;

namespace BrightML.CostFunctions
{
    public class MeanSquaredError
    {
        public T CalculateCost<T>(ITensor<T> prediction, ITensor<T> actual)
            where T:struct
        {

            var computation = prediction.Computation;
            var error = computation.Subtract(prediction.Data, actual.Data);
            computation.PointwiseMultiplyInPlace(error, error);
            return computation.Average(error);
        }

        public ITensorSegment<T> Derivative<T>(ITensor<T> prediction, ITensor<T> actual)
            where T : struct
        {
            var computation = prediction.Computation;
            var derivative = computation.Subtract(prediction.Data, actual.Data);
            computation.MultiplyInPlace(derivative, computation.Get(2.0 / prediction.GetSize()));
            return derivative;
        }
    }
}
