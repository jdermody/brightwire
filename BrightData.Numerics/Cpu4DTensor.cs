using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightWire.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    /// <summary>
    /// 4D Tensor that uses the CPU based math.net numerics library
    /// </summary>
    class Cpu4DTensor : IIndexable4DFloatTensor
    {
        readonly CpuMatrix _data;
	    readonly uint _rows, _columns, _depth, _count;

	    public Cpu4DTensor(uint rows, uint columns, uint depth, uint count)
	    {
		    _rows = rows;
			_columns = columns;
		    _depth = depth;
		    _count = count;
		    _data = new CpuMatrix(DenseMatrix.Build.Dense((int)(_rows * _count * _depth), (int)_count));
	    }

        public Cpu4DTensor(IReadOnlyList<IIndexable3DFloatTensor> tensorList)
        {
            var first = tensorList.First();
            Debug.Assert(tensorList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount && m.Depth == first.Depth));
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
	        _count = (uint)tensorList.Count;

	        uint offset = 0;
	        var rowSize = _rows * _columns * _depth;
	        var data = new float[rowSize * _count];
	        foreach (var matrix in tensorList) {
		        Array.Copy(matrix.GetInternalArray(), 0, data, offset, rowSize);
		        offset += rowSize;
	        }

	        _data = new CpuMatrix(DenseMatrix.Build.Dense((int)rowSize, (int)_count, data));
        }

        public uint RowCount => _rows;
	    public uint ColumnCount => _columns;
	    public uint Depth => _depth;
	    public uint Count => _count;
	    public float[] GetInternalArray() => _data.GetInternalArray();

        public void Dispose()
        {
            // nop
        }

	    public override string ToString()
	    {
		    return $"4D tensor, rows:{RowCount} columns:{ColumnCount} depth:{Depth} count:{Count}";
	    }

	    public I3DFloatTensor GetTensorAt(uint index) => _data.Column(index).ReshapeAs3DTensor(_rows, _columns, _depth);
	    public IReadOnlyList<IIndexable3DFloatTensor> Tensors => _data.Columns.Select(c => c.ReshapeAs3DTensor(_rows, _columns, _depth).AsIndexable()).ToList();

        public IIndexable4DFloatTensor AsIndexable() => this;

        public I4DFloatTensor AddPadding(uint padding)
        {
            var ret = new List<IIndexable3DFloatTensor>();
            foreach (var item in Tensors)
                ret.Add(item.AddPadding(padding).AsIndexable());
            return new Cpu4DTensor(ret);
        }

        public I4DFloatTensor RemovePadding(uint padding)
        {
            var ret = new List<IIndexable3DFloatTensor>();
            foreach (var item in Tensors)
                ret.Add(item.RemovePadding(padding).AsIndexable());
            return new Cpu4DTensor(ret);
        }

        public (I4DFloatTensor Result, I4DFloatTensor Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            List<IIndexable3DFloatTensor> indexList = saveIndices ? new List<IIndexable3DFloatTensor>() : null;
            var ret = new List<IIndexable3DFloatTensor>();
            for (uint i = 0; i < Count; i++) {
                var (result, indices) = GetTensorAt(i).MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
                indexList?.Add(indices.AsIndexable());
                ret.Add(result.AsIndexable());
            }
            return (new Cpu4DTensor(ret), saveIndices ? new Cpu4DTensor(indexList) : null);
        }

        public I4DFloatTensor ReverseMaxPool(I4DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new List<IIndexable3DFloatTensor>();
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseMaxPool(indices.GetTensorAt(i), outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
                ret.Add(result.AsIndexable());
            }
            return new Cpu4DTensor(ret);
        }

        public I3DFloatTensor Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new List<IIndexableFloatMatrix>();
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).Im2Col(filterWidth, filterHeight, xStride, yStride);
                ret.Add(result.AsIndexable());
            }
            return new Cpu3DTensor(ret);
        }

        public I4DFloatTensor ReverseIm2Col(IFloatMatrix filters, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new List<IIndexable3DFloatTensor>();
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseIm2Col(filters, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
                ret.Add(result.AsIndexable());
            }
            return new Cpu4DTensor(ret);
        }

		public IFloatMatrix ReshapeAsMatrix()
		{
			return _data;
		}

	    public IReadOnlyList<BrightData.Tensor3D<float>> Data {
		    get => Tensors.Select(t => t.Data).ToList();
		    set {
			    var count = value.Count;
			    for (var z = 0; z < count && z < _count; z++) {
				    var tensor = value[z];
				    if (tensor != null) {
					    var matrixList = tensor.Matrices.ToArray();
					    var matrixCount = matrixList.Length;
					    for (uint k = 0; k < matrixCount && k < _depth; k++) {
						    var matrix = matrixList[k];
                            for (uint i = 0, len = matrix.RowCount; i < len; i++) {
                                var row = matrix.Row(i);
                                for (uint j = 0, len2 = row.Size; j < len2; j++)
                                    this[i, j, k, (uint)z] = row.Data[j];
                            }
                        }
				    }
			    }
		    }
	    }

	    public IFloatVector ReshapeAsVector()
	    {
		    return _data.ReshapeAsVector();
	    }

        public IFloatVector ColumnSums()
        {
            IFloatVector ret = null;
            for (uint i = 0; i < Count; i++) {
                var tensorAsMatrix = GetTensorAt(i).ReshapeAsMatrix();
                var columnSums = tensorAsMatrix.ColumnSums();
                if (ret == null)
                    ret = columnSums;
                else
                    ret.AddInPlace(columnSums);
            }
            return ret;
        }

	    public float this[uint row, uint column, uint depth, uint index] {
		    get => _data[_RowIndex(row, column, depth), index];
		    set => _data[_RowIndex(row, column, depth), index] = value;
	    }

	    uint _RowIndex(uint row, uint column, uint depth) => depth * _depth + (column * _rows + row);

	    public string AsXml
	    {
		    get
		    {
			    var ret = new StringBuilder();
			    var settings = new XmlWriterSettings {
				    OmitXmlDeclaration = true
			    };
			    using (var writer = XmlWriter.Create(new StringWriter(ret), settings)) {
				    writer.WriteStartElement("tensor-4d");
				    foreach(var tensor in Tensors) {
					    writer.WriteRaw(tensor.AsXml);
				    }
				    writer.WriteEndElement();
			    }
			    return ret.ToString();
		    }
	    }
    }
}
