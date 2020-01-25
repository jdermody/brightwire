using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightTable.Buffers;
using BrightTable.Builders;
using BrightTable.Helper;
using BrightTable.Segments;

namespace BrightTable.Transformations
{
    class NormalisationTransformation : TableTransformationBase
    {
        readonly NormalizationType _type;
        readonly MethodInfo _normalise;

        public NormalisationTransformation(NormalizationType type)
        {
            _type = type;
            _normalise = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(m => m.Name == "_Normalise");
        }

        public List<(uint ColumnIndex, double Divide, double Subtract)> Normalisations { get; } = new List<(uint ColumnIndex, double Divide, double Subtract)>();

        protected override ISingleTypeTableSegment Transform(IColumnOrientedDataTable dataTable, uint index, ISingleTypeTableSegment column)
        {
            if (ColumnTypeClassifier.IsDecimal(column.SingleType)) {
                var type = column.SingleType.GetColumnType();
                var obj = _normalise.MakeGenericMethod(type).Invoke(this, new object[] { dataTable.Context, index, column });

                if (obj is ISingleTypeTableSegment normalised)
                    return normalised;
            }
            return column;
        }

        internal override void Transform(IRowOrientedDataTable dataTable, RowOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }

        internal override IReadOnlyList<(long Position, long EndOfColumnOffset)> Transform(ColumnOrientedTableBuilder builder)
        {
            throw new NotImplementedException();
        }

        ISingleTypeTableSegment _Normalise<T>(IBrightDataContext context, uint index, IDataTableSegment<T> segment)
        {
            var metadata = segment.Analyse();
            if (metadata.Get(Consts.IsNumeric, false)) {
                double divide, subtract = 0;
                if (_type == NormalizationType.Euclidean)
                    divide = Convert.ToDouble(metadata.Get(Consts.L2Norm));
                else if (_type == NormalizationType.Manhattan)
                    divide = Convert.ToDouble(metadata.Get(Consts.L1Norm));
                else if (_type == NormalizationType.Standard) {
                    divide = Convert.ToDouble(metadata.Get(Consts.StdDev) ?? 1);
                    subtract = Convert.ToDouble(metadata.Get(Consts.Mean));
                } else if (_type == NormalizationType.FeatureScale) {
                    var min = Convert.ToDouble(metadata.Get(Consts.Min));
                    var max = Convert.ToDouble(metadata.Get(Consts.Max));
                    divide = max - min;
                    subtract = min;
                } else
                    throw new NotImplementedException();

                var list = new List<T>();
                var typeofT = typeof(T);
                foreach (var item in segment.EnumerateTyped()) {
                    var val = Convert.ToDouble(item);
                    var normalised = (Math.Abs(divide) < FloatMath.AlmostZero) ? val : (val - subtract) / divide;
                    var converted = (T)Convert.ChangeType(normalised, typeofT);
                    list.Add(converted);
                }
                Normalisations.Add((index, divide, subtract));
                var ret = new DataSegmentBuffer<T>(context, typeofT.GetColumnType(), (uint)list.Count, list);
                ret.MetaData.Set(Consts.Name, metadata.Name());
                ret.MetaData.Set(Consts.Index, metadata.Index());
                return ret;
            }
            return null;
        }
    }
}
