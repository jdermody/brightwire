using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData2
{
    public class ComputationUnit : IDisposable
    {
        readonly HashSet<IDisposable> _disposable = new();

        public ComputationUnit(
            BrightDataContext2 context
        )
        {
            Context = context;
        }

        public BrightDataContext2 Context { get; }

        public void Dispose()
        {
            foreach(var item in _disposable)
                item.Dispose();
        }

        internal bool AddToScope(IDisposable obj) => _disposable.Add(obj);
        internal bool RemoveFromScope(IDisposable obj) => _disposable.Remove(obj);
        public IDisposableTensorSegment CreateSegment(uint size) => new TensorSegment2(MemoryOwner<float>.Allocate((int)size));

        // vector creation
        public virtual IVector CreateVector(ITensorSegment2 data) => new Vector2(data, this);
        public IVector CreateVector(uint size) => CreateVector(CreateSegment(size));
        public IVector CreateVector(uint size, Func<uint, float> initializer)
        {
            var segment = CreateSegment(size);
            var array = segment.GetArrayForLocalUseOnly()!;
            for (uint i = 0, len = (uint)array.Length; i < len; i++)
                array[i] = initializer(i);
            return CreateVector(segment);
        }


        // matrix creation
        public virtual IMatrix CreateMatrix(ITensorSegment2 data, uint rowCount, uint columnCount) => new Matrix2(data, rowCount, columnCount, this);
        public IMatrix CreateMatrix(uint rowCount, uint columnCount) => CreateMatrix(CreateSegment(rowCount * columnCount), rowCount, columnCount);
        public IMatrix CreateMatrix(uint rowCount, uint columnCount, Func<uint, uint, float> initializer)
        {
            var segment = CreateSegment(rowCount * columnCount);
            var array = segment.GetArrayForLocalUseOnly()!;
            for (uint i = 0, len = (uint)array.Length; i < len; i++)
                array[i] = initializer(i / columnCount, i % columnCount);
            return CreateMatrix(CreateSegment(rowCount * columnCount), rowCount, columnCount);
        }

        protected static uint GetSize(ITensorSegment2 tensor, ITensorSegment2 tensor2)
        {
            if (tensor.Size != tensor2.Size)
                throw new Exception("Expected tensors to have same size");
            return tensor.Size;
        }

        public virtual ITensorSegment2 Add(ITensorSegment2 tensor, ITensorSegment2 tensor2) => tensor.Add(tensor2);
        public virtual ITensorSegment2 Add(ITensorSegment2 tensor, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => tensor.Add(tensor2, coefficient1, coefficient2);
        public virtual ITensorSegment2 Add(ITensorSegment2 tensor, float scalar) => tensor.Add(scalar);
        public virtual void AddInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.AddInPlace(other);
        public virtual void AddInPlace(ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => target.AddInPlace(other, coefficient1, coefficient2);
        public virtual void AddInPlace(ITensorSegment2 target, float scalar) => target.AddInPlace(scalar);
        public virtual void MultiplyInPlace(ITensorSegment2 target, float scalar) => target.MultiplyInPlace(scalar);
        public virtual ITensorSegment2 Multiply(ITensorSegment2 target, float scalar) => target.Multiply(scalar);
        public virtual ITensorSegment2 Subtract(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => tensor1.Subtract(tensor2);
        public virtual ITensorSegment2 Subtract(ITensorSegment2 tensor1, ITensorSegment2 tensor2, float coefficient1, float coefficient2) => tensor1.Subtract(tensor2, coefficient1, coefficient2);
        public virtual void SubtractInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.SubtractInPlace(other);
        public virtual void SubtractInPlace(ITensorSegment2 target, ITensorSegment2 other, float coefficient1, float coefficient2) => target.SubtractInPlace(other, coefficient1, coefficient2);
        public virtual ITensorSegment2 PointwiseMultiply(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => tensor1.PointwiseMultiply(tensor2);
        public virtual void PointwiseMultiplyInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.PointwiseMultiplyInPlace(other);
        public virtual ITensorSegment2 PointwiseDivide(ITensorSegment2 tensor1, ITensorSegment2 tensor2) => tensor1.PointwiseDivide(tensor2);
        public virtual void PointwiseDivideInPlace(ITensorSegment2 target, ITensorSegment2 other) => target.PointwiseDivideInPlace(other);
        public virtual float DotProduct(ITensorSegment2 tensor, ITensorSegment2 tensor2) => tensor.DotProduct(tensor2);
        public virtual ITensorSegment2 Sqrt(ITensorSegment2 tensor) => tensor.Sqrt();
        public virtual uint? Search(ITensorSegment2 segment, float value) => segment.Search(value);
        public virtual void ConstrainInPlace(ITensorSegment2 segment, float? minValue, float? maxValue) => segment.ConstrainInPlace(minValue, maxValue);
        public virtual float Average(ITensorSegment2 segment) => segment.Average();
        public virtual float L1Norm(ITensorSegment2 segment) => segment.L1Norm();
        public virtual float L2Norm(ITensorSegment2 segment) => segment.L2Norm();
        public virtual (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues(ITensorSegment2 segment) => segment.GetMinAndMaxValues();
        public virtual bool IsEntirelyFinite(ITensorSegment2 segment) => segment.IsEntirelyFinite();
        public virtual ITensorSegment2 Reverse(ITensorSegment2 segment) => segment.Reverse();
        public virtual IEnumerable<ITensorSegment2> Split(ITensorSegment2 segment, uint blockCount) => segment.Split(blockCount);
        public virtual float CosineDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.CosineDistance(other);
        public virtual float EuclideanDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.EuclideanDistance(other);
        public virtual float MeanSquaredDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.MeanSquaredDistance(other);
        public virtual float SquaredEuclideanDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.SquaredEuclideanDistance(other);
        public virtual float ManhattanDistance(ITensorSegment2 tensor, ITensorSegment2 other) => tensor.ManhattanDistance(other);
        public virtual ITensorSegment2 Abs(ITensorSegment2 tensor) => tensor.Abs();
        public virtual ITensorSegment2 Log(ITensorSegment2 tensor) => tensor.Log();
        public virtual ITensorSegment2 Exp(ITensorSegment2 tensor) => tensor.Exp();
        public virtual ITensorSegment2 Squared(ITensorSegment2 tensor) => tensor.Squared();
        public virtual float StdDev(ITensorSegment2 tensor, float? mean) => tensor.StdDev(mean);
        public virtual ITensorSegment2 Sigmoid(ITensorSegment2 tensor) => tensor.Sigmoid();
        public virtual ITensorSegment2 SigmoidDerivative(ITensorSegment2 tensor) => tensor.SigmoidDerivative();
        public virtual ITensorSegment2 Tanh(ITensorSegment2 tensor) => tensor.Tanh();
        public virtual ITensorSegment2 TanhDerivative(ITensorSegment2 tensor) => tensor.TanhDerivative();
        public virtual ITensorSegment2 Relu(ITensorSegment2 tensor) => tensor.Relu();
        public virtual ITensorSegment2 ReluDerivative(ITensorSegment2 tensor) => tensor.ReluDerivative();
        public virtual ITensorSegment2 LeakyRelu(ITensorSegment2 tensor) => tensor.LeakyRelu();
        public virtual ITensorSegment2 LeakyReluDerivative(ITensorSegment2 tensor) => tensor.LeakyReluDerivative();
        public virtual ITensorSegment2 Softmax(ITensorSegment2 tensor) => tensor.Softmax();
        public virtual IMatrix SoftmaxDerivative(ITensorSegment2 tensor) => tensor.SoftmaxDerivative(this);
        public virtual ITensorSegment2 Pow(ITensorSegment2 tensor, float power) => tensor.Pow(power);
        public virtual void RoundInPlace(ITensorSegment2 tensor, float lower, float upper, float? mid) => tensor.RoundInPlace(lower, upper, mid);
    }
}
