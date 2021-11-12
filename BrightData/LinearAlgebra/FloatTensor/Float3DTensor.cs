using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightData.Helper;

namespace BrightData.LinearAlgebra.FloatTensor
{
    internal class Float3DTensor : IIndexable3DFloatTensor
    {
        public Float3DTensor(Tensor3D<float> data) => Data = data;
        public void Dispose() => Data.Dispose();

        public uint RowCount => Data.RowCount;
        public uint ColumnCount => Data.ColumnCount;
        public uint Depth => Data.Depth;
        public Tensor3D<float> Data { get; set; }
        public IFloatMatrix GetMatrixAt(uint depth) => new FloatMatrix(Data.Matrix(depth));
        public IIndexable3DFloatTensor AsIndexable() => this;

        public I3DFloatTensor AddPadding(uint padding)
        {
            var newRows = RowCount + padding * 2;
            var newColumns = ColumnCount + padding * 2;
            var ret = Data.Context.CreateTensor3D<float>(Depth, newRows, newColumns);

            for (uint k = 0; k < Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        if (i < padding || j < padding)
                            continue;
                        if (i >= newRows - padding || j >= newColumns - padding)
                            continue;
                        ret[i, j, k] = this[i - padding, j - padding, k];
                    }
                }
            }
            return new Float3DTensor(ret);
        }

        public I3DFloatTensor RemovePadding(uint padding)
        {
            var newRows = RowCount - padding * 2;
            var newColumns = ColumnCount - padding * 2;
            var ret = Data.Context.CreateTensor3D<float>(Depth, newRows, newColumns);
            for (uint k = 0; k < Depth; k++) {
                for (uint i = 0; i < newRows; i++) {
                    for (uint j = 0; j < newColumns; j++) {
                        ret[i, j, k] = this[i + padding, j + padding, k];
                    }
                }
            }
            return new Float3DTensor(ret);
        }

        public IFloatMatrix Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);
            var filterSize = filterWidth * filterHeight;
            var ret = Data.Context.CreateMatrix((uint)convolutions.Count, filterSize * Depth, 0f);

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

            return new FloatMatrix(ret);
        }

        public IFloatVector ReshapeAsVector() => new FloatVector(new Vector<float>(Data.Segment));

        public IFloatMatrix ReshapeAsMatrix()
        {
            return new FloatMatrix(Data.Reshape(RowCount * ColumnCount, Depth));
        }

        public I4DFloatTensor ReshapeAs4DTensor(uint rows, uint columns)
        {
            Debug.Assert(rows * columns == RowCount);
            var tensorList = new Tensor3D<float>[Depth];
            for (uint i = 0; i < Depth; i++) {
                var slice = GetMatrixAt(i);
                tensorList[i] = slice.ReshapeAs3DTensor(rows, columns).Data;
            }
            return new Float4DTensor(tensorList);
        }

        public (I3DFloatTensor Result, I3DFloatTensor? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            var context = Data.Context;
            var newColumns = (ColumnCount - filterWidth) / xStride + 1;
            var newRows = (RowCount - filterHeight) / yStride + 1;
            var matrixList = new Matrix<float>[Depth];
            var indexList = saveIndices ? new Matrix<float>[Depth] : null;
            var convolutions = ConvolutionHelper.Default(ColumnCount, RowCount, filterWidth, filterHeight, xStride, yStride);

            for (uint k = 0; k < Depth; k++) {
                var indices = saveIndices ? context.CreateMatrix(newRows, newColumns, 0f) : null;
                var layer = context.CreateMatrix(newRows, newColumns, 0f);
                
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

                    if(indices != null)
                        indices[targetY, targetX] = bestOffset;
                    layer[targetY, targetX] = maxVal;
                }
                matrixList[k] = layer;
                if(indexList != null && indices != null)
                    indexList[k] = indices;
            }
            return (new Float3DTensor(context.CreateTensor3D(matrixList)), indexList != null ? new Float3DTensor(context.CreateTensor3D(indexList)) : null);
        }

        public I3DFloatTensor ReverseMaxPool(I3DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var context = Data.Context;
            var matrixList = new Matrix<float>[Depth];
            for (uint k = 0; k < Depth; k++) {
                var source = GetMatrixAt(k).AsIndexable();
                var sourceRows = source.RowCount;
                var sourceColumns = source.ColumnCount;
                var index = indices.GetMatrixAt(k).AsIndexable();
                var target = context.CreateMatrix(outputRows, outputColumns, 0f);
                matrixList[k] = target;

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

            return new Float3DTensor(context.CreateTensor3D(matrixList));
        }

        public I3DFloatTensor ReverseIm2Col(IFloatMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var context = Data.Context;
            var convolutions = ConvolutionHelper.Default(outputColumns, outputRows, filterWidth, filterHeight, xStride, yStride);
            var output = outputDepth.AsRange().Select(i => context.CreateMatrix(outputRows, outputColumns, 0f)).ToArray();

            for (uint k = 0; k < Depth; k++) {
                var slice = GetMatrixAt(k).AsIndexable();
                var filters = filter.Column(k).Split(outputDepth).Select(v => v.AsIndexable()).ToList();

                foreach (var (cx, cy) in convolutions) {
                    var errorY = cy / xStride;
                    var errorX = cx / yStride;
                    if (errorX < slice.ColumnCount && errorY < slice.RowCount) {
                        var error = slice[errorY, errorX];
                        for (uint y = 0; y < filterHeight; y++) {
                            for (uint x = 0; x < filterWidth; x++) {
                                var filterIndex = (filterWidth-x-1)  * filterHeight + (filterHeight-y-1);
                                for (uint z = 0; z < outputDepth; z++)
                                    output[z][cy + y, cx + x] += filters[(int)z][filterIndex] * error;
                            }
                        }
                    }
                }
            }

            return new Float3DTensor(context.CreateTensor3D(output));
        }

        public IFloatMatrix CombineDepthSlices()
        {
            var ret = Data.Context.CreateMatrix(RowCount, ColumnCount, 0f);
            for (uint i = 0; i < Depth; i++)
                ret.AddInPlace(Data.Matrix(i));
            return new FloatMatrix(ret);
        }

        public void AddInPlace(I3DFloatTensor tensor) => Data.AddInPlace(tensor.Data);

        public I3DFloatTensor Multiply(IFloatMatrix matrix)
        {
            var data = matrix.Data;
            var ret = Data.Matrices.Select(m => m.Multiply(data)).ToArray();
            return new Float3DTensor(Data.Context.CreateTensor3D(ret));
        }

        public void AddToEachRow(IFloatVector vector)
        {
            var row = vector.Data.Segment;
            for (uint k = 0; k < Depth; k++) {
                for (uint j = 0; j < ColumnCount; j++) {
                    for (uint i = 0; i < RowCount; i++)
                        this[i, j, k] += row[j];
                }
            }
        }

        public I3DFloatTensor TransposeThisAndMultiply(I4DFloatTensor tensor)
        {
            Debug.Assert(tensor.Count == Depth);
            var ret = new Matrix<float>[tensor.Count];
            for (uint i = 0; i < tensor.Count; i++) {
                var multiplyWith = tensor.GetTensorAt(i).ReshapeAsMatrix();
                var slice = GetMatrixAt(i);
                ret[i] = slice.TransposeThisAndMultiply(multiplyWith).Data;
            }
            return new Float3DTensor(Data.Context.CreateTensor3D(ret));
        }

        public float this[uint row, uint column, uint depth]
        {
            get => Data[depth, row, column];
            set => Data[depth, row, column] = value;
        }

        public IIndexableFloatMatrix[] Matrix => Data.Matrices.Select(m => (IIndexableFloatMatrix)new FloatMatrix(m)).ToArray();
        public string AsXml
        {
            get
            {
                var ret = new StringBuilder();
                var settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true
                };
                using var writer = XmlWriter.Create(new StringWriter(ret), settings);
                writer.WriteStartElement("tensor-3d");
                foreach(var matrix in Matrix) {
                    writer.WriteRaw(matrix.AsXml);
                }
                writer.WriteEndElement();
                return ret.ToString();
            }
        }
        public override string ToString() => $"Simple: {Data}";

        public ILinearAlgebraProvider LinearAlgebraProvider => Data.Context.LinearAlgebraProvider;
    }
}
