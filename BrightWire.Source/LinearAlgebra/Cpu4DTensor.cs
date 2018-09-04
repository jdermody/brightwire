using MathNet.Numerics.LinearAlgebra.Single;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BrightWire.Models;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// 4D Tensor that uses the CPU based math.net numerics library
    /// </summary>
    class Cpu4DTensor : IIndexable4DTensor
    {
        readonly Cpu3DTensor[] _data;

        public Cpu4DTensor(IReadOnlyList<I3DTensor> tensorList)
        {
            var first = tensorList.First();
            Debug.Assert(tensorList.All(m => m.RowCount == first.RowCount && m.ColumnCount == first.ColumnCount && m.Depth == first.Depth));
            RowCount = first.RowCount;
            ColumnCount = first.ColumnCount;
            Depth = first.Depth;
            _data = tensorList.Cast<Cpu3DTensor>().ToArray();
        }

        public int RowCount { get; }
	    public int ColumnCount { get; }
	    public int Depth { get; }
	    public int Count => _data.Length;

        public void Dispose()
        {
            // nop
        }

	    public override string ToString()
	    {
		    return $"4D tensor, rows:{RowCount} columns:{ColumnCount} depth:{Depth} count:{Count}";
	    }

		public I3DTensor GetTensorAt(int index)
        {
            return _data[index];
        }

	    public IReadOnlyList<IIndexable3DTensor> Tensors => _data;

        public IIndexable4DTensor AsIndexable() => this;

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

        public (I4DTensor Result, I4DTensor Indices) MaxPool(int filterWidth, int filterHeight, int stride, bool saveIndices)
        {
            List<I3DTensor> indexList = saveIndices ? new List<I3DTensor>() : null;
            var ret = new List<I3DTensor>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).MaxPool(filterWidth, filterHeight, stride, saveIndices);
                indexList?.Add(result.Indices);
                ret.Add(result.Result);
            }
            return (new Cpu4DTensor(ret), saveIndices ? new Cpu4DTensor(indexList) : null);
        }

        public I4DTensor ReverseMaxPool(I4DTensor indices, int outputRows, int outputColumns, int filterWidth, int filterHeight, int stride)
        {
            var ret = new List<I3DTensor>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseMaxPool(indices.GetTensorAt(i), outputRows, outputColumns, filterWidth, filterHeight, stride);
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

        public I4DTensor ReverseIm2Col(IMatrix filters, int outputRows, int outputColumns, int outputDepth, int filterWidth, int filterHeight, int stride)
        {
            var ret = new List<I3DTensor>();
            for (var i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseIm2Col(filters, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, stride);
                ret.Add(result);
            }
            return new Cpu4DTensor(ret);
        }

		public IMatrix ReshapeAsMatrix()
		{
			var rows = _data.Select(t => t.ReshapeAsVector().AsIndexable()).ToList();
			var first = rows.First();
			return new CpuMatrix(DenseMatrix.Create(first.Count, Count, (i, j) => rows[j][i]));
		}

	    public IReadOnlyList<FloatTensor> Data {
		    get => _data.Select(t => t.Data).ToList();
		    set {
			    var count = value.Count;
			    for (var i = 0; i < count && i < _data.Length; i++) {
				    var tensor = value[i];
				    if (tensor != null)
					    _data[i].Data = tensor;
			    }
		    }
	    }

	    public IVector ReshapeAsVector()
	    {
		    var vectorList = _data.Select(m => m.ReshapeAsVector().AsIndexable()).ToArray();
		    var size = RowCount * ColumnCount * Depth;
		    var ret = DenseVector.Create(Count * size, i => {
			    var offset = i / size;
			    var index = i % size;
			    return vectorList[offset][index];
		    });
		    return new CpuVector(ret);
	    }

        public IVector ColumnSums()
        {
            IVector ret = null;
            for (var i = 0; i < Count; i++) {
                var tensorAsMatrix = GetTensorAt(i).ReshapeAsMatrix();
                var columnSums = tensorAsMatrix.ColumnSums();
                if (ret == null)
                    ret = columnSums;
                else
                    ret.AddInPlace(columnSums);
            }
            return ret;
        }

	    public float this[int row, int column, int depth, int index] {
		    get => _data[index][row, column, depth];
		    set => _data[index][row, column, depth] = value;
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
				    writer.WriteStartElement("tensor-4d");
				    foreach(var tensor in _data) {
					    writer.WriteRaw(tensor.AsXml);
				    }
				    writer.WriteEndElement();
			    }
			    return ret.ToString();
		    }
	    }
    }
}
