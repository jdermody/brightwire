﻿using System;
using BrightData.Converter;
using BrightData.Helper;

namespace BrightData.Transformation
{
    /// <summary>
    /// Parameters that define how to normalize a column of a data table
    /// </summary>
    internal class ColumnNormalization : IColumnTransformationParam
    {
        readonly bool _toFloat;

        class NormalizeToFloat<T> : ITransformColumn<T, float>, INormalize where T: struct
        {
            readonly ICanConvert<T, double> _convertToDouble;
            readonly NormalizeTransformation _normalize;
            readonly ICanConvert<double, float> _convertToFloat;

            public NormalizeToFloat(NormalizationType type, IMetaData analysedMetaData)
            {
                _convertToDouble = StaticConverters.GetConverter<T, double>();
                _convertToFloat = StaticConverters.GetConverter<double, float>();
                _normalize = new NormalizeTransformation(type, analysedMetaData);
            }

            public bool Convert(T input, IHybridBuffer<float> buffer, uint index)
            {
                var asDouble = _convertToDouble.Convert(input);
                var normalized = _normalize.Normalize(asDouble);
                var val = _convertToFloat.Convert(normalized);

                buffer.Add(val, index);
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                var columnType = To.GetBrightDataType();
                if (columnType.IsNumeric())
                    metaData.Set(Consts.IsNumeric, true);
                _normalize.WriteTo(metaData);
            }

            public Type From => typeof(T);
            public Type To => typeof(float);

            public NormalizationType NormalizationType => _normalize.NormalizationType;
            public double Divide => _normalize.Divide;
            public double Subtract => _normalize.Subtract;
        }
        class NormalizeToDouble<T> : ITransformColumn<T, double>, INormalize where T: struct
        {
            readonly ICanConvert<T, double> _convertToDouble;
            readonly NormalizeTransformation _normalize;

            public NormalizeToDouble(NormalizationType type, IMetaData analysedMetaData)
            {
                _convertToDouble = StaticConverters.GetConverter<T, double>();
                _normalize = new NormalizeTransformation(type, analysedMetaData);
            }

            public bool Convert(T input, IHybridBuffer<double> buffer, uint index)
            {
                var asDouble = _convertToDouble.Convert(input);
                var normalized = _normalize.Normalize(asDouble);

                buffer.Add(normalized, index);
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                var columnType = To.GetBrightDataType();
                if (columnType.IsNumeric())
                    metaData.Set(Consts.IsNumeric, true);
                _normalize.WriteTo(metaData);
            }

            public Type From => typeof(T);
            public Type To => typeof(float);

            public NormalizationType NormalizationType => _normalize.NormalizationType;
            public double Divide => _normalize.Divide;
            public double Subtract => _normalize.Subtract;
        }

        public ITransformColumn GetTransformer(BrightDataType fromType, ISingleTypeTableSegment column, Func<IMetaData> analysedMetaData, IProvideTempStreams tempStreams, uint inMemoryRowCount)
        {
            var columnType = column.SingleType.GetDataType();
            var contextType = _toFloat
                ? typeof(NormalizeToFloat<>).MakeGenericType(columnType)
                : typeof(NormalizeToDouble<>).MakeGenericType(columnType)
            ;
            return GenericActivator.Create<ITransformColumn>(contextType, NormalizationType, analysedMetaData());
        }

        public uint? ColumnIndex { get; }
        public NormalizationType NormalizationType { get; }

        public ColumnNormalization(uint? index, NormalizationType type, bool toFloat = true)
        {
            _toFloat = toFloat;
            ColumnIndex = index;
            NormalizationType = type;
        }

        public static implicit operator ColumnNormalization(NormalizationType type)
        {
            return new(null, type);
        }

        public static implicit operator ColumnNormalization((uint Index, NormalizationType Type) column)
        {
            return new(column.Index, column.Type);
        }
    }
}
