using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BrightData.Buffer;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.Transformation
{
    /// <summary>
    /// Parameters that define a column conversion from one type to another
    /// </summary>
    internal class ColumnConversion : IColumnTransformationParam
    {
        class Converter<TF, TT> : ITransformColumn<TF, TT> where TF: notnull where TT: notnull
        {
            readonly Func<TF, TT> _converter;

            public Converter(Func<TF, TT> converter)
            {
                _converter = converter;
            }

            public bool Convert(TF input, IHybridBuffer<TT> buffer)
            {
                var converted = _converter(input);
                buffer.Add(converted);
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

        abstract class ConvertViaString<TF, TT> : ITransformColumn<TF, TT> where TF : notnull where TT : notnull
        {
            protected abstract TT Convert(string str);

            public bool Convert(TF input, IHybridBuffer<TT> buffer)
            {
                buffer.Add(Convert(input.ToString() ?? throw new Exception("String was null")));
                return true;
            }

            public virtual void Finalise(IMetaData metaData)
            {
                // nop
            }
            public Type From => typeof(TF);
            public Type To => typeof(TT);
        }

        class AnyToString<T> : ITransformColumn<T, string> where T : notnull
        {
            public bool Convert(T input, IHybridBuffer<string> buffer)
            {
                buffer.Add(input.ToString() ?? throw new Exception("String was null"));
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                // nop
            }
            public Type From => typeof(T);
            public Type To => typeof(string);
        }

        class NumericConverter<TF, TT> : ITransformColumn<TF, TT> where TF : notnull where TT : notnull
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

        class CategoricalIndexConverter<T> : ConvertViaString<T, int> where T : notnull
        {
            readonly Dictionary<string, int> _categoryIndex = new Dictionary<string, int>();

            protected override int Convert(string str)
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
        readonly ITransformColumn? _converter;

        static readonly HashSet<string> TrueStrings = new HashSet<string> { "Y", "YES", "TRUE", "T", "1" };
        static readonly ITransformColumn StringToBool = new Converter<string, bool>(str => TrueStrings.Contains(str.ToUpperInvariant()));
        static readonly ITransformColumn StringToDate = new Converter<string, DateTime>(ParseDate);
        static readonly ITransformColumn WeightedIndexListToIndexList = new Converter<WeightedIndexList, IndexList>(w => w.AsIndexList());
        static readonly ITransformColumn VectorToIndexList = new Converter<Vector<float>, IndexList>(v => v.Segment.ToSparse().AsIndexList());
        static readonly ITransformColumn IndexListToVector = new Converter<IndexList, Vector<float>>(v => v.ToDense(null));
        static readonly ITransformColumn WeightedIndexListToVector = new Converter<IndexList, Vector<float>>(v => v.ToDense(null));
        static readonly ITransformColumn IndexListToWeightedIndexList = new Converter<IndexList, WeightedIndexList>(indexList => indexList.Context.CreateWeightedIndexList(indexList.Indices.Select(ind => (ind, 1f))));
        static readonly ITransformColumn VectorToWeightedIndexList = new Converter<Vector<float>, WeightedIndexList>(v => v.Segment.ToSparse());

        static DateTime ParseDate(string str)
        {
            DateTime ret;

            if (DateTime.TryParse(str, out ret))
                return ret;

            if (DateTime.TryParse(str, new DateTimeFormatInfo(), DateTimeStyles.AllowWhiteSpaces, out ret))
                return ret;

            throw new Exception($"{str} was not recognised as a valid date");
        }

        public ColumnConversion(uint? columnIndex, ColumnConversionType type)
        {
            ColumnIndex = columnIndex;
            _toType = type;
            _converter = null;
        }

        public ColumnConversion(uint? columnIndex, ITransformColumn converter)
        {
            ColumnIndex = columnIndex;
            _converter = converter;
        }

        /// <inheritdoc />
        public uint? ColumnIndex { get; }

        /// <inheritdoc />
        public ITransformColumn? GetTransformer(ColumnType fromType, ISingleTypeTableSegment column, Func<IMetaData> analysedMetaData, IProvideTempStreams tempStreams, uint inMemoryRowCount)
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
                    return GenericActivator.Create<ITransformColumn>(typeof(AnyToString<>).MakeGenericType(fromType.GetDataType()));

                // to numeric
                case ColumnConversionType.ToNumeric: {
                    var buffer = tempStreams.CreateHybridStructBuffer<double>(inMemoryRowCount);
                    double min = double.MaxValue, max = double.MinValue;
                    var isInteger = true;
                    foreach (var val in column.Enumerate()) {
                        var str = val.ToString();
                        if (double.TryParse(str, out var num)) {
                            if (num < min)
                                min = num;
                            if (num > max)
                                max = num;
                            if (isInteger && Math.Abs(num % 1) > FloatMath.AlmostZero)
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
                
                    var enumerable = GetEnumerableNumbers(toType, buffer.EnumerateTyped());
                    var converterType = typeof(NumericConverter<,>).MakeGenericType(fromType.GetDataType(), toType.GetDataType());
                    return GenericActivator.Create<ITransformColumn>(converterType, enumerable);
                }

                // to categorical index
                case ColumnConversionType.ToCategoricalIndex:
                    return GenericActivator.Create<ITransformColumn>(typeof(CategoricalIndexConverter<>).MakeGenericType(fromType.GetDataType()));

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

        IEnumerable GetEnumerableNumbers(ColumnType toType, IEnumerable<double> numbers)
        {
            IEnumerable<T> ConvertIntegers<T>(IEnumerable<double> list, Func<double, T> converter)
            {
                return list.Select(v => double.IsNaN(v) ? converter(0) : converter(v));
            }

            return toType switch {
                ColumnType.Double => numbers,
                ColumnType.Float => numbers.Select(Convert.ToSingle),
                ColumnType.Decimal => ConvertIntegers(numbers, Convert.ToDecimal),
                ColumnType.Long => ConvertIntegers(numbers, Convert.ToInt64),
                ColumnType.Int => ConvertIntegers(numbers, Convert.ToInt32),
                ColumnType.Short => ConvertIntegers(numbers, Convert.ToInt16),
                ColumnType.Byte => ConvertIntegers(numbers, Convert.ToSByte),
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

        public static implicit operator ColumnConversion((uint Index, ITransformColumn Converter) column)
        {
            return new ColumnConversion(column.Index, column.Converter);
        }

        public static ColumnConversion Create<TF, TT>(uint index, Func<TF, TT> converter) where TF: notnull where TT: notnull => new ColumnConversion(index, new Converter<TF, TT>(converter));
    }
}
