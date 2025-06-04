using BrightData.Helper;
using System.Numerics;

namespace BrightData.LinearAlgebra.CostFunctions
{
    internal class CrossEntropyCostFunction<T>(LinearAlgebraProvider<T> lap) : ICostFunction<T>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
    {
        public T Cost(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected)
        {
            using var ones              = lap.CreateSegment(expected.Size, T.One);
            using var oneMinusExpected  = lap.Subtract(ones, expected);
            using var oneMinusPredicted = lap.Subtract(ones, predicted);
            lap.AddInPlace(oneMinusPredicted, Math<T>.AlmostZero);

            using var logOneMinusPredicted = lap.Log(oneMinusPredicted);
            using var predictedEpsilon = lap.Add(predicted, Math<T>.AlmostZero);
            using var logPredicted = lap.Log(predictedEpsilon);
            using var expectedTimesLogPredicted = lap.PointwiseMultiply(expected, logPredicted);
            using var oneMinusExpectedTimesLogOneMinusPredicted = lap.PointwiseMultiply(oneMinusExpected, logOneMinusPredicted);
            using var result = lap.Add(expectedTimesLogPredicted, oneMinusExpectedTimesLogOneMinusPredicted);
            var ret = lap.Average(result);
            return ret * T.NegativeOne;
        }

        public IReadOnlyNumericSegment<T> Gradient(IReadOnlyNumericSegment<T> predicted, IReadOnlyNumericSegment<T> expected) => lap.Subtract(expected, predicted);
    }
}
