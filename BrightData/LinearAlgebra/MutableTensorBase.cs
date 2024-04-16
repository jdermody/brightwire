using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Base tensor type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="LAP"></typeparam>
    /// <typeparam name="TT"></typeparam>
    /// <typeparam name="RTT"></typeparam>
    public abstract unsafe class MutableTensorBase<T, RTT, TT, LAP> : ReadOnlyTensorBase<T, RTT>, ITensorType<T, RTT, TT>
        where T: unmanaged, IBinaryFloatingPointIeee754<T>, IMinMaxValue<T>
        where RTT: IReadOnlyTensor<T>
        where TT : ITensor<T>
        where LAP : LinearAlgebraProvider<T>
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        protected LAP Lap;

        internal MutableTensorBase(INumericSegment<T> data, LAP lap) : base(data)
        {
            Segment = data;
            Segment.AddRef();

            Lap = lap;
            Lap.AddToScope(this);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Lap.RemoveFromScope(this);
            Segment.Release();
        }

        /// <inheritdoc />
        public override void WriteTo(BinaryWriter writer)
        {
            Shape.WriteTo(writer);
            var temp = SpanOwner<T>.Empty;
            var span = Segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                writer.Write(span.AsBytes());
            }
            finally {
                if (wasTempUsed)
                    temp.Dispose();
            }
        }

        /// <inheritdoc />
        public override void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var shape = reader.ReadStructArray<uint>();
            Shape = shape;
            var size = shape.Aggregate(1, (p, c) => p * (int)c);
            var data = reader.ReadBytes(size * sizeof(T));

            if (context.LinearAlgebraProvider is not LAP lap)
                throw new ArgumentException($"Expected linear algebra provider of type {typeof(T)}");
            Lap = lap;
            Lap.AddToScope(this);

            Segment = Lap.CreateSegment((uint)size, false);
            Segment.CopyFrom(MemoryMarshal.Cast<byte, T>(data));
            Segment.AddRef();

            ReadOnlySegment = Segment;
        }

        /// <summary>
        /// Creates a typed tensor from a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment</param>
        /// <returns></returns>
        public abstract TT Create(INumericSegment<T> segment);

        /// <inheritdoc />
        public abstract uint TotalSize { get; protected set; }
        uint IHaveSize.Size => TotalSize;

        /// <inheritdoc />
        public abstract uint[] Shape { get; protected set; }

        /// <summary>
        /// Underlying tensor segment
        /// </summary>
        public INumericSegment<T> Segment { get; private set; }

        /// <inheritdoc />
        public BrightDataContext Context => Lap.Context;

        /// <inheritdoc />
        public LinearAlgebraProvider<T> LinearAlgebraProvider => Lap;

        /// <inheritdoc />
        public IVector<T> Reshape() => Lap.CreateVector(Segment);

        /// <inheritdoc />
        public IMatrix<T> Reshape(uint? rows, uint? columns)
        {
            var shape = TotalSize.ResolveShape(rows, columns);
            return Lap.CreateMatrix(shape[0], shape[1], Segment);
        }

        /// <inheritdoc />
        public ITensor3D<T> Reshape(uint? depth, uint? rows, uint? columns)
        {
            var shape = TotalSize.ResolveShape(depth, rows, columns);
            return Lap.CreateTensor3D(shape[0], shape[1], shape[2], Segment);
        }

        /// <inheritdoc />
        public ITensor4D<T> Reshape(uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = TotalSize.ResolveShape(count, depth, rows, columns);
            return Lap.CreateTensor4D(shape[0], shape[1], shape[2], shape[3], Segment);
        }

        /// <inheritdoc />
        public new TT Map(Func<T, T> mutator)
        {
            var ret = Segment.MapParallel(mutator);
            return Create(ret);
        }

        /// <inheritdoc />
        public new TT MapIndexed(Func<uint, T, T> mutator)
        {
            var ret = Segment.MapParallel(mutator);
            return Create(ret);
        }

        /// <inheritdoc />
        public void MapInPlace(Func<T, T> mutator)
        {
            var ret = Segment.MapParallel(mutator);
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }

        /// <inheritdoc />
        public void MapIndexedInPlace(Func<uint, T, T> mutator)
        {
            var ret = Segment.MapParallel(mutator);
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }

        ITensor<T> ITensor<T>.Clone() => Create(Lap.Clone(Segment));

        /// <inheritdoc />
        public TT Clone() => Create(Lap.Clone(Segment));

        /// <inheritdoc />
        public void Clear() => Segment.Clear();

        /// <inheritdoc />
        public TT Add(ITensor<T> tensor) => Create(Lap.Add(Segment, tensor.Segment));

        /// <inheritdoc />
        public TT Add(ITensor<T> tensor, T coefficient1, T coefficient2) => Create(Lap.Add(Segment, tensor.Segment, coefficient1, coefficient2));

        /// <inheritdoc />
        public new TT Add(T scalar) => Create(Lap.Add(Segment, scalar));

        /// <inheritdoc />
        public void AddInPlace(ITensor<T> tensor) => Lap.AddInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public void AddInPlace(ITensor<T> tensor, T coefficient1, T coefficient2) => Lap.AddInPlace(Segment, tensor.Segment, coefficient1, coefficient2);

        /// <inheritdoc />
        public void AddInPlace(T scalar) => Lap.AddInPlace(Segment, scalar);

        /// <inheritdoc />
        public void MultiplyInPlace(T scalar) => Lap.MultiplyInPlace(Segment, scalar);

        /// <inheritdoc />
        public new TT Multiply(T scalar) => Create(Lap.Multiply(Segment, scalar));

        /// <inheritdoc />
        public TT Subtract(ITensor<T> tensor) => Create(Lap.Subtract(Segment, tensor.Segment));

        /// <inheritdoc />
        public TT Subtract(ITensor<T> tensor, T coefficient1, T coefficient2) => Create(Lap.Subtract(Segment, tensor.Segment, coefficient1, coefficient2));

        /// <inheritdoc />
        public void SubtractInPlace(ITensor<T> tensor) => Lap.SubtractInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public void SubtractInPlace(ITensor<T> tensor, T coefficient1, T coefficient2) => Lap.SubtractInPlace(Segment, tensor.Segment, coefficient1, coefficient2);

        /// <inheritdoc />
        public TT PointwiseMultiply(ITensor<T> tensor) => Create(Lap.PointwiseMultiply(Segment, tensor.Segment));

        /// <inheritdoc />
        public void PointwiseMultiplyInPlace(ITensor<T> tensor) => Lap.PointwiseMultiplyInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public TT PointwiseDivide(ITensor<T> tensor) => Create(Lap.PointwiseDivide(Segment, tensor.Segment));

        /// <inheritdoc />
        public void PointwiseDivideInPlace(ITensor<T> tensor) => Lap.PointwiseDivideInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public new T DotProduct(ITensor<T> tensor) => Lap.DotProduct(Segment, tensor.Segment);

        /// <inheritdoc />
        public TT Sqrt() => Create(Lap.Sqrt(Segment));

        /// <inheritdoc />
        public void ConstrainInPlace(T? minValue, T? maxValue) => Lap.ConstrainInPlace(Segment, minValue, maxValue);

        /// <inheritdoc />
        public new T Average() => Lap.Average(Segment);

        /// <inheritdoc />
        public new T L1Norm() => Lap.L1Norm(Segment);

        /// <inheritdoc />
        public new T L2Norm() => Lap.L2Norm(Segment);

        /// <inheritdoc />
        public new bool IsEntirelyFinite() => Lap.IsEntirelyFinite(Segment);

        /// <inheritdoc />
        public new TT Reverse() => Create(Lap.Reverse(Segment));

        /// <inheritdoc />
        public IEnumerable<TT> Split(uint blockCount) => Lap.Split(Segment, blockCount).Select(x => Create(Lap.Clone(x)));

        /// <inheritdoc />
        public new T CosineDistance(IReadOnlyTensor<T> other) => Lap.CosineDistance(Segment, other.ReadOnlySegment);

        /// <inheritdoc />
        public new T EuclideanDistance(IReadOnlyTensor<T> other) => Lap.EuclideanDistance(Segment, other.ReadOnlySegment);

        /// <inheritdoc />
        public new T MeanSquaredDistance(IReadOnlyTensor<T> other) => Lap.MeanSquaredDistance(Segment, other.ReadOnlySegment);

        /// <inheritdoc />
        public new T SquaredEuclideanDistance(IReadOnlyTensor<T> other) => Lap.SquaredEuclideanDistance(Segment, other.ReadOnlySegment);

        /// <inheritdoc />
        public new T ManhattanDistance(IReadOnlyTensor<T> other) => Lap.ManhattanDistance(Segment, other.ReadOnlySegment);

        /// <inheritdoc />
        public new TT Abs() => Create(Lap.Abs(Segment));

        /// <inheritdoc />
        public new TT Log() => Create(Lap.Log(Segment));

        /// <inheritdoc />
        public new TT Exp() => Create(Lap.Exp(Segment));

        /// <inheritdoc />
        public new TT Squared() => Create(Lap.Squared(Segment));

        /// <inheritdoc />
        public new T StdDev(T? mean) => Lap.StdDev(Segment, mean);

        /// <inheritdoc />
        public new TT Sigmoid() => Create(Lap.Sigmoid(Segment));

        /// <inheritdoc />
        public new TT SigmoidDerivative() => Create(Lap.SigmoidDerivative(Segment));

        /// <inheritdoc />
        public new TT Tanh() => Create(Lap.Tanh(Segment));

        /// <inheritdoc />
        public new TT TanhDerivative() => Create(Lap.TanhDerivative(Segment));

        /// <inheritdoc />
        public new TT Relu() => Create(Lap.Relu(Segment));

        /// <inheritdoc />
        public new TT ReluDerivative() => Create(Lap.ReluDerivative(Segment));

        /// <inheritdoc />
        public new TT LeakyRelu() => Create(Lap.LeakyRelu(Segment));

        /// <inheritdoc />
        public new TT LeakyReluDerivative() => Create(Lap.LeakyReluDerivative(Segment));

        /// <inheritdoc />
        public new TT Softmax() => Create(Lap.Softmax(Segment));

        /// <inheritdoc />
        public IMatrix<T> SoftmaxDerivative() => Lap.SoftmaxDerivative(Segment);

        /// <inheritdoc />
        public new TT Pow(T power) => Create(Lap.Pow(Segment, power));

        /// <inheritdoc />
        public void RoundInPlace(T lower, T upper) => Lap.RoundInPlace(Segment, lower, upper);

        /// <inheritdoc />
        public new TT CherryPick(uint[] indices) => Create(Lap.CherryPickIndices(Segment, indices));

        /// <inheritdoc />
        public void L1RegularisationInPlace(T coefficient) => Lap.L1Regularisation(Segment, coefficient);

        /// <inheritdoc />
        public new T Sum() => Lap.Sum(Segment);
    }
}
