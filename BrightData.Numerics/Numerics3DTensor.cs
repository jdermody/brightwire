using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData.FloatTensors;
using BrightData.Helper;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    /// <summary>
    /// 3D Tensor that uses the CPU based math.net numerics library
    /// </summary>
    class Numerics3DTensor : IIndexable3DFloatTensor
    {
        public IBrightDataContext Context { get; }
        readonly NumericsMatrix _data;
        readonly uint _rows, _columns, _depth;

        public Numerics3DTensor(IBrightDataContext context, uint rows, uint columns, uint depth)
        {
            Context = context;
            _rows = rows;
            _columns = columns;
	        _depth = depth;
	        _data = new NumericsMatrix(context, DenseMatrix.Build.Dense((int)(rows * columns), (int)depth));
        }

        public Numerics3DTensor(IBrightDataContext context, IReadOnlyList<IIndexableFloatMatrix> matrixList)
        {
            Context = context;
            var first = matrixList.First();
            Debug.Assert(matrixList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount));
            _rows = first.RowCount;
            _columns = first.ColumnCount;
			_depth = (uint)matrixList.Count;

	        uint offset = 0;
	        var rowSize = _rows * _columns;
	        var data = new float[rowSize * _depth];
	        foreach (var matrix in matrixList) {
		        Array.Copy(matrix.GetInternalArray(), 0, data, offset, rowSize);
		        offset += rowSize;
	        }

	        _data = new NumericsMatrix(context, DenseMatrix.Build.Dense((int)rowSize, (int)_depth, data));
        }

        public void Dispose()
        {
            // nop
        }

		public override string ToString()
		{
			return $"3D tensor, rows:{_rows} columns:{_columns} depth:{_depth}";
		}

		public float this[uint row, uint column, uint depth]
        {
            get => _data[_RowIndex(row, column), depth];
	        set => _data[_RowIndex(row, column), depth] = value;
        }

	    uint _RowIndex(uint row, uint column) => column * _rows + row;

        public BrightData.Tensor3D<float> Data
        {
            get
            {
                return Float3DTensor.Create(Context, Matrix.Select(m => m.Data).ToArray());
            }
            set
            {
                var matrixList = value.Matrices.ToArray();
                var matrixCount = matrixList.Length;
                for (uint k = 0; k < matrixCount && k < _depth; k++) {
                    var matrix = matrixList[k];
                    for (uint i = 0, len = matrix.RowCount; i < len; i++) {
                        var row = matrix.Row(i);
                        for (uint j = 0, len2 = row.Size; j < len2; j++)
                            this[i, j, k] = row.Segment[j];
                    }
                }
            }
        }

        public IReadOnlyList<IIndexableFloatMatrix> Matrix => _data.Columns.Select(c => c.ReshapeAsMatrix(_rows, _columns).AsIndexable()).ToList();
	    public uint Depth => _depth;
        public uint RowCount => _rows;
        public uint ColumnCount => _columns;
        public IFloatMatrix GetMatrixAt(uint depth) => _data.Column(depth).ReshapeAsMatrix(_rows, _columns);
        public IIndexable3DFloatTensor AsIndexable() => this;
	    public float[] GetInternalArray() => _data.GetInternalArray();

        public I3DFloatTensor AddPadding(uint padding)
        {
            var newRows = _rows + padding * 2;
            var newColumns = _columns + padding * 2;
            var ret = new Numerics3DTensor(Context, newRows, newColumns, Depth);

            for (uint k = 0; k < Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        if (i < padding || j < padding)
                            continue;
                        else if (i >= newRows - padding || j >= newColumns - padding)
                            continue;
                        ret[i, j, k] = this[i - padding, j - padding, k];
                    }
                }
            }
            return ret;
        }

        public I3DFloatTensor RemovePadding(uint padding)
        {
            var newRows = _rows - padding * 2;
            var newColumns = _columns - padding * 2;
            var ret = new Numerics3DTensor(Context, newRows, newColumns, Depth);
            for (uint k = 0; k < Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        ret[i, j, k] = this[i + padding, j + padding, k];
                    }
                }
            }
            return ret;
        }

        public IFloatVector ReshapeAsVector()
        {
	        return new NumericsVector(Context, DenseVector.Build.Dense(_data.GetInternalArray()));
        }

		public IFloatMatrix ReshapeAsMatrix()
		{
			return _data;
		}

		public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns)
		{
			Debug.Assert(rows * columns == _rows);
			var tensorList = new List<IIndexable3DFloatTensor>();
			for (uint i = 0; i < Depth; i++) {
				var slice = GetMatrixAt(i);
				tensorList.Add(slice.ReshapeAs3DTensor(rows, columns).AsIndexable());
			}
			return new Numerics4DTensor(Context, tensorList);
		}

		public (I3DFloatTensor Result, I3DFloatTensor Indices) MaxPool(
            uint filterWidth, 
            uint filterHeight, 
            uint xStride,
            uint yStride,
            bool saveIndices
		) {
            var newColumns = (ColumnCount - filterWidth) / xStride + 1;
            var newRows = (RowCount - filterHeight) / yStride + 1;
            var matrixList = new List<NumericsMatrix>();
            var indexList = saveIndices ? new List<NumericsMatrix>() : null;
            var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);

            for (uint k = 0; k < Depth; k++) {
                var indices = saveIndices ? new NumericsMatrix(Context, DenseMatrix.Create((int)newRows, (int)newColumns, 0f)) : null;
                var layer = new NumericsMatrix(Context, DenseMatrix.Create((int)newRows, (int)newColumns, 0f));
                
                foreach(var (cx, cy) in convolutions) {
                    var targetX = cx / xStride;
                    var targetY = cy / yStride;
                    var maxVal = float.MinValue;
	                var bestOffset = -1;
	                var offset = 0;
	                
	                for (uint x = 0; x < filterWidth; x++) {
		                for (uint y = 0; y < filterHeight; y++) {
			                var val = this[cy + y, cx + x, k];
			                if (val > maxVal || bestOffset == -1) {
				                bestOffset = offset;
				                maxVal = val;
			                }
							++offset;
		                }
	                }

					if(saveIndices)
						indices[targetY, targetX] = bestOffset;
                    layer[targetY, targetX] = maxVal;
                }
                matrixList.Add(layer);
	            indexList?.Add(indices);
            }
            return (new Numerics3DTensor(Context, matrixList), saveIndices ? new Numerics3DTensor(Context, indexList) : null);
        }

        public I3DFloatTensor ReverseMaxPool(
            I3DFloatTensor indexList, 
            uint outputRows, 
            uint outputColumns, 
            uint filterWidth, 
            uint filterHeight, 
            uint xStride, 
            uint yStride)
        {
            var matrixList = new List<DenseMatrix>();
	        for (uint k = 0; k < Depth; k++) {
		        var source = GetMatrixAt(k).AsIndexable();
		        var sourceRows = source.RowCount;
		        var sourceColumns = source.ColumnCount;
		        var index = indexList.GetMatrixAt(k).AsIndexable();
		        var target = DenseMatrix.Create((int)outputRows, (int)outputColumns, 0f);
		        matrixList.Add(target);

		        for (uint j = 0; j < sourceColumns; j++) {
			        for (uint i = 0; i < sourceRows; i++) {
				        var value = source[i, j];
				        var offset = index[i, j];
				        var offsetRow = (uint)offset % filterHeight;
				        var offsetColumn = (uint)offset / filterHeight;
						target[(int)(i*yStride + offsetRow), (int)(j*xStride + offsetColumn)] = value;
			        }
		        }
	        }

	        return new Numerics3DTensor(Context, matrixList.Select(m => new NumericsMatrix(Context, m)).ToList());
        }

	    public IFloatMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
	    {
		    var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);
		    var filterSize = filterWidth * filterHeight;
		    var ret = new NumericsMatrix(Context, DenseMatrix.Create(convolutions.Count, (int)(filterSize * Depth), 0f));

		    for(int i = 0; i < convolutions.Count; i++) {
			    var (offsetX, offsetY) = convolutions[i];
			    for (uint k = 0; k < Depth; k++) {
					var filterOffset = k * filterSize;
				    for (uint y = 0; y < filterHeight; y++) {
					    for (uint x = 0; x < filterWidth; x++) {
							// write in column major format
							var filterIndex = filterOffset + (x * filterHeight + y);
						    ret[(uint)i, filterIndex] = this[offsetY + y, offsetX + x, k];
					    }
				    }
			    }
		    }

		    return ret;
	    }

        public I3DFloatTensor ReverseIm2Col(
            IFloatMatrix filterMatrix, 
            uint outputRows, 
            uint outputColumns, 
            uint outputDepth, 
            uint filterWidth, 
            uint filterHeight, 
            uint xStride, 
            uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
	        var output = outputDepth.AsRange().Select(i => DenseMatrix.Create((int)outputRows, (int)outputColumns, 0f)).ToList();

			for (uint k = 0; k < Depth; k++) {
				var slice = GetMatrixAt(k).AsIndexable();
				var filters = filterMatrix.Column(k).Split(outputDepth).Select(v => v.AsIndexable()).ToList();

				foreach (var (cx, cy) in convolutions) {
					var errorY = cy / xStride;
					var errorX = cx / yStride;
					if (errorX < slice.ColumnCount && errorY < slice.RowCount) {
						var error = slice[errorY, errorX];
						for (uint y = 0; y < filterHeight; y++) {
							for (uint x = 0; x < filterWidth; x++) {
								var filterIndex = (filterWidth-x-1)  * filterHeight + (filterHeight-y-1);
								for (var z = 0; z < outputDepth; z++)
									output[z][(int)(cy + y), (int)(cx + x)] += filters[z][filterIndex] * error;
							}
						}
					}
				}
			}
			return new Numerics3DTensor(Context, output.Select(m => new NumericsMatrix(Context, m)).ToList());
        }

        public IFloatMatrix CombineDepthSlices()
        {
            var ret = new NumericsMatrix(Context, DenseMatrix.Create((int)_rows, (int)_columns, 0f));
            for (uint i = 0; i < Depth; i++)
                ret.AddInPlace(GetMatrixAt(i));
            return ret;
        }

        public void AddInPlace(I3DFloatTensor tensor)
        {
            var other = (Numerics3DTensor)tensor;
	        _data.AddInPlace(other._data);
        }

        public I3DFloatTensor Multiply(IFloatMatrix matrix)
        {
            var ret = new List<IIndexableFloatMatrix>();
            foreach (var item in Matrix)
                ret.Add(item.Multiply(matrix).AsIndexable());
            return new Numerics3DTensor(Context, ret);
        }

        public void AddToEachRow(IFloatVector vector)
        {
	        var row = vector.Data.Segment;
	        for (uint k = 0; k < _depth; k++) {
		        for (uint j = 0; j < _columns; j++) {
			        for (uint i = 0; i < _rows; i++)
				        this[i, j, k] += row[j];
		        }
	        }
        }

        public I3DFloatTensor TransposeThisAndMultiply(I4DFloatTensor tensor)
        {
            Debug.Assert(tensor.Count == Depth);
            var ret = new List<IIndexableFloatMatrix>();
            for (uint i = 0; i < tensor.Count; i++) {
                var multiplyWith = tensor.GetTensorAt(i).ReshapeAsMatrix();
                var slice = GetMatrixAt(i);
                ret.Add(slice.TransposeThisAndMultiply(multiplyWith).AsIndexable());
            }
            return new Numerics3DTensor(Context, ret);
        }

	    public string AsXml
	    {
		    get
		    {
			    var ret = new StringBuilder();
			    var settings = new XmlWriterSettings {
				    OmitXmlDeclaration = true
			    };
			    using (var writer = XmlWriter.Create(new StringWriter(ret), settings)) {
				    writer.WriteStartElement("tensor-3d");
				    foreach(var matrix in Matrix) {
					    writer.WriteRaw(matrix.AsXml);
				    }
				    writer.WriteEndElement();
			    }
			    return ret.ToString();
		    }
	    }
    }
}
