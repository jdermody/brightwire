using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BrightData.LinearAlgebra;
using BrightData.Serialisation;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public abstract class TensorBase2<T, LAP> : ITensor2<T>
        where T: ITensor2
        where LAP: LinearAlgebraProvider
    {
        protected LAP _lap;

        internal TensorBase2(ITensorSegment2 data, LAP lap)
        {
            Segment = data;
            Segment.AddRef();

            _lap = lap;
            _lap.AddToScope(this);
        }

        public void Dispose()
        {
            if(_lap.RemoveFromScope(this))
                Segment.Release();
        }

        public void WriteTo(BinaryWriter writer)
        {
            Shape.WriteTo(writer);
            var temp = SpanOwner<float>.Empty;
            var span = Segment.GetSpan(ref temp, out var wasTempUsed);
            try {
                writer.Write(span.AsBytes());
            }
            finally {
                if(wasTempUsed)
                    temp.Dispose();;
            }
        }

        public virtual void Initialize(BrightDataContext context, BinaryReader reader)
        {
            var shape = reader.ReadStructArray<uint>();
            Shape = shape;
            var size = shape.Aggregate(1, (p, c) => p * (int)c);
            var data = reader.ReadBytes(size * sizeof(float));

            _lap = (LAP)context.LinearAlgebraProvider2;
            _lap.AddToScope(this);

            Segment = _lap.CreateSegment((uint)size);
            Segment.CopyFrom(MemoryMarshal.Cast<byte, float>(data));
            Segment.AddRef();
        }

        public abstract T Create(ITensorSegment2 segment);
        public abstract uint TotalSize { get; protected set; }
        public abstract uint[] Shape { get; protected set; }
        public ITensorSegment2 Segment { get; private set; }
        public BrightDataContext Context => _lap.Context;
        public LinearAlgebraProvider LinearAlgebraProvider => _lap;

        public ReadOnlySpan<float> GetSpan(ref SpanOwner<float> temp, out bool wasTempUsed) => Segment.GetSpan(ref temp, out wasTempUsed);

        public IVector Reshape() => _lap.CreateVector(Segment);
        public IMatrix Reshape(uint? rows, uint? columns)
        {
            var shape = ResolveShape(TotalSize, rows, columns);
            return _lap.CreateMatrix(shape[0], shape[1], Segment);
        }
        public ITensor3D Reshape(uint? depth, uint? rows, uint? columns)
        {
            var shape = ResolveShape(TotalSize, depth, rows, columns);
            return _lap.CreateTensor3D(shape[0], shape[1], shape[2], Segment);
        }
        public ITensor4D Reshape(uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = ResolveShape(TotalSize, count, depth, rows, columns);
            return _lap.CreateTensor4D(shape[0], shape[1], shape[2], shape[3], Segment);
        }
        static uint[] ResolveShape(uint total, params uint?[] shape)
        {
            uint nonNullTotal = 0;
            var hasFoundNull = false;
            foreach (var item in shape)
            {
                if (item.HasValue)
                    nonNullTotal += item.Value;
                else if (!hasFoundNull)
                    hasFoundNull = true;
                else
                    throw new ArgumentException("Only one parameter can be null");
            }

            if (hasFoundNull && nonNullTotal == 0)
                throw new ArgumentException("Cannot resolve null parameter");

            return shape.Select(v => v ?? total / nonNullTotal).ToArray();
        }

        public T Map(Func<float, float> mutator)
        {
            var ret = _lap.MapParallel(Segment, mutator);
            return Create(ret);
        }

        public void MapInPlace(Func<float, float> mutator)
        {
            var ret = _lap.MapParallel(Segment, mutator);
            try {
                ret.CopyTo(Segment);
            }
            finally {
                ret.Release();
            }
        }

        ITensor2 ITensor2.Clone()                                                            => Create(_lap.Clone(Segment));
        public T Clone()                                                                     => Create(_lap.Clone(Segment));
        public void Clear()                                                                  => Segment.Clear();
        public T Add(ITensor2 tensor)                                                        => Create(_lap.Add(Segment, tensor.Segment));
        public T Add(ITensor2 tensor, float coefficient1, float coefficient2)                => Create(_lap.Add(Segment, tensor.Segment, coefficient1, coefficient2));
        public T Add(float scalar)                                                           => Create(_lap.Add(Segment, scalar));
        public void AddInPlace(ITensor2 tensor)                                              => _lap.AddInPlace(Segment, tensor.Segment);
        public void AddInPlace(ITensor2 tensor, float coefficient1, float coefficient2)      => _lap.AddInPlace(Segment, tensor.Segment, coefficient1, coefficient2);
        public void AddInPlace(float scalar)                                                 => _lap.AddInPlace(Segment, scalar);
        public void MultiplyInPlace(float scalar)                                            => _lap.MultiplyInPlace(Segment, scalar);
        public T Multiply(float scalar)                                                      => Create(_lap.Multiply(Segment, scalar));
        public T Subtract(ITensor2 tensor)                                                   => Create(_lap.Subtract(Segment, tensor.Segment));
        public T Subtract(ITensor2 tensor, float coefficient1, float coefficient2)           => Create(_lap.Subtract(Segment, tensor.Segment, coefficient1, coefficient2));
        public void SubtractInPlace(ITensor2 tensor)                                         => _lap.SubtractInPlace(Segment, tensor.Segment);
        public void SubtractInPlace(ITensor2 tensor, float coefficient1, float coefficient2) => _lap.SubtractInPlace(Segment, tensor.Segment, coefficient1, coefficient2);
        public T PointwiseMultiply(ITensor2 tensor)                                          => Create(_lap.PointwiseMultiply(Segment, tensor.Segment));
        public void PointwiseMultiplyInPlace(ITensor2 tensor)                                => _lap.PointwiseMultiplyInPlace(Segment, tensor.Segment);
        public T PointwiseDivide(ITensor2 tensor)                                            => Create(_lap.PointwiseDivide(Segment, tensor.Segment));
        public void PointwiseDivideInPlace(ITensor2 tensor)                                  => _lap.PointwiseDivideInPlace(Segment, tensor.Segment);
        public float DotProduct(ITensor2 tensor)                                             => _lap.DotProduct(Segment, tensor.Segment);
        public T Sqrt()                                                                      => Create(_lap.Sqrt(Segment));
        public uint? Search(float value)                                                     => _lap.Search(Segment, value);
        public void ConstrainInPlace(float? minValue, float? maxValue)                       => _lap.ConstrainInPlace(Segment, minValue, maxValue);
        public float Average()                                                               => _lap.Average(Segment);
        public float L1Norm()                                                                => _lap.L1Norm(Segment);
        public float L2Norm()                                                                => _lap.L2Norm(Segment);
        public (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues()     => _lap.GetMinAndMaxValues(Segment);
        public uint GetMinIndex()                                                            => _lap.GetMinIndex(Segment);
        public uint GetMaxIndex()                                                            => _lap.GetMaxIndex(Segment);
        public float GetMin()                                                                => _lap.GetMin(Segment);
        public float GetMax()                                                                => _lap.GetMax(Segment);
        public bool IsEntirelyFinite()                                                       => _lap.IsEntirelyFinite(Segment);
        public T Reverse()                                                                   => Create(_lap.Reverse(Segment));
        public IEnumerable<T> Split(uint blockCount)                                         => _lap.Split(Segment, blockCount).Select(Create);
        public float CosineDistance(ITensor2 other)                                          => _lap.CosineDistance(Segment, other.Segment);
        public float EuclideanDistance(ITensor2 other)                                       => _lap.EuclideanDistance(Segment, other.Segment);
        public float MeanSquaredDistance(ITensor2 other)                                     => _lap.MeanSquaredDistance(Segment, other.Segment);
        public float SquaredEuclideanDistance(ITensor2 other)                                => _lap.SquaredEuclideanDistance(Segment, other.Segment);
        public float ManhattanDistance(ITensor2 other)                                       => _lap.ManhattanDistance(Segment, other.Segment);
        public T Abs()                                                                       => Create(_lap.Abs(Segment));
        public T Log()                                                                       => Create(_lap.Log(Segment));
        public T Exp()                                                                       => Create(_lap.Exp(Segment));
        public T Squared()                                                                   => Create(_lap.Squared(Segment));
        public float StdDev(float? mean)                                                     => _lap.StdDev(Segment, mean);
        public T Sigmoid()                                                                   => Create(_lap.Sigmoid(Segment));
        public T SigmoidDerivative()                                                         => Create(_lap.SigmoidDerivative(Segment));
        public T Tanh()                                                                      => Create(_lap.Tanh(Segment));
        public T TanhDerivative()                                                            => Create(_lap.TanhDerivative(Segment));
        public T Relu()                                                                      => Create(_lap.Relu(Segment));
        public T ReluDerivative()                                                            => Create(_lap.ReluDerivative(Segment));
        public T LeakyRelu()                                                                 => Create(_lap.LeakyRelu(Segment));
        public T LeakyReluDerivative()                                                       => Create(_lap.LeakyReluDerivative(Segment));
        public T Softmax()                                                                   => Create(_lap.Softmax(Segment));
        public IMatrix SoftmaxDerivative()                                                   => _lap.SoftmaxDerivative(Segment);
        public T Pow(float power)                                                            => Create(_lap.Pow(Segment, power));
        public void RoundInPlace(float lower, float upper, float? mid)                       => _lap.RoundInPlace(Segment, lower, upper, mid);
        public T CherryPick(uint[] indices)                                                  => Create(_lap.CherryPickIndices(Segment, indices));
        public void L1Regularisation(float coefficient)                                      => _lap.L1Regularisation(Segment, coefficient);
    }
}
