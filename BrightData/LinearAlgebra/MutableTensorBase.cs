using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.Helper;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlgebra
{
    /// <summary>
    /// Base tensor type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="LAP"></typeparam>
    public abstract class MutableTensorBase<T, LAP> : ITensor<T>
        where T : ITensor
        where LAP : LinearAlgebraProvider
    {
        /// <summary>
        /// Linear algebra provider
        /// </summary>
        protected LAP Lap;

        internal MutableTensorBase(INumericSegment<float> data, LAP lap)
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
        public void WriteTo(BinaryWriter writer)
        {
            Shape.WriteTo(writer);
            var temp = SpanOwner<float>.Empty;
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
        public virtual void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var shape = reader.ReadStructArray<uint>();
            Shape = shape;
            var size = shape.Aggregate(1, (p, c) => p * (int)c);
            var data = reader.ReadBytes(size * sizeof(float));

            Lap = (LAP)context.LinearAlgebraProvider;
            Lap.AddToScope(this);

            Segment = Lap.CreateSegment((uint)size, false);
            Segment.CopyFrom(MemoryMarshal.Cast<byte, float>(data));
            Segment.AddRef();
        }

        /// <summary>
        /// Creates a typed tensor from a tensor segment
        /// </summary>
        /// <param name="segment">Tensor segment</param>
        /// <returns></returns>
        public abstract T Create(INumericSegment<float> segment);

        /// <inheritdoc />
        public abstract uint TotalSize { get; protected set; }
        uint IHaveSize.Size => TotalSize;

        /// <inheritdoc />
        public abstract uint[] Shape { get; protected set; }

        /// <summary>
        /// Underlying tensor segment
        /// </summary>
        public INumericSegment<float> Segment { get; private set; }

        /// <inheritdoc />
        public BrightDataContext Context => Lap.Context;

        /// <inheritdoc />
        public LinearAlgebraProvider LinearAlgebraProvider => Lap;

        /// <inheritdoc />
        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => Segment.GetSpan(ref temp, out wasTempUsed);

        /// <inheritdoc />
        public IVector Reshape() => Lap.CreateVector(Segment);

        /// <inheritdoc />
        public IMatrix Reshape(uint? rows, uint? columns)
        {
            var shape = TotalSize.ResolveShape(rows, columns);
            return Lap.CreateMatrix(shape[0], shape[1], Segment);
        }

        /// <inheritdoc />
        public ITensor3D Reshape(uint? depth, uint? rows, uint? columns)
        {
            var shape = TotalSize.ResolveShape(depth, rows, columns);
            return Lap.CreateTensor3D(shape[0], shape[1], shape[2], Segment);
        }

        /// <inheritdoc />
        public ITensor4D Reshape(uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = TotalSize.ResolveShape(count, depth, rows, columns);
            return Lap.CreateTensor4D(shape[0], shape[1], shape[2], shape[3], Segment);
        }

        /// <inheritdoc />
        public T Map(Func<float, float> mutator)
        {
            var ret = Lap.MapParallel(Segment, mutator);
            return Create(ret);
        }

        /// <inheritdoc />
        public void MapInPlace(Func<float, float> mutator)
        {
            var ret = Lap.MapParallel(Segment, mutator);
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }

        ITensor ITensor.Clone() => Create(Lap.Clone(Segment));

        /// <inheritdoc />
        public T Clone() => Create(Lap.Clone(Segment));

        /// <inheritdoc />
        public void Clear() => Segment.Clear();

        /// <inheritdoc />
        public T Add(ITensor tensor) => Create(Lap.Add(Segment, tensor.Segment));

        /// <inheritdoc />
        public T Add(ITensor tensor, float coefficient1, float coefficient2) => Create(Lap.Add(Segment, tensor.Segment, coefficient1, coefficient2));

        /// <inheritdoc />
        public T Add(float scalar) => Create(Lap.Add(Segment, scalar));

        /// <inheritdoc />
        public void AddInPlace(ITensor tensor) => Lap.AddInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public void AddInPlace(ITensor tensor, float coefficient1, float coefficient2) => Lap.AddInPlace(Segment, tensor.Segment, coefficient1, coefficient2);

        /// <inheritdoc />
        public void AddInPlace(float scalar) => Lap.AddInPlace(Segment, scalar);

        /// <inheritdoc />
        public void MultiplyInPlace(float scalar) => Lap.MultiplyInPlace(Segment, scalar);

        /// <inheritdoc />
        public T Multiply(float scalar) => Create(Lap.Multiply(Segment, scalar));

        /// <inheritdoc />
        public T Subtract(ITensor tensor) => Create(Lap.Subtract(Segment, tensor.Segment));

        /// <inheritdoc />
        public T Subtract(ITensor tensor, float coefficient1, float coefficient2) => Create(Lap.Subtract(Segment, tensor.Segment, coefficient1, coefficient2));

        /// <inheritdoc />
        public void SubtractInPlace(ITensor tensor) => Lap.SubtractInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public void SubtractInPlace(ITensor tensor, float coefficient1, float coefficient2) => Lap.SubtractInPlace(Segment, tensor.Segment, coefficient1, coefficient2);

        /// <inheritdoc />
        public T PointwiseMultiply(ITensor tensor) => Create(Lap.PointwiseMultiply(Segment, tensor.Segment));

        /// <inheritdoc />
        public void PointwiseMultiplyInPlace(ITensor tensor) => Lap.PointwiseMultiplyInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public T PointwiseDivide(ITensor tensor) => Create(Lap.PointwiseDivide(Segment, tensor.Segment));

        /// <inheritdoc />
        public void PointwiseDivideInPlace(ITensor tensor) => Lap.PointwiseDivideInPlace(Segment, tensor.Segment);

        /// <inheritdoc />
        public float DotProduct(ITensor tensor) => Lap.DotProduct(Segment, tensor.Segment);

        /// <inheritdoc />
        public T Sqrt() => Create(Lap.Sqrt(Segment));

        /// <inheritdoc />
        public void ConstrainInPlace(float? minValue, float? maxValue) => Lap.ConstrainInPlace(Segment, minValue, maxValue);

        /// <inheritdoc />
        public float Average() => Lap.Average(Segment);

        /// <inheritdoc />
        public float L1Norm() => Lap.L1Norm(Segment);

        /// <inheritdoc />
        public float L2Norm() => Lap.L2Norm(Segment);

        /// <inheritdoc />
        public bool IsEntirelyFinite() => Lap.IsEntirelyFinite(Segment);

        /// <inheritdoc />
        public T Reverse() => Create(Lap.Reverse(Segment));

        /// <inheritdoc />
        public IEnumerable<T> Split(uint blockCount) => Lap.Split(Segment, blockCount).Select(x => Create(Lap.Clone(x)));

        /// <inheritdoc />
        public float CosineDistance(ITensor other) => Lap.CosineDistance(Segment, other.Segment);

        /// <inheritdoc />
        public float EuclideanDistance(ITensor other) => Lap.EuclideanDistance(Segment, other.Segment);

        /// <inheritdoc />
        public float MeanSquaredDistance(ITensor other) => Lap.MeanSquaredDistance(Segment, other.Segment);

        /// <inheritdoc />
        public float SquaredEuclideanDistance(ITensor other) => Lap.SquaredEuclideanDistance(Segment, other.Segment);

        /// <inheritdoc />
        public float ManhattanDistance(ITensor other) => Lap.ManhattanDistance(Segment, other.Segment);

        /// <inheritdoc />
        public T Abs() => Create(Lap.Abs(Segment));

        /// <inheritdoc />
        public T Log() => Create(Lap.Log(Segment));

        /// <inheritdoc />
        public T Exp() => Create(Lap.Exp(Segment));

        /// <inheritdoc />
        public T Squared() => Create(Lap.Squared(Segment));

        /// <inheritdoc />
        public float StdDev(float? mean) => Lap.StdDev(Segment, mean);

        /// <inheritdoc />
        public T Sigmoid() => Create(Lap.Sigmoid(Segment));

        /// <inheritdoc />
        public T SigmoidDerivative() => Create(Lap.SigmoidDerivative(Segment));

        /// <inheritdoc />
        public T Tanh() => Create(Lap.Tanh(Segment));

        /// <inheritdoc />
        public T TanhDerivative() => Create(Lap.TanhDerivative(Segment));

        /// <inheritdoc />
        public T Relu() => Create(Lap.Relu(Segment));

        /// <inheritdoc />
        public T ReluDerivative() => Create(Lap.ReluDerivative(Segment));

        /// <inheritdoc />
        public T LeakyRelu() => Create(Lap.LeakyRelu(Segment));

        /// <inheritdoc />
        public T LeakyReluDerivative() => Create(Lap.LeakyReluDerivative(Segment));

        /// <inheritdoc />
        public T Softmax() => Create(Lap.Softmax(Segment));

        /// <inheritdoc />
        public IMatrix SoftmaxDerivative() => Lap.SoftmaxDerivative(Segment);

        /// <inheritdoc />
        public T Pow(float power) => Create(Lap.Pow(Segment, power));

        /// <inheritdoc />
        public void RoundInPlace(float lower, float upper) => Lap.RoundInPlace(Segment, lower, upper);

        /// <inheritdoc />
        public T CherryPick(uint[] indices) => Create(Lap.CherryPickIndices(Segment, indices));

        /// <inheritdoc />
        public void L1RegularisationInPlace(float coefficient) => Lap.L1Regularisation(Segment, coefficient);

        /// <inheritdoc />
        public float Sum() => Lap.Sum(Segment);

        /// <inheritdoc />
        public IReadOnlyNumericSegment<float> ReadOnlySegment => Segment;

        /// <inheritdoc cref="ITensor" />
        public bool IsReadOnly => false;
    }
}
