using System;
using BrightWire.Helper;
using BrightWire.Models;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// 3D Tensor that uses the CPU based math.net numerics library
    /// </summary>
    class Cpu3DTensor : IIndexable3DTensor
    {
        readonly CpuMatrix _data;
        readonly uint _rows, _columns, _depth;

        public Cpu3DTensor(uint rows, uint columns, uint depth)
        {
            _rows = rows;
            _columns = columns;
	        _depth = depth;
	        _data = new CpuMatrix(DenseMatrix.Build.Dense(rows * columns, depth));
        }

        public Cpu3DTensor(IReadOnlyList<IIndexableMatrix> matrixList)
        {
            var first = matrixList.First();
            Debug.Assert(matrixList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount));
            _rows = first.RowCount;
            _columns = first.ColumnCount;
			_depth = matrixList.Count;

	        uint offset = 0;
	        var rowSize = _rows * _columns;
	        var data = new float[rowSize * _depth];
	        foreach (var matrix in matrixList) {
		        Array.Copy(matrix.GetInternalArray(), 0, data, offset, rowSize);
		        offset += rowSize;
	        }

	        _data = new CpuMatrix(DenseMatrix.Build.Dense(rowSize, _depth, data));
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

        public FloatTensor Data
        {
            get
            {
                return FloatTensor.Create(Matrix.Select(m => m.Data).ToArray());
            }
            set
            {
                var matrixList = value.Matrix;
                var matrixCount = matrixList.Length;
                for (var k = 0; k < matrixCount && k < _depth; k++) {
                    var matrix = matrixList[k];
	                if (matrix.Row != null) {
		                for (int i = 0, len = matrix.Row.Length; i < len; i++) {
			                var row = matrix.Row[i];
			                for (int j = 0, len2 = row.Count; j < len2; j++)
				                this[i, j, k] = row.Data[j];
		                }
	                }
                }
            }
        }

        public IReadOnlyList<IIndexableMatrix> Matrix => _data.Columns.Select(c => c.ReshapeAsMatrix(_rows, _columns).AsIndexable()).ToList();
	    public uint Depth => _depth;
        public uint RowCount => _rows;
        public uint ColumnCount => _columns;
        public IMatrix GetMatrixAt(uint depth) => _data.Column(depth).ReshapeAsMatrix(_rows, _columns);
        public IIndexable3DTensor AsIndexable() => this;
	    public float[] GetInternalArray() => _data.GetInternalArray();

        public I3DTensor AddPadding(uint padding)
        {
            var newRows = _rows + padding * 2;
            var newColumns = _columns + padding * 2;
            var ret = new Cpu3DTensor(newRows, newColumns, Depth);

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

        public I3DTensor RemovePadding(uint padding)
        {
            var newRows = _rows - padding * 2;
            var newColumns = _columns - padding * 2;
            var ret = new Cpu3DTensor(newRows, newColumns, Depth);
            for (uint k = 0; k < Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        ret[i, j, k] = this[i + padding, j + padding, k];
                    }
                }
            }
            return ret;
        }

        public IVector ReshapeAsVector()
        {
	        return new CpuVector(DenseVector.Build.Dense(_data.GetInternalArray()));
        }

		public IMatrix ReshapeAsMatrix()
		{
			return _data;
		}

		public I4DTensor ReshapeAs4DTensor(uint rows, uint columns)
		{
			Debug.Assert(rows * columns == _rows);
			var tensorList = new List<IIndexable3DTensor>();
			for (var i = 0; i < Depth; i++) {
				var slice = GetMatrixAt(i);
				tensorList.Add(slice.ReshapeAs3DTensor(rows, columns).AsIndexable());
			}
			return new Cpu4DTensor(tensorList);
		}

		public (I3DTensor Result, I3DTensor Indices) MaxPool(
            uint filterWidth, 
            uint filterHeight, 
            uint xStride,
            uint yStride,
            bool saveIndices
		) {
            var newColumns = (ColumnCount - filterWidth) / xStride + 1;
            var newRows = (RowCount - filterHeight) / yStride + 1;
            var matrixList = new List<CpuMatrix>();
            var indexList = saveIndices ? new List<CpuMatrix>() : null;
            var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);

            for (uint k = 0; k < Depth; k++) {
                var indices = saveIndices ? new CpuMatrix(DenseMatrix.Create(newRows, newColumns, 0f)) : null;
                var layer = new CpuMatrix(DenseMatrix.Create(newRows, newColumns, 0f));
                
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
            return (new Cpu3DTensor(matrixList), saveIndices ? new Cpu3DTensor(indexList) : null);
        }

        public I3DTensor ReverseMaxPool(
            I3DTensor indexList, 
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

	        return new Cpu3DTensor(matrixList.Select(m => new CpuMatrix(m)).ToList());
        }

	    public IMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
	    {
		    var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);
		    var filterSize = filterWidth * filterHeight;
		    var ret = new CpuMatrix(DenseMatrix.Create(convolutions.Count, filterSize * Depth, 0f));

		    for(uint i = 0; i < convolutions.Count; i++) {
			    var (offsetX, offsetY) = convolutions[i];
			    for (uint k = 0; k < Depth; k++) {
					var filterOffset = k * filterSize;
				    for (uint y = 0; y < filterHeight; y++) {
					    for (uint x = 0; x < filterWidth; x++) {
							// write in column major format
							var filterIndex = filterOffset + (x * filterHeight + y);
						    ret[i, filterIndex] = this[offsetY + y, offsetX + x, k];
					    }
				    }
			    }
		    }

		    return ret;
	    }

        public I3DTensor ReverseIm2Col(
            IMatrix filterMatrix, 
            uint outputRows, 
            uint outputColumns, 
            uint outputDepth, 
            uint filterWidth, 
            uint filterHeight, 
            uint xStride, 
            uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
	        var output = outputDepth.AsRange().Select(i => DenseMatrix.Create(outputRows, outputColumns, 0f)).ToList();

			for (var k = 0; k < Depth; k++) {
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
									output[z][cy + y, cx + x] += filters[z][filterIndex] * error;
							}
						}
					}
				}
			}
			return new Cpu3DTensor(output.Select(m => new CpuMatrix(m)).ToList());
        }

        public IMatrix CombineDepthSlices()
        {
            var ret = new CpuMatrix(DenseMatrix.Create(_rows, _columns, 0f));
            for (var i = 0; i < Depth; i++)
                ret.AddInPlace(GetMatrixAt(i));
            return ret;
        }

        public void AddInPlace(I3DTensor tensor)
        {
            var other = (Cpu3DTensor)tensor;
	        _data.AddInPlace(other._data);
        }

        public I3DTensor Multiply(IMatrix matrix)
        {
            var ret = new List<IIndexableMatrix>();
            foreach (var item in Matrix)
                ret.Add(item.Multiply(matrix).AsIndexable());
            return new Cpu3DTensor(ret);
        }

        public void AddToEachRow(IVector vector)
        {
	        var row = vector.Data.Data;
	        for (uint k = 0; k < _depth; k++) {
		        for (uint j = 0; j < _columns; j++) {
			        for (uint i = 0; i < _rows; i++)
				        this[i, j, k] += row[j];
		        }
	        }
        }

        public I3DTensor TransposeThisAndMultiply(I4DTensor tensor)
        {
            Debug.Assert(tensor.Count == Depth);
            var ret = new List<IIndexableMatrix>();
            for (uint i = 0; i < tensor.Count; i++) {
                var multiplyWith = tensor.GetTensorAt(i).ReshapeAsMatrix();
                var slice = GetMatrixAt(i);
                ret.Add(slice.TransposeThisAndMultiply(multiplyWith).AsIndexable());
            }
            return new Cpu3DTensor(ret);
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
