using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Buffers;
using BrightData.Helper;
using BrightTable.Buffers;
using BrightTable.Input;

namespace BrightTable.Transformations
{
    public class ColumnConversion : IColumnTransformationParam
    {
        public class Converter<TF, TT> : IConvert<TF, TT>
        {
            private readonly Func<TF, TT> _converter;

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
            private readonly IEnumerator<TT> _list;

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

            public CategoricalIndexConverter()
            {
            }

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
                foreach (var category in _categoryIndex.OrderBy(d => d.Value))
                    metaData.Set("category:" + category.Value, category.Key);
            }
        }

        private readonly uint? _columnIndex;
        private readonly ColumnConversionType _toType;
        private readonly ICanConvert _converter;

        private static readonly HashSet<string> TrueStrings = new HashSet<string> { "Y", "YES", "TRUE", "T" };
        private static readonly ICanConvert StringToBool = new Converter<string, bool>(str => TrueStrings.Contains(str));
        private static readonly ICanConvert StringToDate = new Converter<string, DateTime>(DateTime.Parse);
        private static readonly ICanConvert WeightedIndexListToIndexList = new Converter<WeightedIndexList, IndexList>(w => w.AsIndexList());
        private static readonly ICanConvert VectorToIndexList = new Converter<Vector<float>, IndexList>(v => v.Segment.ToSparse().AsIndexList());
        private static readonly ICanConvert IndexListToVector = new Converter<IndexList, Vector<float>>(v => v.ToDense());
        private static readonly ICanConvert WeightedIndexListToVector = new Converter<IndexList, Vector<float>>(v => v.ToDense());
        private static readonly ICanConvert IndexListToWeightedIndexList = new Converter<IndexList, WeightedIndexList>(indexList => WeightedIndexList.Create(indexList.Context, indexList.Indices.Select(ind => new WeightedIndexList.Item(ind, 1f)).ToArray()));
        private static readonly ICanConvert VectorToWeightedIndexList = new Converter<Vector<float>, WeightedIndexList>(v => v.Segment.ToSparse());

        public ColumnConversion(uint? columnIndex, ColumnConversionType type)
        {
            _columnIndex = columnIndex;
            _toType = type;
            _converter = null;
        }

        public ColumnConversion(uint? columnIndex, ICanConvert converter)
        {
            _columnIndex = columnIndex;
            _converter = converter;
        }

        public uint? Index => _columnIndex;

        public ICanConvert GetConverter(ColumnType fromType, ISingleTypeTableSegment column, TempStreamManager tempStreams, uint inMemoryRowCount)
        {
            ICanConvert ViaString<T>(Func<string, T> convertFromString)
            {
                var t = typeof(ConvertViaString<,>).MakeGenericType(ExtensionMethods.GetDataType(fromType), typeof(T));
                var ret = Activator.CreateInstance(t, new object[] { convertFromString });
                return (ICanConvert)ret;
            }

            if (_converter != null)
                return _converter;

            switch (_toType) {
                case ColumnConversionType.Unchanged:
                    return null;
                case ColumnConversionType.ToBoolean when fromType == ColumnType.Boolean:
                    return null;
                case ColumnConversionType.ToBoolean when fromType == ColumnType.String:
                    return StringToBool;
                case ColumnConversionType.ToDate when fromType == ColumnType.Date:
                    return null;
                case ColumnConversionType.ToDate when fromType == ColumnType.String:
                    return StringToDate;
                case ColumnConversionType.ToString when fromType == ColumnType.String:
                    return null;
                case ColumnConversionType.ToString: {
                    var t = typeof(AnyToString<>).MakeGenericType(ExtensionMethods.GetDataType(fromType));
                    return (ICanConvert)Activator.CreateInstance(t);
                }
                case ColumnConversionType.ToNumeric: {
                    var buffer = new StructHybridBuffer<double>(tempStreams, inMemoryRowCount, 1024);
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
                    var converterType = typeof(NumericConverter<,>).MakeGenericType(ExtensionMethods.GetDataType(fromType), ExtensionMethods.GetDataType(toType));
                    var converter = Activator.CreateInstance(converterType, enumerable);
                    return (ICanConvert) converter;
                }
                case ColumnConversionType.ToCategoricalIndex: {
                    var converterType = typeof(CategoricalIndexConverter<>).MakeGenericType(ExtensionMethods.GetDataType(fromType));
                    var converter = Activator.CreateInstance(converterType);
                    return (ICanConvert)converter;
                }
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
            switch (toType) {
                case ColumnType.Double:
                    return numbers;
                case ColumnType.Float:
                    return numbers.Select(System.Convert.ToSingle);
                case ColumnType.Decimal:
                    return _ConvertIntegers(numbers, System.Convert.ToDecimal);
                case ColumnType.Long:
                    return _ConvertIntegers(numbers, System.Convert.ToInt64);
                case ColumnType.Int:
                    return _ConvertIntegers(numbers, System.Convert.ToInt32);
                case ColumnType.Short:
                    return _ConvertIntegers(numbers, System.Convert.ToInt16);
                case ColumnType.Byte:
                    return _ConvertIntegers(numbers, System.Convert.ToSByte);
                default:
                    throw new Exception("Invalid column type for numeric");
            }
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
