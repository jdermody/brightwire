using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    class Numerics4DTensor //: IComputable4DTensor
    {
        //readonly NumericsMatrix _data;
        //readonly int _rows, _columns, _depth, _count;

        //public Numerics4DTensor(int rows, int columns, int depth, int count)
        //{
        //    _rows = rows;
        //    _columns = columns;
        //    _depth = depth;
        //    _count = count;
        //    _data = new NumericsMatrix(DenseMatrix.Build.Dense(_rows * _count * _depth, _count));
        //}

        //public Numerics4DTensor(IReadOnlyList<IComputable3DTensor> tensorList)
        //{
        //    var first = tensorList.First();
        //    Debug.Assert(tensorList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount && m.Depth == first.Depth));
        //    _rows = first.RowCount;
        //    _columns = first.ColumnCount;
        //    _depth = first.Depth;
        //    _count = tensorList.Count;

        //    var offset = 0;
        //    var rowSize = _rows * _columns * _depth;
        //    var data = new float[rowSize * _count];
        //    foreach (var matrix in tensorList) {
        //        Array.Copy(matrix.GetInternalArray(), 0, data, offset, rowSize);
        //        offset += rowSize;
        //    }

        //    _data = new NumericsMatrix(DenseMatrix.Build.Dense(rowSize, _count, data));
        //}

        //public int RowCount => _rows;
        //public int ColumnCount => _columns;
        //public int Depth => _depth;
        //public int Count => _count;
        //public float[] GetInternalArray() => _data.GetInternalArray();

        //public void Dispose()
        //{
        //    // nop
        //}

        //public override string ToString()
        //{
        //    return $"4D tensor, rows:{RowCount} columns:{ColumnCount} depth:{Depth} count:{Count}";
        //}

        //public IComputable3DTensor GetTensorAt(int index) => _data.Column(index).ReshapeAs3DTensor(_rows, _columns, _depth);
        //public IReadOnlyList<IComputable3DTensor> Tensors => _data.Columns.Select(c => c.ReshapeAs3DTensor(_rows, _columns, _depth)).ToList();

        //public IComputable4DTensor AddPadding(int padding)
        //{
        //    var ret = new List<IComputable3DTensor>();
        //    foreach (var item in Tensors)
        //        ret.Add(item.AddPadding(padding));
        //    return new Numerics4DTensor(ret);
        //}

        //public IComputable4DTensor RemovePadding(int padding)
        //{
        //    var ret = new List<IComputable3DTensor>();
        //    foreach (var item in Tensors)
        //        ret.Add(item.RemovePadding(padding));
        //    return new Numerics4DTensor(ret);
        //}

        //public (IComputable4DTensor Result, IComputable4DTensor Indices) MaxPool(int filterWidth, int filterHeight, int xStride, int yStride, bool saveIndices)
        //{
        //    List<IComputable3DTensor> indexList = saveIndices ? new List<IComputable3DTensor>() : null;
        //    var ret = new List<IComputable3DTensor>();
        //    for (var i = 0; i < Count; i++) {
        //        var (result, indices) = GetTensorAt(i).MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
        //        indexList?.Add(indices);
        //        ret.Add(result);
        //    }
        //    return (new Numerics4DTensor(ret), saveIndices ? new Numerics4DTensor(indexList) : null);
        //}

        //public IComputable4DTensor ReverseMaxPool(IComputable4DTensor indices, int outputRows, int outputColumns, int filterWidth, int filterHeight, int xStride, int yStride)
        //{
        //    var ret = new List<IComputable3DTensor>();
        //    for (var i = 0; i < Count; i++) {
        //        var result = GetTensorAt(i).ReverseMaxPool(indices.GetTensorAt(i), outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
        //        ret.Add(result);
        //    }
        //    return new Numerics4DTensor(ret);
        //}

        //public IComputable3DTensor Im2Col(int filterWidth, int filterHeight, int xStride, int yStride)
        //{
        //    var ret = new List<IComputableMatrix>();
        //    for (var i = 0; i < Count; i++) {
        //        var result = GetTensorAt(i).Im2Col(filterWidth, filterHeight, xStride, yStride);
        //        ret.Add(result);
        //    }
        //    return new Numerics3DTensor(ret);
        //}

        //public IComputable4DTensor ReverseIm2Col(IComputableMatrix filters, int outputRows, int outputColumns, int outputDepth, int filterWidth, int filterHeight, int xStride, int yStride)
        //{
        //    var ret = new List<IComputable3DTensor>();
        //    for (var i = 0; i < Count; i++) {
        //        var result = GetTensorAt(i).ReverseIm2Col(filters, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
        //        ret.Add(result);
        //    }
        //    return new Numerics4DTensor(ret);
        //}

        //public IComputableMatrix ReshapeAsMatrix()
        //{
        //    return _data;
        //}

        ////public IReadOnlyList<FloatTensor> Data
        ////{
        ////    get => Tensors.Select(t => t.Data).ToList();
        ////    set {
        ////        var count = value.Count;
        ////        for (var z = 0; z < count && z < _count; z++) {
        ////            var tensor = value[z];
        ////            if (tensor != null) {
        ////                var matrixList = tensor.Matrix;
        ////                var matrixCount = matrixList.Length;
        ////                for (var k = 0; k < matrixCount && k < _depth; k++) {
        ////                    var matrix = matrixList[k];
        ////                    if (matrix.Row != null) {
        ////                        for (int i = 0, len = matrix.Row.Length; i < len; i++) {
        ////                            var row = matrix.Row[i];
        ////                            for (int j = 0, len2 = row.Count; j < len2; j++)
        ////                                this[i, j, k, z] = row.Data[j];
        ////                        }
        ////                    }
        ////                }
        ////            }
        ////        }
        ////    }
        ////}

        //public IComputableVector ReshapeAsVector()
        //{
        //    return _data.ReshapeAsVector();
        //}

        //public IComputableVector ColumnSums()
        //{
        //    IComputableVector ret = null;
        //    for (var i = 0; i < Count; i++) {
        //        var tensorAsMatrix = GetTensorAt(i).ReshapeAsMatrix();
        //        var columnSums = tensorAsMatrix.ColumnSums();
        //        if (ret == null)
        //            ret = columnSums;
        //        else
        //            ret.AddInPlace(columnSums);
        //    }
        //    return ret;
        //}

        //public float this[int row, int column, int depth, int index]
        //{
        //    get => _data[_RowIndex(row, column, depth), index];
        //    set => _data[_RowIndex(row, column, depth), index] = value;
        //}

        //int _RowIndex(int row, int column, int depth) => depth * _depth + (column * _rows + row);

        //public string AsXml
        //{
        //    get {
        //        var ret = new StringBuilder();
        //        var settings = new XmlWriterSettings {
        //            OmitXmlDeclaration = true
        //        };
        //        using (var writer = XmlWriter.Create(new StringWriter(ret), settings)) {
        //            writer.WriteStartElement("tensor-4d");
        //            foreach (var tensor in Tensors) {
        //                writer.WriteRaw(tensor.AsXml);
        //            }
        //            writer.WriteEndElement();
        //        }
        //        return ret.ToString();
        //    }
        //}
    }
}
