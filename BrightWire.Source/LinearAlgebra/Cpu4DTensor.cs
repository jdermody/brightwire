using MathNet.Numerics.LinearAlgebra.Single;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// 4D Tensor that uses the CPU based math.net numerics library
    /// </summary>
    class Cpu4DTensor : I4DTensor
    {
        readonly Cpu3DTensor[] _data;

	    public Cpu4DTensor(int rows, int columns, int depth, int count)
        {
            RowCount = rows;
            ColumnCount = columns;
            Depth = depth;
            _data = Enumerable.Range(0, count).Select(i => new Cpu3DTensor(rows, columns, depth)).ToArray();
        }

        public Cpu4DTensor(IReadOnlyList<I3DTensor> tensorList)
        {
            var first = tensorList.First();
            Debug.Assert(tensorList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount && m.Depth == first.Depth));
            RowCount = first.RowCount;
            ColumnCount = first.ColumnCount;
            Depth = first.Depth;
            _data = tensorList.Cast<Cpu3DTensor>().ToArray();
        }

        public Cpu4DTensor(IReadOnlyList<IReadOnlyList<IMatrix>> tensorList)
        {
            var first = tensorList.First();
            var firstMatrix = first.First();
            RowCount = firstMatrix.RowCount;
            ColumnCount = firstMatrix.ColumnCount;
            Depth = first.Count;
            _data = tensorList.Select(d => new Cpu3DTensor(d)).ToArray();
        }

        public int RowCount { get; }
	    public int ColumnCount { get; }
	    public int Depth { get; }
	    public int Count => _data.Length;

        public void Dispose()
        {
            // nop
        }

        public I3DTensor GetTensorAt(int index)
        {
            return _data[index];
        }

        public IReadOnlyList<IIndexable3DTensor> AsIndexable() => _data;

        public I4DTensor AddPadding(int padding)
        {
            var ret = new List<I3DTensor>();
            foreach (var item in _data)
                ret.Add(item.AddPadding(padding));
            return new Cpu4DTensor(ret);
        }

        public I4DTensor RemovePadding(int padding)
        {
            var ret = new List<I3DTensor>();
            foreach (var item in _data)
                ret.Add(item.RemovePadding(padding));
            return new Cpu4DTensor(ret);
        }

        public (I4DTensor Result, IReadOnlyList<IReadOnlyList<(object X, object Y)>> Index) MaxPool(int filterWidth, int filterHeight, int stride, bool calculateIndex)
        {
            List<IReadOnlyList<(object X, object Y)>> indexList = calculateIndex ? new List<IReadOnlyList<(object X, object Y)>>() : null;
            var ret = new List<I3DTensor>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).MaxPool(filterWidth, filterHeight, stride, calculateIndex);
                if (calculateIndex)
                    indexList.Add(result.Index);
                ret.Add(result.Result);
            }
            return (new Cpu4DTensor(ret), indexList);
        }

        public I4DTensor ReverseMaxPool(int rows, int columns, IReadOnlyList<IReadOnlyList<(object X, object Y)>> indexList)
        {
            var ret = new List<I3DTensor>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseMaxPool(rows, columns, indexList?[i]);
                ret.Add(result);
            }
            return new Cpu4DTensor(ret);
        }

        public I3DTensor Im2Col(int filterWidth, int filterHeight, int stride)
        {
            var ret = new List<IMatrix>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).Im2Col(filterWidth, filterHeight, stride);
                ret.Add(result);
            }
            return new Cpu3DTensor(ret);
        }

        public I3DTensor ReverseIm2Col(IReadOnlyList<IReadOnlyList<IVector>> filter, int inputHeight, int inputWidth, int padding, int filterWidth, int filterHeight, int stride)
        {
            var ret = new List<IMatrix>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseIm2Col(filter, inputHeight, inputWidth, padding, filterWidth, filterHeight, stride);
                ret.Add(result);
            }
            return new Cpu3DTensor(ret);
        }

        public IMatrix ConvertToMatrix()
        {
            var rows = _data.Select(t => t.ConvertToVector().AsIndexable()).ToList();
            var first = rows.First();
            return new CpuMatrix(DenseMatrix.Create(first.Count, Count, (i, j) => rows[j][i]));
        }

        public IVector ColumnSums()
        {
            IVector ret = null;
            for (var i = 0; i < Count; i++) {
                var tensorAsMatrix = GetTensorAt(i).ConvertToMatrix();
                var columnSums = tensorAsMatrix.ColumnSums();
                if (ret == null)
                    ret = columnSums;
                else
                    ret.AddInPlace(columnSums);
            }
            return ret;
        }
    }
}
