using System;
using System.Collections.Generic;

namespace BrightData.Analysis
{
    public class DimensionAnalyser : IDataAnalyser<ITensor<float>>
    {
        public uint? XDimension { get; private set; } = null;
        public uint? YDimension { get; private set; } = null;
        public uint? ZDimension { get; private set; } = null;
        readonly HashSet<(uint X, uint Y, uint Z)> _distinct = new HashSet<(uint X, uint Y, uint Z)>();

        public void Add(ITensor<float> obj)
        {
            uint x = 0, y = 0, z = 0;
            if (obj is Vector<float> vector) {
                if (XDimension == null || vector.Size > XDimension)
                    XDimension = x = vector.Size;
            } else if (obj is Matrix<float> matrix) {
                if (XDimension == null || matrix.ColumnCount > XDimension)
                    XDimension = x = matrix.ColumnCount;
                if (YDimension == null || matrix.RowCount > YDimension)
                    YDimension = y = matrix.RowCount;
            } else if (obj is Tensor3D<float> tensor) {
                if (XDimension == null || tensor.ColumnCount > XDimension)
                    XDimension = x = tensor.ColumnCount;
                if (YDimension == null || tensor.RowCount > YDimension)
                    YDimension = y = tensor.RowCount;
                if (ZDimension == null || tensor.Depth > ZDimension)
                    ZDimension = z = tensor.Depth;
            } else
                throw new NotImplementedException();

            if (_distinct.Count < Consts.MaxDistinct)
                _distinct.Add((x, y, z));
        }

        public void AddObject(object obj)
        {
            if (obj is ITensor<float> tensor)
                Add(tensor);
        }

        public void WriteTo(IMetaData metadata)
        {
            metadata.Set(Consts.HasBeenAnalysed, true);
            metadata.WriteIfNotNull(Consts.XDimension, XDimension);
            metadata.WriteIfNotNull(Consts.YDimension, YDimension);
            metadata.WriteIfNotNull(Consts.ZDimension, ZDimension);
            if (_distinct.Count < Consts.MaxDistinct)
                metadata.Set(Consts.NumDistinct, _distinct.Count);
        }
    }
}
