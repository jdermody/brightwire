using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using BrightData.Helper;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra.ReadOnly
{
    /// <summary>
    /// Read only tensor base class
    /// </summary>
    /// <param name="segment"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TT"></typeparam>
    public abstract class ReadOnlyTensorBase<T, TT>(IReadOnlyNumericSegment<T> segment) : IReadOnlyTensorType<T, TT>, IHaveDataAsReadOnlyByteSpan
        where T : unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where TT : IReadOnlyTensor<T>
    {
        /// <inheritdoc />
        public IReadOnlyNumericSegment<T> ReadOnlySegment { get; protected set; } = segment;

        /// <inheritdoc />
        public ReadOnlySpan<T> GetSpan(ref SpanOwner<T> temp, out bool wasTempUsed) => ReadOnlySegment.GetSpan(ref temp, out wasTempUsed);

        /// <inheritdoc />
        public uint Size => ReadOnlySegment.Size;

        /// <inheritdoc />
        public abstract void WriteTo(BinaryWriter writer);

        /// <inheritdoc />
        public abstract void Initialize(BrightDataContext context, BinaryReader reader);

        /// <inheritdoc />
        public T[] ToArray() => ReadOnlySegment.ToNewArray();

        /// <inheritdoc />
        public (T Min, T Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues() => ReadOnlySegment.ApplyReadOnlySpan(x => x.GetMinAndMaxValues());

        /// <inheritdoc />
        public T Sum() => ReadOnlySegment.ApplyReadOnlySpan(x => x.Sum());

        /// <inheritdoc />
        public T Average() => ReadOnlySegment.ApplyReadOnlySpan(x => x.Average());

        /// <inheritdoc />
        public T L1Norm() => ReadOnlySegment.ApplyReadOnlySpan(x => x.L1Norm());

        /// <inheritdoc />
        public T L2Norm() => ReadOnlySegment.ApplyReadOnlySpan(x => x.L2Norm());

        /// <inheritdoc />
        public bool IsEntirelyFinite() => ReadOnlySegment.ApplyReadOnlySpan(x => x.IsEntirelyFinite());

        /// <inheritdoc />
        public T StdDev(T? mean) => ReadOnlySegment.ApplyReadOnlySpan(x => x.StdDev(mean));

        /// <inheritdoc />
        public T CosineDistance(IReadOnlyTensor<T> other) => ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.CosineDistance(y));

        /// <inheritdoc />
        public T EuclideanDistance(IReadOnlyTensor<T> other) => ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.EuclideanDistance(y));

        /// <inheritdoc />
        public T ManhattanDistance(IReadOnlyTensor<T> other) => ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.ManhattanDistance(y));

        /// <inheritdoc />
        public T MeanSquaredDistance(IReadOnlyTensor<T> other) => ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.MeanSquaredDistance(y));

        /// <inheritdoc />
        public T SquaredEuclideanDistance(IReadOnlyTensor<T> other) => ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.SquaredEuclideanDistance(y));

        /// <inheritdoc />
        public T FindDistance(IReadOnlyTensor<T> other, DistanceMetric distance) => ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.FindDistance(y, distance));

        /// <inheritdoc />
        public T DotProduct(ITensor<T> tensor) => ReadOnlySegment.ApplyReadOnlySpans(tensor.ReadOnlySegment, (x, y) => x.DotProduct(y));

        /// <inheritdoc />
        public TT Add(IReadOnlyTensor<T> other) => Create(ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.Add(y)));

        /// <inheritdoc />
        public TT Add(IReadOnlyTensor<T> other, T coefficient1, T coefficient2) => Create(ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.Add(y, coefficient1, coefficient2)));

        /// <inheritdoc />
        public TT Add(T scalar) => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Add(scalar)));

        /// <inheritdoc />
        public TT Multiply(T scalar) => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Multiply(scalar)));

        /// <inheritdoc />
        public TT Subtract(IReadOnlyTensor<T> other) => Create(ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.Subtract(y)));

        /// <inheritdoc />
        public TT Subtract(IReadOnlyTensor<T> other, T coefficient1, T coefficient2) => Create(ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.Subtract(y, coefficient1, coefficient2)));

        /// <inheritdoc />
        public TT PointwiseMultiply(IReadOnlyTensor<T> other) => Create(ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.PointwiseMultiply(y)));

        /// <inheritdoc />
        public TT PointwiseDivide(IReadOnlyTensor<T> other) => Create(ReadOnlySegment.ApplyReadOnlySpans(other.ReadOnlySegment, (x, y) => x.PointwiseDivide(y)));

        /// <inheritdoc />
        public TT Sqrt(T? adjustment = null) => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Sqrt(adjustment ?? Math<T>.AlmostZero)));

        /// <inheritdoc />
        public TT Reverse() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Reverse()));

        /// <inheritdoc />
        public TT Abs() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Abs()));

        /// <inheritdoc />
        public TT Log() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Log()));

        /// <inheritdoc />
        public TT Exp() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Exp()));

        /// <inheritdoc />
        public TT Pow(T power) => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Pow(power)));

        /// <inheritdoc />
        public TT Squared() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Squared()));

        /// <inheritdoc />
        public TT Sigmoid() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Sigmoid()));

        /// <inheritdoc />
        public TT SigmoidDerivative() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.SigmoidDerivative()));

        /// <inheritdoc />
        public TT Tanh() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Tanh()));

        /// <inheritdoc />
        public TT TanhDerivative() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.TanhDerivative()));

        /// <inheritdoc />
        public TT Relu() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Relu()));

        /// <inheritdoc />
        public TT ReluDerivative() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.ReluDerivative()));

        /// <inheritdoc />
        public TT LeakyRelu() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.LeakyRelu()));

        /// <inheritdoc />
        public TT LeakyReluDerivative() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.LeakyReluDerivative()));

        /// <inheritdoc />
        public TT Softmax() => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.Softmax()));

        /// <inheritdoc />
        public TT SoftmaxDerivative(int rowCount) => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.SoftmaxDerivative(rowCount)));

        /// <inheritdoc />
        public TT CherryPick(params uint[] indices) => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.CherryPickIndices(indices)));

        /// <inheritdoc />
        public TT Map(Func<T, T> mutator) => Create(ReadOnlySegment.ApplyReadOnlySpan(x => x.MapParallel(mutator)));

        /// <inheritdoc />
        public abstract ReadOnlySpan<byte> DataAsBytes { get; }

        /// <summary>
        /// Creates a tensor type from a memory owner of T
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        protected abstract TT Create(MemoryOwner<T> memory);

        /// <inheritdoc />
        public IEnumerable<T> Values => ReadOnlySegment.Values;
    }
}
