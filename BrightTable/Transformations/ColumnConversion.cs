using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.Buffers;
using BrightData.Helper;

namespace BrightTable.Transformations
{
    /// <summary>
    /// Parameters that define a column conversion from one type to another
    /// </summary>
    public class ColumnConversion : IColumnTransformationParam
    {
        public class Converter<TF, TT> : IConvert<TF, TT>
        {
            readonly Func<TF, TT> _converter;

            public Converter(Func<TF, TT> converter)
            {
                _converter = converter;
            }

            public bool Convert(TF input, IHybridBuffer<TT> buffer)
            {
                buffer.Add(_converter(input));
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                var columnType = To.GetColumnType();
                if (columnType.IsNumeric())
                    metaData.Set(Consts.IsNumeric, true);
            }

            public Type From => typeof(TF);
            public Type To => typeof(TT);
        }

        abstract class ConvertViaString<TF, TT> : IConvert<TF, TT>
        {
            protected abstract TT _Convert(string str);

            public bool Convert(TF input, IHybridBuffer<TT> buffer)
            {
                buffer.Add(_Convert(input.ToString()));
                return true;
            }

            public virtual void Finalise(IMetaData metaData)
            {
                // nop
            }
            public Type From => typeof(TF);
            public Type To => typeof(TT);
        }

        class AnyToString<T> : IConvert<T, string>
        {
            public bool Convert(T input, IHybridBuffer<string> buffer)
            {
                buffer.Add(input.ToString());
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                // nop
            }
            public Type From => typeof(T);
            public Type To => typeof(string);
        }

        class NumericConverter<TF, TT> : IConvert<TF, TT>
        {
            readonly IEnumerator<TT> _list;

            public NumericConverter(IEnumerable<TT> data)
            {
                _list = data.GetEnumerator();
            }

            public Type From => typeof(TF);
            public Type To => typeof(TT);
            public bool Convert(TF input, IHybridBuffer<TT> buffer)
            {
                if (_list.MoveNext()) {
                    buffer.Add(_list.Current);
                    return true;
                }

                return false;
            }

            public void Finalise(IMetaData metaData)
            {
                metaData.Set(Consts.IsNumeric, true);
            }
        }

        class CategoricalIndexConverter<T> : ConvertViaString<T, int>
        {
            readonly Dictionary<string, int> _categoryIndex = new Dictionary<string, int>();

            protected override int _Convert(string str)
            {
                if (_categoryIndex.TryGetValue(str, out var index))
                    return index;
                _categoryIndex.Add(str, index = _categoryIndex.Count);
                return index;
            }

            public override void Finalise(IMetaData metaData)
            {
                metaData.Set(Consts.IsNumeric, true);
                metaData.SetType(ColumnType.Int);

                foreach (var category in _categoryIndex.OrderBy(d => d.Value))
                    metaData.Set(Consts.CategoryPrefix + category.Value, category.Key);
            }
        }

        readonly ColumnConversionType _toType;
        readonly ICanConvert? _converter;

        static readonly HashSet<string> TrueStrings = new HashSet<string> { "Y", "YES", "TRUE", "T", "1" };
        static readonly ICanConvert StringToBool = new Converter<string, bool>(str => TrueStrings.Contains(str));
        static readonly ICanConvert StringToDate = new Converter<string, DateTime>(DateTime.Parse);
        static readonly ICanConvert WeightedIndexListToIndexList = new Converter<WeightedIndexList, IndexList>(w => w.AsIndexList());
        static readonly ICanConvert VectorToIndexList = new Converter<Vector<float>, IndexList>(v => v.Segment.ToSparse().AsIndexList());
        static readonly ICanConvert IndexListToVector = new Converter<IndexList, Vector<float>>(v => v.ToDense(null));
        static readonly ICanConvert WeightedIndexListToVector = new Converter<IndexList, Vector<float>>(v => v.ToDense(null));
        static readonly ICanConvert IndexListToWeightedIndexList = new Converter<IndexList, WeightedIndexList>(indexList => indexList.Context.CreateWeightedIndexList(indexList.Indices.Select(ind => (ind, 1f))));
        static readonly ICanConvert VectorToWeightedIndexList = new Converter<Vector<float>, WeightedIndexList>(v => v.Segment.ToSparse());

        public ColumnConversion(uint? columnIndex, ColumnConversionType type)
        {
            ColumnIndex = columnIndex;
            _toType = type;
            _converter = null;
        }

        public ColumnConversion(uint? columnIndex, ICanConvert converter)
        {
            ColumnIndex = columnIndex;
            _converter = converter;
        }

        public uint? ColumnIndex { get; }

        public ICanConvert? GetConverter(ColumnType fromType, ISingleTypeTableSegment column, IProvideTempStreams tempStreams, uint inMemoryRowCount)
        {
            if (_converter != null)
                return _converter;

            switch (_toType) {
                case ColumnConversionType.Unchanged:
                    return null;

                // to boolean
                case ColumnConversionType.ToBoolean when fromType == ColumnType.Boolean:
                    return null;
                case ColumnConversionType.ToBoolean when fromType == ColumnType.String:
                    return StringToBool;

                // to date
                case ColumnConversionType.ToDate when fromType == ColumnType.Date:
                    return null;
                case ColumnConversionType.ToDate when fromType == ColumnType.String:
                    return StringToDate;

                // to string
                case ColumnConversionType.ToString when fromType == ColumnType.String:
                    return null;
                case ColumnConversionType.ToString:
                    return GenericActivator.Create<ICanConvert>(typeof(AnyToString<>).MakeGenericType(fromType.GetDataType()));

                // to numeric
                case ColumnConversionType.ToNumeric: {
                    var buffer = tempStreams.CreateHybridStructBuffer<double>(inMemoryRowCount, 1024);
                    double min = double.MaxValue, max = double.MinValue;
                    var isInteger = true;
                    foreach (var val in column.Enumerate()) {
                        var str = val.ToString();
                        if (double.TryParse(str, out var num)) {
                            if (num < min)
                                min = num;
                            if (num > max)
                                max = num;
                            if (isInteger && Math.Abs(num - Math.Floor(num)) > FloatMath.AlmostZero)
                                isInteger = false;
                            buffer.Add(num);
                        } else
                            buffer.Add(double.NaN);
                    }
                    ColumnType toType;
                    if (isInteger) {
                        if (min >= sbyte.MinValue && max <= sbyte.MaxValue)
                            toType = ColumnType.Byte;
                        else if (min >= short.MinValue && max <= short.MaxValue)
                            toType = ColumnType.Short;
                        else if (min >= int.MinValue && max <= int.MaxValue)
                            toType = ColumnType.Int;
                        else
                            toType = ColumnType.Long;
                    } else {
                        if (min >= float.MinValue && max <= float.MaxValue)
                            toType = ColumnType.Float;
                        else
                            toType = ColumnType.Double;
                    }
                
                    var enumerable = _GetEnumerableNumbers(toType, buffer.EnumerateTyped());
                    var converterType = typeof(NumericConverter<,>).MakeGenericType(fromType.GetDataType(), toType.GetDataType());
                    return GenericActivator.Create<ICanConvert>(converterType, enumerable);
                }

                // to categorical index
                case ColumnConversionType.ToCategoricalIndex:
                    return GenericActivator.Create<ICanConvert>(typeof(CategoricalIndexConverter<>).MakeGenericType(fromType.GetDataType()));

                case ColumnConversionType.ToIndexList when fromType == ColumnType.IndexList:
                    return null;
                case ColumnConversionType.ToIndexList when fromType == ColumnType.WeightedIndexList:
                    return WeightedIndexListToIndexList;
                case ColumnConversionType.ToIndexList when fromType == ColumnType.Vector:
                    return VectorToIndexList;

                case ColumnConversionType.ToVector when fromType == ColumnType.Vector:
                    return null;
                case ColumnConversionType.ToVector when fromType == ColumnType.WeightedIndexList:
                    return WeightedIndexListToVector;
                case ColumnConversionType.ToVector when fromType == ColumnType.IndexList:
                    return IndexListToVector;

                case ColumnConversionType.ToWeightedIndexList when fromType == ColumnType.WeightedIndexList:
                    return null;
                case ColumnConversionType.ToWeightedIndexList when fromType == ColumnType.IndexList:
                    return IndexListToWeightedIndexList;
                case ColumnConversionType.ToWeightedIndexList when fromType == ColumnType.Vector:
                    return VectorToWeightedIndexList;

                default:
                    throw new Exception($"Converting from {fromType} to {_toType} is not supported");
            }
        }

        IEnumerable _GetEnumerableNumbers(ColumnType toType, IEnumerable<double> numbers)
        {
            IEnumerable<T> _ConvertIntegers<T>(IEnumerable<double> list, Func<double, T> converter)
            {
                return list.Select(v => double.IsNaN(v) ? converter(0) : converter(v));
            }

            return toType switch {
                ColumnType.Double => numbers,
                ColumnType.Float => numbers.Select(Convert.ToSingle),
                ColumnType.Decimal => _ConvertIntegers(numbers, Convert.ToDecimal),
                ColumnType.Long => _ConvertIntegers(numbers, Convert.ToInt64),
                ColumnType.Int => _ConvertIntegers(numbers, Convert.ToInt32),
                ColumnType.Short => _ConvertIntegers(numbers, Convert.ToInt16),
                ColumnType.Byte => _ConvertIntegers(numbers, Convert.ToSByte),
                _ => throw new Exception("Invalid column type for numeric")
            };
        }

        public static implicit operator ColumnConversion(ColumnConversionType type)
        {
            return new ColumnConversion(null, type);
        }

        public static implicit operator ColumnConversion((uint Index, ColumnConversionType Type) column)
        {
            return new ColumnConversion(column.Index, column.Type);
        }

        public static implicit operator ColumnConversion((uint Index, ICanConvert Converter) column)
        {
            return new ColumnConversion(column.Index, column.Converter);
        }

        public static ColumnConversion Create<TF, TT>(uint index, Func<TF, TT> converter) => new ColumnConversion(index, new Converter<TF, TT>(converter));
    }
}
