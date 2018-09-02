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

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// 3D Tensor that uses the CPU based math.net numerics library
    /// </summary>
    class Cpu3DTensor : IIndexable3DTensor
    {
        readonly CpuMatrix[] _data;
        readonly int _rows, _columns;

        public Cpu3DTensor(int rows, int columns, int depth)
        {
            _rows = rows;
            _columns = columns;
            _data = Enumerable.Range(0, depth)
                .Select(i => new CpuMatrix(DenseMatrix.Create(_rows, _columns, 0f)))
                .ToArray()
            ;
        }

        public Cpu3DTensor(IReadOnlyList<IMatrix> matrixList)
        {
            var first = matrixList.First();
            Debug.Assert(matrixList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount));
            _rows = first.RowCount;
            _columns = first.ColumnCount;
            _data = matrixList.Cast<CpuMatrix>().ToArray();
        }

        public void Dispose()
        {
            // nop
        }

        public float this[int row, int column, int depth]
        {
            get => _data[depth][row, column];
	        set => _data[depth][row, column] = value;
        }

        public FloatTensor Data
        {
            get
            {
                return FloatTensor.Create(_data.Select(m => m.Data).ToArray());
            }
            set
            {
                var matrixList = value.Matrix;
                var matrixCount = matrixList.Length;
                for (var i = 0; i < matrixCount && i < _data.Length; i++) {
                    var matrix = matrixList[i];
                    if (matrix.Row != null)
                        _data[i].Data = matrix;
                }
            }
        }

        public IReadOnlyList<IIndexableMatrix> Matrix => _data;
	    public int Depth => _data.Length;
        public int RowCount => _rows;
        public int ColumnCount => _columns;
        public IMatrix GetMatrixAt(int depth) => _data[depth];
        public IIndexable3DTensor AsIndexable() => this;

        public I3DTensor AddPadding(int padding)
        {
            var newRows = _rows + padding * 2;
            var newColumns = _columns + padding * 2;
            var ret = new Cpu3DTensor(newRows, newColumns, Depth);

            for (var k = 0; k < Depth; k++) {
                for (var i = 0; i < newRows; i++) {
                    for (var j = 0; j < newColumns; j++) {
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

        public I3DTensor RemovePadding(int padding)
        {
            var newRows = _rows - padding * 2;
            var newColumns = _columns - padding * 2;
            var ret = new Cpu3DTensor(newRows, newColumns, Depth);
            for (var k = 0; k < Depth; k++) {
                for (var i = 0; i < newRows; i++) {
                    for (var j = 0; j < newColumns; j++) {
                        ret[i, j, k] = this[i + padding, j + padding, k];
                    }
                }
            }
            return ret;
        }

        public IVector ConvertToVector()
        {
            var vectorList = _data.Select(m => m.ConvertInPlaceToVector().AsIndexable()).ToArray();
            var size = _rows * _columns;
            var ret = DenseVector.Create(Depth * size, i => {
                var offset = i / size;
                var index = i % size;
                return vectorList[offset][index];
            });
            return new CpuVector(ret);
        }

        public IMatrix ConvertToMatrix()
        {
            var ret = DenseMatrix.Create(_rows * _columns, Depth, (i, j) => {
                var matrix = _data[j];
                var x = i % _rows;
                var y = i / _rows;
                return matrix[x, y];
            });
            return new CpuMatrix(ret);
        }

        public I4DTensor ConvertTo4DTensor(int rows, int columns)
        {
            var tensorList = new List<I3DTensor>();
            for (var i = 0; i < Depth; i++) {
                var slice = GetMatrixAt(i);
                tensorList.Add(slice.ConvertTo3DTensor(rows, columns));
            }
            return new Cpu4DTensor(tensorList);
        }

        public (I3DTensor Result, I3DTensor Indices) MaxPool(
            int filterWidth, 
            int filterHeight, 
            int stride,
            bool saveIndices
		) {
            var newColumns = (ColumnCount - filterWidth) / stride + 1;
            var newRows = (RowCount - filterHeight) / stride + 1;
            var matrixList = new List<CpuMatrix>();
            var indexList = saveIndices ? new List<CpuMatrix>() : null;
            var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, stride);

            for (var k = 0; k < Depth; k++) {
                var matrix = _data[k];
                var indices = saveIndices ? new CpuMatrix(DenseMatrix.Create(newRows, newColumns, 0f)) : null;
                var layer = new CpuMatrix(DenseMatrix.Create(newRows, newColumns, 0f));
                
                foreach(var (cx, cy) in convolutions) {
                    var targetX = cx / stride;
                    var targetY = cy / stride;
                    var maxVal = float.MinValue;
	                var bestOffset = -1;
	                var offset = 0;
	                
	                for (var x = 0; x < filterWidth; x++) {
		                for (var y = 0; y < filterHeight; y++) {
			                var val = matrix[cy + y, cx + x];
			                if (val > maxVal || bestOffset == -1) {
				                bestOffset = offset;
				                maxVal = val;
			                }
							++offset;
		                }
	                }

	                //var index = targetX * newRows + targetY;
					if(saveIndices)
						indices[targetY, targetX] = bestOffset;
                    layer[targetY, targetX] = maxVal;
                }
                matrixList.Add(layer);
	            indexList?.Add(indices);
            }
            return (new Cpu3DTensor(matrixList), saveIndices ? new Cpu3DTensor(indexList) : null);
        }

        public I3DTensor ReverseMaxPool(I3DTensor indexList, int outputRows, int outputColumns, int filterWidth, int filterHeight, int stride)
        {
            var matrixList = new List<DenseMatrix>();
	        for (var z = 0; z < Depth; z++) {
		        var source = GetMatrixAt(z).AsIndexable();
		        var sourceRows = source.RowCount;
		        var sourceColumns = source.ColumnCount;
		        var index = indexList.GetMatrixAt(0).AsIndexable();
		        var target = DenseMatrix.Create(outputRows, outputColumns, 0f);
		        matrixList.Add(target);

		        for (var j = 0; j < sourceColumns; j++) {
			        for (int i = 0; i < sourceRows; i++) {
				        var value = source[i, j];
				        var offset = index[i, j];
				        var offsetRow = (int)offset % filterHeight;
				        var offsetColumn = (int)offset / filterHeight;
						target[i*stride + offsetRow, j*stride + offsetColumn] = value;
			        }
		        }
	        }

	        return new Cpu3DTensor(matrixList.Select(m => new CpuMatrix(m)).ToList());
        }

        //public (IMatrix WeightUpdate, IVector BiasUpdate) CalculateWeightUpdate(IMatrix im2Col)
        //{
        //    var multiplyWith = DenseMatrix.Create(RowCount * ColumnCount, Depth, 0f);
        //    var biasUpdate = new float[Depth];

        //    for (var k = 0; k < Depth; k++) {
        //        var total = 0f;
        //        int count = 0;
        //        var slice = GetDepthSlice(k).AsIndexable();
        //        for (var x = 0; x < slice.ColumnCount; x++) {
        //            for (var y = 0; y < slice.RowCount; y++) {
        //                var value = slice[x, y];
        //                multiplyWith[x * slice.RowCount + y, k] = value;
        //                total += value;
        //                ++count;
        //            }
        //        }
        //        biasUpdate[k] = total / count;
        //    }

        //    var delta = im2Col.TransposeThisAndMultiply(new CpuMatrix(multiplyWith));
        //    var biasUpdateVector = new CpuVector(biasUpdate);
        //    return (delta, biasUpdateVector);
        //}

        //public I3DTensor CalculatePreviousError(IMatrix filterMatrix, int inputHeight, int inputWidth, int inputDepth, int padding, int filterHeight, int filterWidth, int stride)
        //{
        //    var filters = filterMatrix.AsIndexable().Columns.ToList();
        //    var columns = inputWidth + padding * 2;
        //    var rows = inputHeight + padding * 2;
        //    var matrixList = Enumerable.Range(0, inputDepth)
        //        .Select(i => DenseMatrix.Create(rows, columns, 0f))
        //        .ToList()
        //    ;
        //    var convolutions = ConvolutionHelper.Default(columns, rows, filterHeight, filterHeight, stride);

        //    for (var k = 0; k < Depth; k++) {
        //        var slice = GetMatrixAt(k).AsIndexable();
        //        var filterList = filters[k]
        //            .Split(inputDepth)
        //            .Select(f => f.Rotate(f.Count / filterWidth).AsIndexable())
        //            .ToList()
        //        ;

        //        foreach (var convolution in convolutions) {
        //            var first = convolution.First();
        //            var error = slice[first.X / stride, first.Y / stride];
	       //         foreach (var item in convolution) {
		      //          var i = item.X - first.X;
		      //          var j = item.Y - first.Y;
		      //          var filterIndex = i * filterHeight + j;
		      //          for (var z = 0; z < filterList.Count; z++) {
			     //           var filter = filterList[z];
			     //           var output = matrixList[z];
			     //           output[item.Y, item.X] += filter[filterIndex] * error;
		      //          }
	       //         }
        //        }
        //    }

        //    var matrixList2 = matrixList.Select(m => new CpuMatrix(m));
        //    var ret = new Cpu3DTensor(matrixList2.ToList());
        //    if (padding > 0)
        //        return ret.RemovePadding(padding);
        //    return ret;
        //}

	    public IMatrix Im2Col(int filterWidth, int filterHeight, int stride)
	    {
		    var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, stride);
		    var filterSize = filterWidth * filterHeight;
		    var ret = new CpuMatrix(DenseMatrix.Create(convolutions.Count, filterSize * Depth, 0f));

		    for(var i = 0; i < convolutions.Count; i++) {
			    var offset = convolutions[i];
			    for (var k = 0; k < Depth; k++) {
					var filterOffset = k * filterSize;
				    for (var y = 0; y < filterHeight; y++) {
					    for (var x = 0; x < filterWidth; x++) {
							// write as column major
							var filterIndex = filterOffset + (x * filterHeight + y);
						    ret[i, filterIndex] = this[offset.Y + y, offset.X + x, k];
					    }
				    }
			    }
		    }

		    return ret;
	    }

        public IMatrix ReverseIm2Col(IReadOnlyList<IReadOnlyList<IVector>> filters, int inputHeight, int inputWidth, int padding, int filterWidth, int filterHeight, int stride)
        {
	        var outputRows = inputHeight + padding * 2;
	        var outputColumns = inputWidth + padding * 2;
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, stride);
	        var output = DenseMatrix.Create(outputRows, outputColumns, 0f);
	        var numFilters = filters[0].Count;

			for (var k = 0; k < Depth; k++) {
				var slice = GetMatrixAt(k).AsIndexable();
				var filterList = filters[k].Select(f => f.AsIndexable()).ToList();

				foreach (var (cx, cy) in convolutions) {
					var error = slice[cy / stride, cx / stride];
					for (var y = 0; y < filterHeight; y++) {
						for (var x = 0; x < filterWidth; x++) {
							var filterIndex = x * filterHeight + y;
							for (var z = 0; z < numFilters; z++) {
								var filter = filterList[z];
								output[cy+y, cx+x] += filter[filterIndex] * error;
							}
						}
					}
				}
			}
			return new CpuMatrix(output);
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
            for (var i = 0; i < Depth; i++)
                _data[i].AddInPlace(other.GetMatrixAt(i));
        }

        public I3DTensor Multiply(IMatrix matrix)
        {
            var ret = new List<IMatrix>();
            foreach (var item in _data)
                ret.Add(item.Multiply(matrix));
            return new Cpu3DTensor(ret);
        }

        public void AddToEachRow(IVector vector)
        {
            foreach (var item in _data)
                item.AddToEachRow(vector);
        }

        public I3DTensor TransposeThisAndMultiply(I4DTensor tensor)
        {
            Debug.Assert(tensor.Count == Depth);
            var ret = new List<IMatrix>();
            for (var i = 0; i < tensor.Count; i++) {
                var multiplyWith = tensor.GetTensorAt(i).ConvertToMatrix();
                var slice = GetMatrixAt(i);
                ret.Add(slice.TransposeThisAndMultiply(multiplyWith));
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
				    writer.WriteStartElement("tensor");
				    foreach(var matrix in _data) {
					    writer.WriteRaw(matrix.AsXml);
				    }
				    writer.WriteEndElement();
			    }
			    return ret.ToString();
		    }
	    }
    }
}
