using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightData.LinearAlegbra2
{
    public abstract class TensorBase2<T, CU> : ITensor2<T>, IDisposable
        where T: ITensor2
        where CU: ComputationUnit
    {
        protected readonly CU _computationUnit;

        internal TensorBase2(ITensorSegment2 data, CU computationUnit)
        {
            Segment = data;
            Segment.AddRef();

            _computationUnit = computationUnit;
            _computationUnit.AddToScope(this);
        }

        public void Dispose()
        {
            if(_computationUnit.RemoveFromScope(this))
                Segment.Release();
        }

        public abstract T Create(ITensorSegment2 segment);
        public abstract uint Size { get; }

        public ITensorSegment2 Segment { get; }
        public BrightDataContext2 Context => _computationUnit.Context;

        public T Clone() => Create(_computationUnit.Clone(Segment));
        public IVector Reshape() => _computationUnit.CreateVector(Segment);
        public IMatrix Reshape(uint? rows, uint? columns)
        {
            var shape = ResolveShape(Size, rows, columns);
            return _computationUnit.CreateMatrix(Segment, shape[0], shape[1]);
        }

        public T Map(Func<float, float> mutator)
        {
            var ret = _computationUnit.MapParallel(Segment, mutator);
            return Create(ret);
        }

        public void MapInPlace(Func<float, float> mutator)
        {
            var ret = _computationUnit.MapParallel(Segment, mutator);
            try {
                Segment.CopyFrom(ret.GetSpan());
            }
            finally {
                ret.Release();
            }
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

        public void Clear() => Segment.Clear();
        public T Add(ITensor2 tensor)                                                        => Create(_computationUnit.Add(Segment, tensor.Segment));
        public T Add(ITensor2 tensor, float coefficient1, float coefficient2)                => Create(_computationUnit.Add(Segment, tensor.Segment, coefficient1, coefficient2));
        public T Add(float scalar)                                                           => Create(_computationUnit.Add(Segment, scalar));
        public void AddInPlace(ITensor2 tensor)                                              => _computationUnit.AddInPlace(Segment, tensor.Segment);
        public void AddInPlace(ITensor2 tensor, float coefficient1, float coefficient2)      => _computationUnit.AddInPlace(Segment, tensor.Segment, coefficient1, coefficient2);
        public void AddInPlace(float scalar)                                                 => _computationUnit.AddInPlace(Segment, scalar);
        public void MultiplyInPlace(float scalar)                                            => _computationUnit.MultiplyInPlace(Segment, scalar);
        public T Multiply(float scalar)                                                      => Create(_computationUnit.Multiply(Segment, scalar));
        public T Subtract(ITensor2 tensor)                                                   => Create(_computationUnit.Subtract(Segment, tensor.Segment));
        public T Subtract(ITensor2 tensor, float coefficient1, float coefficient2)           => Create(_computationUnit.Subtract(Segment, tensor.Segment, coefficient1, coefficient2));
        public void SubtractInPlace(ITensor2 tensor)                                         => _computationUnit.SubtractInPlace(Segment, tensor.Segment);
        public void SubtractInPlace(ITensor2 tensor, float coefficient1, float coefficient2) => _computationUnit.SubtractInPlace(Segment, tensor.Segment, coefficient1, coefficient2);
        public T PointwiseMultiply(ITensor2 tensor)                                          => Create(_computationUnit.PointwiseMultiply(Segment, tensor.Segment));
        public void PointwiseMultiplyInPlace(ITensor2 tensor)                                => _computationUnit.PointwiseMultiplyInPlace(Segment, tensor.Segment);
        public T PointwiseDivide(ITensor2 tensor)                                            => Create(_computationUnit.PointwiseDivide(Segment, tensor.Segment));
        public void PointwiseDivideInPlace(ITensor2 tensor)                                  => _computationUnit.PointwiseDivideInPlace(Segment, tensor.Segment);
        public float DotProduct(ITensor2 tensor)                                             => _computationUnit.DotProduct(Segment, tensor.Segment);
        public T Sqrt()                                                                      => Create(_computationUnit.Sqrt(Segment));
        public uint? Search(float value)                                                     => _computationUnit.Search(Segment, value);
        public void ConstrainInPlace(float? minValue, float? maxValue)                       => _computationUnit.ConstrainInPlace(Segment, minValue, maxValue);
        public float Average()                                                               => _computationUnit.Average(Segment);
        public float L1Norm()                                                                => _computationUnit.L1Norm(Segment);
        public float L2Norm()                                                                => _computationUnit.L2Norm(Segment);
        public (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues()     => _computationUnit.GetMinAndMaxValues(Segment);
        public bool IsEntirelyFinite()                                                       => _computationUnit.IsEntirelyFinite(Segment);
        public T Reverse()                                                                   => Create(_computationUnit.Reverse(Segment));
        public IEnumerable<T> Split(uint blockCount)                                         => _computationUnit.Split(Segment, blockCount).Select(Create);
        public float CosineDistance(ITensor2 other)                                          => _computationUnit.CosineDistance(Segment, other.Segment);
        public float EuclideanDistance(ITensor2 other)                                       => _computationUnit.EuclideanDistance(Segment, other.Segment);
        public float MeanSquaredDistance(ITensor2 other)                                     => _computationUnit.MeanSquaredDistance(Segment, other.Segment);
        public float SquaredEuclideanDistance(ITensor2 other)                                => _computationUnit.SquaredEuclideanDistance(Segment, other.Segment);
        public float ManhattanDistance(ITensor2 other)                                       => _computationUnit.ManhattanDistance(Segment, other.Segment);
        public T Abs()                                                                       => Create(_computationUnit.Abs(Segment));
        public T Log()                                                                       => Create(_computationUnit.Log(Segment));
        public T Exp()                                                                       => Create(_computationUnit.Exp(Segment));
        public T Squared()                                                                   => Create(_computationUnit.Squared(Segment));
        public float StdDev(float? mean)                                                     => _computationUnit.StdDev(Segment, mean);
        public T Sigmoid()                                                                   => Create(_computationUnit.Sigmoid(Segment));
        public T SigmoidDerivative()                                                         => Create(_computationUnit.SigmoidDerivative(Segment));
        public T Tanh()                                                                      => Create(_computationUnit.Tanh(Segment));
        public T TanhDerivative()                                                            => Create(_computationUnit.TanhDerivative(Segment));
        public T Relu()                                                                      => Create(_computationUnit.Relu(Segment));
        public T ReluDerivative()                                                            => Create(_computationUnit.ReluDerivative(Segment));
        public T LeakyRelu()                                                                 => Create(_computationUnit.LeakyRelu(Segment));
        public T LeakyReluDerivative()                                                       => Create(_computationUnit.LeakyReluDerivative(Segment));
        public T Softmax()                                                                   => Create(_computationUnit.Softmax(Segment));
        public IMatrix SoftmaxDerivative()                                                   => _computationUnit.SoftmaxDerivative(Segment);
        public T Pow(float power)                                                            => Create(_computationUnit.Pow(Segment, power));
        public void RoundInPlace(float lower, float upper, float? mid)                       => _computationUnit.RoundInPlace(Segment, lower, upper, mid);
        public T CherryPick(uint[] indices)                                                  => Create(_computationUnit.CherryPickIndices(Segment, indices));
    }
}
