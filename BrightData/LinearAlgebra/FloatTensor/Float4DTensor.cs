using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightData.LinearAlgebra.FloatTensor
{
    internal class Float4DTensor : IIndexable4DFloatTensor
    {
        public Float4DTensor(Tensor4D<float> data)
        {
            Data = data;
        }

        public void Dispose() => Data.Dispose();

        public Tensor4D<float> Data { get; }
        public ILinearAlgebraProvider LinearAlgebraProvider => Data.Context.LinearAlgebraProvider;
        public uint RowCount => Data.RowCount;
        public uint ColumnCount => Data.ColumnCount;
        public uint Depth => Data.Depth;
        public uint Count => Data.Count;
        public I3DFloatTensor GetTensorAt(uint index) => new Float3DTensor(Data.Tensor(index));

        public IIndexable4DFloatTensor AsIndexable() => this;

        public I4DFloatTensor AddPadding(uint padding)
        {
            var ret = Tensors
                .Select(t => t.AddPadding(padding).Data)
                .ToArray();
            return new Float4DTensor(Data.Context.CreateTensor4D(ret));
        }

        public I4DFloatTensor RemovePadding(uint padding)
        {
            var ret = Tensors
                .Select(t => t.RemovePadding(padding).Data)
                .ToArray();
            return new Float4DTensor(Data.Context.CreateTensor4D(ret));
        }

        public (I4DFloatTensor Result, I4DFloatTensor? Indices) MaxPool(uint filterWidth, uint filterHeight, uint xStride, uint yStride, bool saveIndices)
        {
            Tensor3D<float>[]? indexList = saveIndices 
                ? new Tensor3D<float>[Count]
                : null;
            var ret = new Tensor3D<float>[Count];

            for (uint i = 0; i < Count; i++) {
                var (result, indices) = GetTensorAt(i).MaxPool(filterWidth, filterHeight, xStride, yStride, saveIndices);
                ret[i] = result.Data;
                if(indexList != null && indices != null)
                    indexList[i] = indices.Data;
            }
            return (new Float4DTensor(Data.Context.CreateTensor4D(ret)), indexList != null ? new Float4DTensor(Data.Context.CreateTensor4D(indexList)) : null);
        }

        public I4DFloatTensor ReverseMaxPool(I4DFloatTensor indices, uint outputRows, uint outputColumns, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new Tensor3D<float>[Count];
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseMaxPool(indices.GetTensorAt(i), outputRows, outputColumns, filterWidth, filterHeight, xStride, yStride);
                ret[i] = result.Data;
            }
            return new Float4DTensor(Data.Context.CreateTensor4D(ret));
        }

        public I3DFloatTensor Im2Col(uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new Matrix<float>[Count];
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).Im2Col(filterWidth, filterHeight, xStride, yStride);
                ret[i] = result.Data;
            }
            return new Float3DTensor(Data.Context.CreateTensor3D(ret));
        }

        public I4DFloatTensor ReverseIm2Col(IFloatMatrix filter, uint outputRows, uint outputColumns, uint outputDepth, uint filterWidth, uint filterHeight, uint xStride, uint yStride)
        {
            var ret = new Tensor3D<float>[Count];
            for (uint i = 0; i < Count; i++) {
                var result = GetTensorAt(i).ReverseIm2Col(filter, outputRows, outputColumns, outputDepth, filterWidth, filterHeight, xStride, yStride);
                ret[i] = result.Data;
            }
            return new Float4DTensor(Data.Context.CreateTensor4D(ret));
        }

        public IFloatVector ReshapeAsVector() => new FloatVector(Data.Reshape());

        public IFloatMatrix ReshapeAsMatrix() => new FloatMatrix(Data.Reshape(RowCount * ColumnCount * Depth, Count));

        public float this[uint row, uint column, uint depth, uint index]
        {
            get => Data[index, depth, row, column];
            set => Data[index, depth, row, column] = value;
        }

        public IIndexable3DFloatTensor[] Tensors => Data.Tensors.Select(t => (IIndexable3DFloatTensor)new Float3DTensor(t)).ToArray();
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

        public override string ToString()
        {
            return $"Simple 4D tensor, rows:{RowCount} columns:{ColumnCount} depth:{Depth} count:{Count}";
        }
    }
}
