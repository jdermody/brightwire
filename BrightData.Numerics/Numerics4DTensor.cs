using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    /// <summary>
    /// 4D Tensor that uses the CPU based math.net numerics library
    /// </summary>
    internal class Numerics4DTensor : IIndexable4DFloatTensor
    {
        public IBrightDataContext Context { get; }
        readonly NumericsMatrix _data;
	    readonly uint _rows, _columns, _depth, _count;

	    public Numerics4DTensor(IBrightDataContext context, uint rows, uint columns, uint depth, uint count)
	    {
            Context = context;
            _rows = rows;
			_columns = columns;
		    _depth = depth;
		    _count = count;
		    _data = new NumericsMatrix(context, DenseMatrix.Build.Dense((int)(_rows * _count * _depth), (int)_count));
	    }

        public Numerics4DTensor(IBrightDataContext context, IIndexable3DFloatTensor[] tensorList)
        {
            Context = context;
            var first = tensorList.First();
            Debug.Assert(tensorList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount && m.Depth == first.Depth));
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _depth = first.Depth;
	        _count = (uint)tensorList.Length;

	        uint offset = 0;
	        var rowSize = _rows * _columns * _depth;
	        var data = new float[rowSize * _count];
	        foreach (var matrix in tensorList) {
		        Array.Copy(matrix.GetInternalArray(), 0, data, offset, rowSize);
		        offset += rowSize;
	        }

	        _data = new NumericsMatrix(context, DenseMatrix.Build.Dense((int)rowSize, (int)_count, data));
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => Context.LinearAlgebraProvider;
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
	    public IIndexable3DFloatTensor[] Tensors => _data.Columns.Select(c => c.ReshapeAs3DTensor(_rows, _columns, _depth).AsIndexable()).ToArray();

        public IIndexable4DFloatTensor AsIndexable() => this;

        public I4DFloatTensor AddPadding(uint padding)
        {
            var ret = Tensors
                .Select(t => t.AddPadding(padding).AsIndexable())
                .ToArray();
            return new Numerics4DTensor(Context, ret);
        }

        public I4DFloatTensor RemovePadding(uint padding)
        {
            var ret = Tensors
                .Select(t => t.RemovePadding(padding).AsIndexable())
                .ToArray();
            return new Numerics4DTensor(Context, ret);
        }

        public (I4DFloatTensor Result, I4DFloatTensor Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            IIndexable3DFloatTensor[] indexList = saveIndices 
                ? new IIndexable3DFloatTensor[Count]
                : null;
            var ret = new IIndexable3DFloatTensor[Count];

            for (uint i = 0; i < Count; i++) {
                var (result, indices) = GetTensorAt(i).MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
                ret[i] = result.AsIndexable();
                if(indexList != null)
                    indexList[i] = indices.AsIndexable();
            }
            return (new Numerics4DTensor(Context, ret), saveIndices ? new Numerics4DTensor(Context, indexList) : null);
        }

        public I4DFloatTensor ReverseMaxPool(I4DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new IIndexable3DFloatTensor[Count];
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseMaxPool(indices.GetTensorAt(i), outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
                ret[i] = result.AsIndexable();
            }
            return new Numerics4DTensor(Context, ret);
        }

        public I3DFloatTensor Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new IIndexableFloatMatrix[Count];
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).Im2Col(filterWidth, filterHeight, xStride, yStride);
                ret[i] = result.AsIndexable();
            }
            return new Numerics3DTensor(Context, ret);
        }

        public I4DFloatTensor ReverseIm2Col(IFloatMatrix filters, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new IIndexable3DFloatTensor[Count];
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseIm2Col(filters, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
                ret[i] = result.AsIndexable();
            }
            return new Numerics4DTensor(Context, ret);
        }

		public IFloatMatrix ReshapeAsMatrix()
		{
			return _data;
		}

	    public Tensor3D<float>[] Data {
		    get => Tensors.Select(t => t.Data).ToArray();
		    set {
			    var count = value.Length;
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
                                    this[i, j, k, (uint)z] = row.Segment[j];
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
                using var writer = XmlWriter.Create(new StringWriter(ret), settings);
                writer.WriteStartElement("tensor-4d");
                foreach(var tensor in Tensors) {
                    writer.WriteRaw(tensor.AsXml);
                }
                writer.WriteEndElement();
                return ret.ToString();
		    }
	    }
    }
}
