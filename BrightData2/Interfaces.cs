using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightData2
{
    public interface ITensor2 : IDisposable
    {
        BrightDataContext2 Context { get; }
        ITensorSegment2 Segment { get; }
        IVector AsVector();
    }

    public interface ITensor2<out T> : ITensor2 
        where T: ITensor2
    {
        T Add(ITensor2 tensor);
        T Add(ITensor2 tensor, float coefficient1, float coefficient2);
        T Add(float scalar);
        void AddInPlace(ITensor2 tensor);
        void AddInPlace(ITensor2 tensor, float coefficient1, float coefficient2);
        void AddInPlace(float scalar);
        void MultiplyInPlace(float scalar);
        T Multiply(float scalar);
        T Subtract(ITensor2 tensor);
        T Subtract(ITensor2 tensor, float coefficient1, float coefficient2);
        void SubtractInPlace(ITensor2 tensor);
        void SubtractInPlace(ITensor2 tensor, float coefficient1, float coefficient2);
        T PointwiseMultiply(ITensor2 tensor);
        void PointwiseMultiplyInPlace(ITensor2 tensor);
        T PointwiseDivide(ITensor2 tensor);
        void PointwiseDivideInPlace(ITensor2 tensor);
        float DotProduct(ITensor2 tensor);
        T Sqrt();
        uint? Search(float value);
        void ConstrainInPlace(float? minValue, float? maxValue);
        float Average();
        float L1Norm();
        float L2Norm();
        (float Min, float Max, uint MinIndex, uint MaxIndex) GetMinAndMaxValues();
        bool IsEntirelyFinite();
        T Reverse();
        IEnumerable<T> Split(uint blockCount);
        float CosineDistance(ITensor2 other);
        float EuclideanDistance(ITensor2 other);
        float MeanSquaredDistance(ITensor2 other);
        float SquaredEuclideanDistance(ITensor2 other);
        float ManhattanDistance(ITensor2 other);
        T Abs();
        T Log();
        T Exp();
        T Squared();
        float StdDev(float? mean);
        T Sigmoid();
        T SigmoidDerivative();
        T Tanh();
        T TanhDerivative();
        T Relu();
        T ReluDerivative();
        T LeakyRelu();
        T LeakyReluDerivative();
        T Softmax();
        IMatrix SoftmaxDerivative();
        T Pow(float power);
        void RoundInPlace(float lower, float upper, float? mid);
    }

    public interface IVector : ITensor2<IVector>
    {
        public uint Size { get; }
        float this[int index] { get; set; }
        float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
    }

    public interface IMatrix : ITensor2<IMatrix>
    {
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint Size { get; }
        float this[int rowY, int columnX] { get; set; }
        float this[uint rowY, uint columnX] { get; set; }
        float this[long rowY, long columnX] { get; set; }
        float this[ulong rowY, ulong columnX] { get; set; }
        IDisposableTensorSegmentWrapper Row(uint index);
        IDisposableTensorSegmentWrapper Column(uint index);
        IDisposableTensorSegmentWrapper[] Rows();
        IDisposableTensorSegmentWrapper[] Columns();
    }

    public interface ICountReferences
    {
        int AddRef();
        int Release();
        bool IsValid { get; }
    }

    public interface ITensorSegment2 : ICountReferences
    {
        uint Size { get; }
        string SegmentType { get; }
        float this[int index] { get; set; }
        float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
        IEnumerable<float> Values { get; }
        float[]? GetArrayForLocalUseOnly();
        float[] ToNewArray();
        void CopyFrom(Span<float> span);
    }

    public interface IDisposableTensorSegment : ITensorSegment2, IDisposable
    {
    }

    public interface IDisposableTensorSegmentWrapper : IDisposableTensorSegment
    {
    }
}
