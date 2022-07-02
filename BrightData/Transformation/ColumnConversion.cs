using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BrightData.Buffer.Hybrid;
using BrightData.Converter;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.Transformation
{
    /// <summary>
    /// Parameters that define a column conversion from one type to another
    /// </summary>
    internal class ColumnConversion : IColumnTransformationParam
    {
        class Converter<TF, TT> : IConvertColumn<TF, TT> where TF: notnull where TT: notnull
        {
            readonly Func<TF, TT> _converter;

            public Converter(Func<TF, TT> converter)
            {
                _converter = converter;
            }

            public bool Convert(TF input, IHybridBuffer<TT> buffer, uint index)
            {
                var converted = _converter(input);
                buffer.Add(converted);
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                var columnType = To.GetBrightDataType();
                if (columnType.IsNumeric())
                    metaData.Set(Consts.IsNumeric, true);
            }

            public Type From { get; } = typeof(TF);
            public Type To { get; } = typeof(TT);
        }

        abstract class ConvertViaString<TF, TT> : IConvertColumn<TF, TT> where TF : notnull where TT : notnull
        {
            protected abstract TT Convert(string str);

            public bool Convert(TF input, IHybridBuffer<TT> buffer, uint index)
            {
                buffer.Add(Convert(input.ToString() ?? throw new Exception("String was null")));
                return true;
            }

            public virtual void Finalise(IMetaData metaData)
            {
                // nop
            }
            public Type From { get; } = typeof(TF);
            public Type To { get; } = typeof(TT);
        }

        class AnyToString<T> : IConvertColumn<T, string> where T : notnull
        {
            public bool Convert(T input, IHybridBuffer<string> buffer, uint index)
            {
                buffer.Add(input.ToString() ?? throw new Exception("String was null"));
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                // nop
            }
            public Type From { get; } = typeof(T);
            public Type To { get; } = typeof(string);
        }

        class NumericConverter<TF, TT> : IConvertColumn<TF, TT> where TF : notnull where TT : notnull
        {
            readonly IEnumerator<TT> _list;

            public NumericConverter(IEnumerable<TT> data)
            {
                _list = data.GetEnumerator();
            }

            public Type From { get; } = typeof(TF);
            public Type To { get; } = typeof(TT);
            public bool Convert(TF input, IHybridBuffer<TT> buffer, uint index)
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
            readonly Dictionary<string, int> _categoryIndex = new();

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
                metaData.SetType(BrightDataType.Int);
                metaData.SetIsCategorical(true);

                foreach (var category in _categoryIndex.OrderBy(d => d.Value))
                    metaData.Set(Consts.CategoryPrefix + category.Value, category.Key);
            }
        }

        class StringTokeniser : IConvertColumn<string, IndexList>
        {
            readonly Func<string, StringIndexer, IEnumerable<uint>> _tokeniser;
            readonly StringIndexer _stringIndexer = new();

            public StringTokeniser(Func<string, StringIndexer, IEnumerable<uint>> tokeniser)
            {
                _tokeniser = tokeniser;
            }

            public bool Convert(string input, IHybridBuffer<IndexList> buffer, uint index)
            {
                var indexList = IndexList.Create(_tokeniser(input, _stringIndexer));
                buffer.Add(indexList);
                return true;
            }

            public Type From { get; } = typeof(string);
            public Type To { get; } = typeof(IndexList);
            public void Finalise(IMetaData metaData)
            {
                metaData.SetType(BrightDataType.IndexList);
                metaData.SetIsCategorical(true);

                uint index = 0;
                foreach (var str in _stringIndexer.OrderedStrings)
                    metaData.Set($"{Consts.CategoryPrefix}{index++}", str);
            }
        }

        class StandardConverter<TF, TT> : IConvertColumn<TF, TT> where TF : notnull where TT : notnull
        {
            readonly ICanConvert<TF, TT> _converter;

            public StandardConverter()
            {
                _converter = StaticConverters.GetConverter<TF, TT>();
            }

            public bool Convert(TF input, IHybridBuffer<TT> buffer, uint index)
            {
                buffer.Add(_converter.Convert(input));
                return true;
            }

            public Type From { get; } = typeof(TF);
            public Type To { get; } = typeof(TT);
            public void Finalise(IMetaData metaData)
            {
                metaData.Set(Consts.IsNumeric, true);
            }
        }

        public class CustomConverter<TF, TT> : IConvertColumn<TF, TT> where TF : notnull where TT : notnull
        {
            readonly Func<TF, TT> _converter;
            readonly Action<IMetaData>? _finalise;

            public CustomConverter(Func<TF, TT> converter, Action<IMetaData>? finalise)
            {
                _converter = converter;
                _finalise = finalise;
            }

            public bool Convert(TF input, IHybridBuffer<TT> buffer, uint index)
            {
                buffer.Add(_converter(input));
                return true;
            }

            public Type From => typeof(TF);
            public Type To => typeof(TT);
            public void Finalise(IMetaData metaData) => _finalise?.Invoke(metaData);
        }

        readonly ColumnConversionType _toType;
        readonly IConvertColumn? _converter;

        static readonly HashSet<string> TrueStrings = new() { "Y", "YES", "TRUE", "T", "1" };
        static readonly IConvertColumn StringToBool = new Converter<string, bool>(str => TrueStrings.Contains(str.ToUpperInvariant()));
        static readonly IConvertColumn StringToDate = new Converter<string, DateTime>(ParseDate);
        static readonly IConvertColumn WeightedIndexListToIndexList = new Converter<WeightedIndexList, IndexList>(w => w.AsIndexList());
        static readonly IConvertColumn IndexListToWeightedIndexList = new Converter<IndexList, WeightedIndexList>(indexList => WeightedIndexList.Create(indexList.Indices.Select(ind => (ind, 1f))));

        static DateTime ParseDate(string str)
        {
            if (DateTime.TryParse(str, out var ret))
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

        public ColumnConversion(uint? columnIndex, IConvertColumn converter)
        {
            ColumnIndex = columnIndex;
            _converter = converter;
        }

        /// <inheritdoc />
        public uint? ColumnIndex { get; }

        /// <inheritdoc />
        public IConvertColumn? GetTransformer(BrightDataContext context, BrightDataType fromType, ISingleTypeTableSegment column, Func<IMetaData> analysedMetaData, IProvideTempStreams tempStreams, uint inMemoryRowCount)
        {
            if (_converter != null)
                return _converter;

            var lap = context.LinearAlgebraProvider2;
            switch (_toType) {
                case ColumnConversionType.Unchanged:
                    return null;

                // to boolean
                case ColumnConversionType.ToBoolean when fromType == BrightDataType.Boolean:
                    return null;
                case ColumnConversionType.ToBoolean when fromType == BrightDataType.String:
                    return StringToBool;

                // to date
                case ColumnConversionType.ToDate when fromType == BrightDataType.Date:
                    return null;
                case ColumnConversionType.ToDate when fromType == BrightDataType.String:
                    return StringToDate;

                // to string
                case ColumnConversionType.ToString when fromType == BrightDataType.String:
                    return null;
                case ColumnConversionType.ToString:
                    return GenericActivator.Create<IConvertColumn>(typeof(AnyToString<>).MakeGenericType(fromType.GetDataType()));

                // to numeric
                case ColumnConversionType.ToNumeric: {
                    var buffer = tempStreams.CreateHybridStructBuffer<double>(inMemoryRowCount);
                    double min = double.MaxValue, max = double.MinValue;
                    var isInteger = true;
                    uint index = 0;
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

                        ++index;
                    }
                    BrightDataType toType;
                    if (isInteger) {
                        if (min >= sbyte.MinValue && max <= sbyte.MaxValue)
                            toType = BrightDataType.SByte;
                        else if (min >= short.MinValue && max <= short.MaxValue)
                            toType = BrightDataType.Short;
                        else if (min >= int.MinValue && max <= int.MaxValue)
                            toType = BrightDataType.Int;
                        else
                            toType = BrightDataType.Long;
                    } else {
                        if (min >= float.MinValue && max <= float.MaxValue)
                            toType = BrightDataType.Float;
                        else
                            toType = BrightDataType.Double;
                    }
                
                    var enumerable = GetEnumerableNumbers(toType, buffer.EnumerateTyped());
                    var converterType = typeof(NumericConverter<,>).MakeGenericType(fromType.GetDataType(), toType.GetDataType());
                    return GenericActivator.Create<IConvertColumn>(converterType, enumerable);
                }

                case ColumnConversionType.ToByte:
                    return GetStandardConverter(fromType, BrightDataType.SByte);
                case ColumnConversionType.ToShort:
                    return GetStandardConverter(fromType, BrightDataType.Short);
                case ColumnConversionType.ToInt:
                    return GetStandardConverter(fromType, BrightDataType.Int);
                case ColumnConversionType.ToLong:
                    return GetStandardConverter(fromType, BrightDataType.Long);
                case ColumnConversionType.ToFloat:
                    return GetStandardConverter(fromType, BrightDataType.Float);
                case ColumnConversionType.ToDouble:
                    return GetStandardConverter(fromType, BrightDataType.Double);
                case ColumnConversionType.ToDecimal:
                    return GetStandardConverter(fromType, BrightDataType.Decimal);

                // to categorical index
                case ColumnConversionType.ToCategoricalIndex:
                    return GenericActivator.Create<IConvertColumn>(typeof(CategoricalIndexConverter<>).MakeGenericType(fromType.GetDataType()));

                // index list
                case ColumnConversionType.ToIndexList when fromType == BrightDataType.IndexList:
                    return null;
                case ColumnConversionType.ToIndexList when fromType == BrightDataType.WeightedIndexList:
                    return WeightedIndexListToIndexList;
                case ColumnConversionType.ToIndexList when fromType == BrightDataType.Vector:
                    return new Converter<IVector, IndexList>(v => v.Segment.ToSparse(context).AsIndexList());

                // vector
                case ColumnConversionType.ToVector when fromType == BrightDataType.Vector:
                    return null;
                case ColumnConversionType.ToVector when fromType == BrightDataType.WeightedIndexList:
                    return new Converter<IndexList, IVector>(v => v.AsDense(lap));
                case ColumnConversionType.ToVector when fromType == BrightDataType.IndexList:
                    return new Converter<IndexList, IVector>(v => v.AsDense(lap));

                // weighted index list
                case ColumnConversionType.ToWeightedIndexList when fromType == BrightDataType.WeightedIndexList:
                    return null;
                case ColumnConversionType.ToWeightedIndexList when fromType == BrightDataType.IndexList:
                    return IndexListToWeightedIndexList;
                case ColumnConversionType.ToWeightedIndexList when fromType == BrightDataType.Vector:
                    return new Converter<IVector, WeightedIndexList>(v => v.Segment.ToSparse(context));

                default:
                    throw new Exception($"Converting from {fromType} to {_toType} is not supported");
            }
        }

        static IConvertColumn GetStandardConverter(BrightDataType fromType, BrightDataType toType)
        {
            var converterType = typeof(StandardConverter<,>).MakeGenericType(fromType.GetDataType(), toType.GetDataType());
            return GenericActivator.Create<IConvertColumn>(converterType);
        }

        static IEnumerable GetEnumerableNumbers(BrightDataType toType, IEnumerable<double> numbers)
        {
            IEnumerable<T> ConvertIntegers<T>(IEnumerable<double> list, Func<double, T> converter)
            {
                return list.Select(v => double.IsNaN(v) ? converter(0) : converter(v));
            }

            return toType switch {
                BrightDataType.Double  => numbers,
                BrightDataType.Float   => numbers.Select(Convert.ToSingle),
                BrightDataType.Decimal => ConvertIntegers(numbers, Convert.ToDecimal),
                BrightDataType.Long    => ConvertIntegers(numbers, Convert.ToInt64),
                BrightDataType.Int     => ConvertIntegers(numbers, Convert.ToInt32),
                BrightDataType.Short   => ConvertIntegers(numbers, Convert.ToInt16),
                BrightDataType.SByte   => ConvertIntegers(numbers, Convert.ToSByte),
                _                      => throw new Exception("Invalid column type for numeric")
            };
        }

        public static implicit operator ColumnConversion(ColumnConversionType type)
        {
            return new(null, type);
        }

        public static implicit operator ColumnConversion((uint Index, ColumnConversionType Type) column)
        {
            return new(column.Index, column.Type);
        }

        public static implicit operator ColumnConversion((uint Index, IConvertColumn Converter) column)
        {
            return new(column.Index, column.Converter);
        }

        public static ColumnConversion Create<TF, TT>(uint index, Func<TF, TT> converter) where TF: notnull where TT: notnull => new(index, new Converter<TF, TT>(converter));
    }
}
