using System;
using System.Collections.Generic;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace BrightData.LinearAlegbra2
{
    public interface ITensor2 : IDisposable
    {
        BrightDataContext2 Context { get; }
        ITensorSegment2 Segment { get; }
        IVector Reshape();
        IMatrix Reshape(uint? rows, uint? columns);
        void Clear();
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
        T CherryPick(uint[] indices);
        T Map(Func<float, float> mutator);
        void MapInPlace(Func<float, float> mutator);
    }

    public interface IVector : ITensor2<IVector>
    {
        public uint Size { get; }
        float this[int index] { get; set; }
        float this[uint index] { get; set; }
        float this[long index] { get; set; }
        float this[ulong index] { get; set; }
        IVector MapIndexed(Func<uint, float, float> mutator);
        void MapIndexedInPlace(Func<uint, float, float> mutator);
    }

    public interface IMatrix : ITensor2<IMatrix>
    {
        uint RowCount { get; }
        uint ColumnCount { get; }
        float this[int rowY, int columnX] { get; set; }
        float this[uint rowY, uint columnX] { get; set; }
        float this[long rowY, long columnX] { get; set; }
        float this[ulong rowY, ulong columnX] { get; set; }
        IDisposableTensorSegmentWrapper Row(uint index);
        IDisposableTensorSegmentWrapper Column(uint index);
        IDisposableTensorSegmentWrapper[] Rows();
        IDisposableTensorSegmentWrapper[] Columns();
        MemoryOwner<float> ToNewColumnMajor();
        IMatrix Transpose();
        IMatrix Multiply(IMatrix other);
        IMatrix TransposeAndMultiply(IMatrix other);
        IMatrix TransposeThisAndMultiply(IMatrix other);
        IVector GetDiagonal();
        IVector RowSums();
        IVector ColumnSums();
        IMatrix Multiply(IVector vector);
        (IMatrix Left, IMatrix Right) SplitAtColumn(uint columnIndex);
        (IMatrix Top, IMatrix Bottom) SplitAtRow(uint rowIndex);
        IMatrix ConcatColumns(IMatrix bottom);
        IMatrix ConcatRows(IMatrix right);
        IMatrix MapIndexed(Func<uint, uint, float, float> mutator);
        void MapIndexedInPlace(Func<uint, uint, float, float> mutator);
    }

    public interface ITensor3D : ITensor2<ITensor3D>
    {
        uint Depth { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint MatrixSize { get; }
        float this[int depth, int rowY, int columnX] { get; set; }
        float this[uint depth, uint rowY, uint columnX] { get; set; }
        float this[long depth, long rowY, long columnX] { get; set; }
        float this[ulong depth, ulong rowY, ulong columnX] { get; set; }
        IMatrix Matrix(uint index);
        MemoryOwner<float> ToNewColumnMajor();
        ITensor3D AddPadding(uint padding);
        ITensor3D RemovePadding(uint padding);
        IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        (ITensor3D Result, ITensor3D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);
        ITensor3D ReverseMaxPool(ITensor3D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor3D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        IMatrix CombineDepthSlices();
        ITensor3D Multiply(IMatrix matrix);
        void AddToEachRow(IVector vector);
        ITensor3D TransposeThisAndMultiply(ITensor4D other);
    }

    public interface ITensor4D : ITensor2<ITensor4D>
    {
        uint Count { get; }
        uint Depth { get; }
        uint RowCount { get; }
        uint ColumnCount { get; }
        uint MatrixSize { get; }
        uint TensorSize { get; }
        float this[int count, int depth, int rowY, int columnX] { get; set; }
        float this[uint count, uint depth, uint rowY, uint columnX] { get; set; }
        float this[long count, long depth, long rowY, long columnX] { get; set; }
        float this[ulong count, ulong depth, ulong rowY, ulong columnX] { get; set; }
        ITensor3D Tensor(uint index);
        ITensor4D AddPadding(uint padding);
        ITensor4D RemovePadding(uint padding);
        (ITensor4D Result, ITensor4D? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices);
        ITensor4D ReverseMaxPool(ITensor4D indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor3D Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride);
        ITensor4D ReverseIm2Col(IMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride);
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
        void Clear();
    }

    public interface IDisposableTensorSegment : ITensorSegment2, IDisposable
    {
    }

    public interface IDisposableTensorSegmentWrapper : IDisposableTensorSegment
    {
    }
}
