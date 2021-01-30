using System;
using BrightData;
using BrightData.Helper;
using BrightData.Transformation;

namespace BrightTable.Transformations
{
    /// <summary>
    /// Parameters that define how to normalize a column of a data table
    /// </summary>
    internal class ColumnNormalization : IColumnTransformationParam
    {
        class Normalizer<T> : ITransformColumn<T, T>, INormalize where T: struct
        {
            readonly ICanConvert<T, double> _convertToDouble;
            readonly NormalizeTransformation _normalize;
            readonly ICanConvert<double, T> _convertBack;

            public Normalizer(NormalizationType type, IMetaData analysedMetaData)
            {
                _convertToDouble = (ICanConvert<T, double>) typeof(double).GetConverter<T>();
                _convertBack = (ICanConvert<double, T>)typeof(T).GetConverter<double>();
                _normalize = new NormalizeTransformation(type, analysedMetaData);
            }

            public bool Convert(T input, IHybridBuffer<T> buffer)
            {
                var asDouble = _convertToDouble.Convert(input);
                var normalized = _normalize.Normalize(asDouble);
                var val = _convertBack.Convert(normalized);

                buffer.Add(val);
                return true;
            }

            public void Finalise(IMetaData metaData)
            {
                var columnType = To.GetColumnType();
                if (columnType.IsNumeric())
                    metaData.Set(Consts.IsNumeric, true);
                _normalize.WriteTo(metaData);
            }

            public Type From => typeof(T);
            public Type To => typeof(T);

            public NormalizationType NormalizationType => _normalize.NormalizationType;
            public double Divide => _normalize.Divide;
            public double Subtract => _normalize.Subtract;
        }

        public ITransformColumn? GetTransformer(ColumnType fromType, ISingleTypeTableSegment column, IProvideTempStreams tempStreams, uint inMemoryRowCount)
        {
            var columnType = column.SingleType.GetDataType();
            var contextType = typeof(Normalizer<>).MakeGenericType(columnType);
            var analysedMetaData = column.Analyse();
            return GenericActivator.Create<ITransformColumn>(contextType, NormalizationType, analysedMetaData);
        }

        public uint? ColumnIndex { get; }
        public NormalizationType NormalizationType { get; }

        public ColumnNormalization(uint? index, NormalizationType type)
        {
            ColumnIndex = index;
            NormalizationType = type;
        }

        public static implicit operator ColumnNormalization(NormalizationType type)
        {
            return new ColumnNormalization(null, type);
        }

        public static implicit operator ColumnNormalization((uint Index, NormalizationType Type) column)
        {
            return new ColumnNormalization(column.Index, column.Type);
        }
    }
}
