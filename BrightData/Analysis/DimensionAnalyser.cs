using System;
using System.Collections.Generic;
using BrightData.Types;

namespace BrightData.Analysis
{
    /// <summary>
    /// Tensor analysis
    /// </summary>
    internal class DimensionAnalyser : IDataAnalyser<IReadOnlyTensor<float>>
    {
        readonly HashSet<(uint X, uint Y, uint Z, uint C)> _distinct = [];

        public uint? XDimension { get; private set; }
        public uint? YDimension { get; private set; }
        public uint? ZDimension { get; private set; }
        public uint? TensorCount { get; private set; }

        public void Add(IReadOnlyTensor<float> obj)
        {
            uint x = 0, y = 0, z = 0, c = 0;
            if (obj is IHaveTensor4DDimensions tensor4D && (TensorCount == null || tensor4D.Count > TensorCount)) TensorCount = c = tensor4D.ColumnCount;
            if (obj is IHaveTensor3DDimensions tensor3D && (ZDimension == null || tensor3D.Depth > ZDimension)) ZDimension = z = tensor3D.Depth;
            if (obj is IHaveMatrixDimensions matrix) {
                if (XDimension == null || matrix.ColumnCount > XDimension)
                    XDimension = x = matrix.ColumnCount;
                if (YDimension == null || matrix.RowCount > YDimension)
                    YDimension = y = matrix.RowCount;
            } else {
                if (XDimension == null || obj.Size > XDimension)
                    XDimension = x = obj.Size;
            }

            _distinct.Add((x, y, z, c));
        }

        public void AddObject(object obj)
        {
            if (obj is IReadOnlyTensor<float> tensor)
                Add(tensor);
            else
                throw new ArgumentException("Expected a tensor", nameof(obj));
        }

        public void Append(ReadOnlySpan<IReadOnlyTensor<float>> block)
        {
            foreach(ref readonly var item in block)
                Add(item);
        }

        public void WriteTo(MetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.SetIfNotNull(Consts.XDimension, XDimension);
            metadata.SetIfNotNull(Consts.YDimension, YDimension);
            metadata.SetIfNotNull(Consts.ZDimension, ZDimension);
            metadata.SetIfNotNull(Consts.CDimension, TensorCount);
            metadata.Set(Consts.NumDistinct, (uint)_distinct.Count);

            var size = (XDimension ?? 1) * (YDimension ?? 1) * (ZDimension ?? 1) * (TensorCount ?? 1);
            metadata.Set(Consts.Size, size);
        }
    }
}
