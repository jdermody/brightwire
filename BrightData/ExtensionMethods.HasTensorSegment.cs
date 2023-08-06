using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using CommunityToolkit.HighPerformance.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Returns the index with the minimum value from this tensor segment
        /// </summary>
        /// <param name="tensorSegment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static uint GetMinimumIndex(this IHaveTensorSegment tensorSegment) => tensorSegment.Segment.GetMinAndMaxValues().MinIndex;

        /// <summary>
        /// Returns the index with the maximum value from this tensor segment
        /// </summary>
        /// <param name="tensorSegment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static uint GetMaximumIndex(this IHaveTensorSegment tensorSegment) => tensorSegment.Segment.GetMinAndMaxValues().MaxIndex;

        /// <summary>
        /// Sums all values
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]public static float Sum(this IHaveTensorSegment segment) => segment.Segment.Sum();

        /// <summary>
        /// Pairwise addition of this with another tensor segment into a new tensor segment
        /// </summary>
        /// <param name="tensor1">This tensor</param>
        /// <param name="tensor2">Other tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Add(this IHaveTensorSegment tensor1, IHaveTensorSegment tensor2) => tensor1.Segment.Add(tensor2.Segment);

        /// <summary>
        /// Pairwise addition of this with another tensor segment into a new tensor segment
        /// </summary>
        /// <param name="tensor1">This tensor</param>
        /// <param name="tensor2">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each value in this tensor</param>
        /// <param name="coefficient2">Value to multiply each value in the other tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Add(this IHaveTensorSegment tensor1, IHaveTensorSegment tensor2, float coefficient1, float coefficient2) =>
            tensor1.Segment.Add(tensor2.Segment, coefficient1, coefficient2);

        /// <summary>
        /// Adds a scalar to each value in this tensor segment into a new tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="scalar">Scalar to add to each value</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Add(this IHaveTensorSegment tensor, float scalar) => tensor.Segment.Add(scalar);

        /// <summary>
        /// Adds another tensor segment in place to this tensor segment
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="other">Other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInPlace(this IHaveTensorSegment target, IHaveTensorSegment other) => target.Segment.AddInPlace(other.Segment);

        /// <summary>
        /// Adds another tensor segment in place to this tensor segment
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each value in this tensor</param>
        /// <param name="coefficient2">Value to multiply each value in the other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInPlace(this IHaveTensorSegment target, IHaveTensorSegment other, float coefficient1, float coefficient2) => target.Segment.AddInPlace(other.Segment, coefficient1, coefficient2);

        /// <summary>
        /// Adds a scalar to each value in this tensor segment in place
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="scalar">Scalar to add to each value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddInPlace(this IHaveTensorSegment target, float scalar) => target.Segment.AddInPlace(scalar);

        /// <summary>
        /// Multiplies each value in this tensor segment by a scalar (in place)
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="scalar">Scalar to multiply</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MultiplyInPlace(this IHaveTensorSegment target, float scalar) => target.Segment.MultiplyInPlace(scalar);

        /// <summary>
        /// Multiplies each value in this tensor segment by a scalar into a new tensor segment
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="scalar">Scalar to multiply</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Multiply(this IHaveTensorSegment target, float scalar) => target.Segment.Multiply(scalar);

        /// <summary>
        /// Subtracts another tensor segment from this tensor segment into a new tensor segment
        /// </summary>
        /// <param name="tensor1">This tensor</param>
        /// <param name="tensor2">Other tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Subtract(this IHaveTensorSegment tensor1, IHaveTensorSegment tensor2) => tensor1.Segment.Subtract(tensor2.Segment);

            /// <summary>
        /// Subtracts another tensor segment from this tensor segment into a new tensor segment
        /// </summary>
        /// <param name="tensor1">This tensor</param>
        /// <param name="tensor2">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each value in this tensor</param>
        /// <param name="coefficient2">Value to multiply each value in the other tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Subtract(this IHaveTensorSegment tensor1, IHaveTensorSegment tensor2, float coefficient1, float coefficient2) => tensor1.Segment.Subtract(tensor2.Segment, coefficient1, coefficient2);

        /// <summary>
        /// Subtracts another tensor segment from this tensor segment in place
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="other">Other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractInPlace(this IHaveTensorSegment target, IHaveTensorSegment other) => target.Segment.SubtractInPlace(other.Segment);

            /// <summary>
        /// Subtracts another tensor segment from this tensor segment in place
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <param name="coefficient1">Value to multiply each value in this tensor</param>
        /// <param name="coefficient2">Value to multiply each value in the other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractInPlace(this IHaveTensorSegment target, IHaveTensorSegment other, float coefficient1, float coefficient2) => target.Segment.SubtractInPlace(other.Segment, coefficient1, coefficient2);

        /// <summary>
        /// Pairwise multiply each value in this tensor segment with the corresponding value from another tensor segment into a new tensor segment
        /// </summary>
        /// <param name="tensor1">This tensor</param>
        /// <param name="tensor2">Other tensor</param>
        /// <returns></returns>
        public static ITensorSegment PointwiseMultiply(this IHaveTensorSegment tensor1, IHaveTensorSegment tensor2) => tensor1.Segment.PointwiseMultiply(tensor2.Segment);

        /// <summary>
        /// Pairwise multiply each value in this tensor segment with the corresponding value from another tensor segment in place
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="other">Other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PointwiseMultiplyInPlace(this IHaveTensorSegment target, IHaveTensorSegment other) => target.Segment.PointwiseMultiplyInPlace(other.Segment);

        /// <summary>
        /// Pairwise divide each value in this tensor segment with the corresponding value from another tensor segment into a new tensor segment
        /// </summary>
        /// <param name="tensor1">This tensor</param>
        /// <param name="tensor2">Other tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment PointwiseDivide(this IHaveTensorSegment tensor1, IHaveTensorSegment tensor2) => tensor1.Segment.PointwiseDivide(tensor2.Segment);

        /// <summary>
        /// Pairwise divide each value in this tensor segment with the corresponding value from another tensor segment in place
        /// </summary>
        /// <param name="target">This tensor</param>
        /// <param name="other">Other tensor</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PointwiseDivideInPlace(this IHaveTensorSegment target, IHaveTensorSegment other) => target.Segment.PointwiseDivideInPlace(other.Segment);

        /// <summary>
        /// Calculates the dot product of this with another tensor
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotProduct(this IHaveTensorSegment segment, IHaveTensorSegment other) => segment.Segment.DotProduct(other.Segment);

        /// <summary>
        /// Creates a new tensor segment that contains the square root of each value in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="adjustment">A small value to add to each value in case of zeros</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Sqrt(this IHaveTensorSegment tensor, float adjustment = FloatMath.AlmostZero) => tensor.Segment.Sqrt(adjustment);

        /// <summary>
        /// Searches this tensor segment for the index of the first value that matches the specified value within a level of tolerance
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="value">Value to find</param>
        /// <param name="tolerance">Degree of tolerance</param>
        /// <returns></returns>
        public static uint? Search(this IHaveTensorSegment segment, float value, float tolerance = FloatMath.AlmostZero) => segment.Segment.Search(value, tolerance);

        /// <summary>
        /// Constrains each value in this tensor segment to fit between a supplied minimum and maximum value
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="minInclusiveValue">Minimum allowed inclusive value (optional)</param>
        /// <param name="maxInclusiveValue">Maximum allowed inclusive value (optional)</param>
        public static void ConstrainInPlace(this IHaveTensorSegment segment, float? minInclusiveValue, float? maxInclusiveValue) => segment.Segment.ConstrainInPlace(minInclusiveValue, maxInclusiveValue);

        /// <summary>
        /// Finds the average value in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static float Average(this IHaveTensorSegment segment) => segment.Segment.Average();

        /// <summary>
        /// Calculates the L1 norm of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static float L1Norm(this IHaveTensorSegment segment) => segment.Segment.L1Norm();

        /// <summary>
        /// Calculates the L2 norm of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static float L2Norm(this IHaveTensorSegment segment) => segment.Segment.L2Norm();

        /// <summary>
        /// Finds the min and max values (and their indices) of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(this IHaveTensorSegment segment) => segment.Segment.GetMinAndMaxValues();

        /// <summary>
        /// Checks if this tensor segment is finite for each value (not NaN or Infinity)
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEntirelyFinite(this IHaveTensorSegment segment) => segment.Segment.IsEntirelyFinite();

        /// <summary>
        /// Creates a new tensor segment that is the reverse of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <returns></returns>
        public static ITensorSegment Reverse(this IHaveTensorSegment segment) => segment.Segment.Reverse();

        /// <summary>
        /// Splits this tensor segment into multiple contiguous tensor segments
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="blockCount">Number of blocks</param>
        /// <returns></returns>
        public static IEnumerable<ITensorSegment> Split(this IHaveTensorSegment segment, uint blockCount) => segment.Segment.Split(blockCount);

        /// <summary>
        /// Calculates the cosine distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static float CosineDistance(this IHaveTensorSegment tensor, IHaveTensorSegment other) => tensor.Segment.CosineDistance(other.Segment);

        /// <summary>
        /// Calculates the euclidean distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static float EuclideanDistance(this IHaveTensorSegment tensor, IHaveTensorSegment other) => tensor.Segment.EuclideanDistance(other.Segment);

        /// <summary>
        /// Calculates the mean squared distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static float MeanSquaredDistance(this IHaveTensorSegment tensor, IHaveTensorSegment other) => tensor.Segment.MeanSquaredDistance(other.Segment);

        /// <summary>
        /// Calculates the squared euclidean distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static float SquaredEuclideanDistance(this IHaveTensorSegment tensor, IHaveTensorSegment other) => tensor.Segment.SquaredEuclideanDistance(other.Segment);

        /// <summary>
        /// Calculates the manhattan distance between this and another tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <param name="other">Other tensor</param>
        /// <returns></returns>
        public static float ManhattanDistance(this IHaveTensorSegment tensor, IHaveTensorSegment other) => tensor.Segment.ManhattanDistance(other.Segment);

        /// <summary>
        /// Creates a new tensor segment that contains the absolute value of each value in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Abs(this IHaveTensorSegment tensor) => tensor.Segment.Abs();

        /// <summary>
        /// Creates a new tensor segment that contains the natural logarithm of each value in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Log(this IHaveTensorSegment tensor) => tensor.Segment.Log();

        /// <summary>
        /// Creates a new tensor segment that contains the exponent of each value in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Exp(this IHaveTensorSegment tensor) => tensor.Segment.Exp();

        /// <summary>
        /// Creates a new tensor segment that contains each value raised by the specified power in this tensor segment
        /// </summary>
        /// <param name="segment">This tensor</param>
        /// <param name="power">Specified power</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Pow(this IHaveTensorSegment segment, float power) => segment.Segment.Pow(power);

        /// <summary>
        /// Creates a new tensor segment that contains each value squared in this tensor segment
        /// </summary>
        /// <param name="tensor">This tensor</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Squared(this IHaveTensorSegment tensor) => tensor.Segment.Squared();

            /// <summary>
        /// Calculates the standard deviation of this tensor segment
        /// </summary>
        /// <param name="segment">This tensor segment</param>
        /// <param name="mean">Mean of the tensor segment (optional)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float StdDev(this IHaveTensorSegment segment, float? mean) => segment.Segment.StdDev(mean);

        /// <summary>
        /// Creates a new tensor segment with sigmoid function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Sigmoid(this IHaveTensorSegment segment) => segment.Segment.Sigmoid();

        /// <summary>
        /// Creates a new tensor segment with sigmoid derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment SigmoidDerivative(this IHaveTensorSegment segment) => segment.Segment.SigmoidDerivative();

        /// <summary>
        /// Creates a new tensor segment with tanh function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Tanh(this IHaveTensorSegment segment) => segment.Segment.Tanh();

        /// <summary>
        /// Creates a new tensor segment with tanh derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment TanhDerivative(this IHaveTensorSegment segment) => segment.Segment.TanhDerivative();

        /// <summary>
        /// Creates a new tensor segment with RELU function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Relu(this IHaveTensorSegment segment) => segment.Segment.Relu();

        /// <summary>
        /// Creates a new tensor segment with RELU derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment ReluDerivative(this IHaveTensorSegment segment) => segment.Segment.ReluDerivative();

        /// <summary>
        /// Creates a new tensor segment with Leaky RELU function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment LeakyRelu(this IHaveTensorSegment segment) => segment.Segment.LeakyRelu();

        /// <summary>
        /// Creates a new tensor segment with Leaky RELU derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment LeakyReluDerivative(this IHaveTensorSegment segment) => segment.Segment.LeakyReluDerivative();

        /// <summary>
        /// Creates a new tensor segment with softmax function applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITensorSegment Softmax(this IHaveTensorSegment segment) => segment.Segment.Softmax();

        /// <summary>
        /// Creates a new tensor segment with softmax derivative applied to each value in this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static IReadOnlyMatrix SoftmaxDerivative(this IHaveTensorSegment segment, BrightDataContext context) => segment.Segment.SoftmaxDerivative(context);

        /// <summary>
        /// Returns a new tensor segment from the values at the supplied indices from this tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="indices">Indices to copy to new tensor segment</param>
        /// <returns></returns>
        public static ITensorSegment CherryPickIndices(this IHaveTensorSegment segment, params uint[] indices) => segment.Segment.CherryPickIndices(indices);

        /// <summary>
        /// Rounds each value in this tensor segment to be either the lower or upper supplied parameters
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public static void RoundInPlace(this IHaveTensorSegment segment, float lower, float upper) => segment.Segment.RoundInPlace(lower, upper);

        /// <summary>
        /// Invokes a callback on each element of the tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="analyser">Callback that will receive each value and its corresponding index in the segment</param>
        //public static void Analyze(IHaveTensorSegment segment, Action<float /* value */, uint /* index */> analyser) => segment.Segment.Analyze(analyser);

        /// <summary>
        /// In place L1 regularization of the tensor segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="coefficient">Coefficient to apply to each adjusted value</param>
        public static void L1Regularization(this IHaveTensorSegment segment, float coefficient) => segment.Segment.L1Regularization(coefficient);

        /// <summary>
        /// Applies a mapping function to each value in the segment, potentially in parallel
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        //public static MemoryOwner<float> MapParallel(this IHaveTensorSegment segment, Func<float /* value */, float /* new value */> mapper) => segment.Segment.MapParallel(mapper);

        /// <summary>
        /// Applies a mapping function to each value in the segment, potentially in parallel
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        //public static MemoryOwner<float> MapParallel(this IHaveTensorSegment segment, Func<uint /* index */, float /* value */, float /* new value */> mapper) => segment.Segment.MapParallel(mapper);

        /// <summary>
        /// Reshapes to a vector
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyVector Reshape(this IHaveTensorSegment vector)
        {
            return new ReadOnlyVectorWrapper(vector.Segment);
        }

        /// <summary>
        /// Reshapes to a matrix
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="rows">Row count of each matrix (one parameter is optional null)</param>
        /// <param name="columns">Column count of each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        public static IReadOnlyMatrix Reshape(this IHaveTensorSegment vector, uint? rows, uint? columns)
        {
            var shape = vector.Segment.Size.ResolveShape(rows, columns);
            return new ReadOnlyMatrixWrapper(vector.Segment, shape[0], shape[1]);
        }

        /// <summary>
        /// Reshapes to a 3D tensor
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="depth">Number of matrices (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        public static IReadOnlyTensor3D Reshape(this IHaveTensorSegment vector, uint? depth, uint? rows, uint? columns)
        {
            var shape = vector.Segment.Size.ResolveShape(depth, rows, columns);
            return new ReadOnlyTensor3DWrapper(vector.Segment, shape[0], shape[1], shape[2]);
        }

        /// <summary>
        /// Reshapes to a 4D tensor
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="count">Number of 3D tensors (one parameter is optional null)</param>
        /// <param name="depth">Number of matrices in each 3D tensor (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        public static IReadOnlyTensor4D Reshape(this IHaveTensorSegment vector, uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = vector.Segment.Size.ResolveShape(count, depth, rows, columns);
            return new ReadOnlyTensor4DWrapper(vector.Segment, shape[0], shape[1], shape[2], shape[3]);
        }
    }
}
