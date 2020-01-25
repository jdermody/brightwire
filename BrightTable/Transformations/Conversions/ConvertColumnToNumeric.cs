using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Buffers;
using BrightTable.Segments;

namespace BrightTable.Transformations.Conversions
{
    public class ConvertColumnToNumeric : IConvertColumn
    {
        readonly ColumnType? _convertTo;
        readonly double _invalidNumber;
        readonly int _invalidInteger;

        public ConvertColumnToNumeric(ColumnType? columnType = null, double invalidNumber = double.NaN, int invalidInteger = 0)
        {
            _convertTo = columnType;
            _invalidNumber = invalidNumber;
            _invalidInteger = invalidInteger;
        }

        public ISingleTypeTableSegment Convert(IBrightDataContext context, ISingleTypeTableSegment segment)
        {
            var ret = _Convert(context, segment);
            ret.MetaData.Set(Consts.Type, ret.SingleType.ToString());
            ret.MetaData.Set(Consts.IsNumeric, true);
            ((IEditableBuffer)ret).Finalise();
            return ret;
        }

        ISingleTypeTableSegment _Convert(IBrightDataContext context, ISingleTypeTableSegment segment)
        {
            var list = new List<double>();
            double min = double.MaxValue, max = double.MinValue;
            var isInteger = true;

            foreach(var val in segment.Enumerate()) {
                var str = val.ToString();
                if (double.TryParse(str, out var num)) {
                    if (num < min)
                        min = num;
                    if (num > max)
                        max = num;
                    if (!_convertTo.HasValue && isInteger && num != Math.Floor(num))
                        isInteger = false;
                    list.Add(num);
                } else
                    list.Add(_invalidNumber);
            }

            ColumnType columnType;
            if (_convertTo.HasValue)
                columnType = _convertTo.Value;
            else {
                if (isInteger) {
                    if (min >= sbyte.MinValue && max <= sbyte.MaxValue)
                        columnType = ColumnType.Byte;
                    else if (min >= short.MinValue && max <= short.MaxValue)
                        columnType = ColumnType.Short;
                    else if (min >= int.MinValue && max <= int.MaxValue)
                        columnType = ColumnType.Int;
                    else
                        columnType = ColumnType.Long;
                } else {
                    if (min >= float.MinValue && max <= float.MaxValue)
                        columnType = ColumnType.Float;
                    else
                        columnType = ColumnType.Double;
                }
            }

            switch (columnType) {
                case ColumnType.Double:
                    return new DataSegmentBuffer<double>(context, columnType, (uint)list.Count, list);
                case ColumnType.Float:
                    return new DataSegmentBuffer<float>(context, columnType, (uint)list.Count, list.Select(System.Convert.ToSingle));
                case ColumnType.Decimal:
                    return new DataSegmentBuffer<decimal>(context, columnType, (uint)list.Count, _ConvertIntegers(list, System.Convert.ToDecimal));
                case ColumnType.Long:
                    return new DataSegmentBuffer<long>(context, columnType, (uint)list.Count, _ConvertIntegers(list, System.Convert.ToInt64));
                case ColumnType.Int:
                    return new DataSegmentBuffer<int>(context, columnType, (uint)list.Count, _ConvertIntegers(list, System.Convert.ToInt32));
                case ColumnType.Short:
                    return new DataSegmentBuffer<short>(context, columnType, (uint)list.Count, _ConvertIntegers(list, System.Convert.ToInt16));
                case ColumnType.Byte:
                    return new DataSegmentBuffer<sbyte>(context, columnType, (uint)list.Count, _ConvertIntegers(list, System.Convert.ToSByte));
                default:
                    throw new Exception("Invalid column type for numeric");
            }
        }

        public IEnumerable<T> _ConvertIntegers<T>(IEnumerable<double> list, Func<double, T> converter)
        {
            return list.Select(v => double.IsNaN(v) ? converter(_invalidInteger) : converter(v));
        }
    }
}
