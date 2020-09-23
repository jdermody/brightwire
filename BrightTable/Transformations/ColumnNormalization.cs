using System;
using System.Collections.Generic;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightData.Transformation;

namespace BrightTable.Transformations
{
    public class ColumnNormalization : IColumnTransformationParam
    {
        public class Normalizer<T> : IConvert<T, T>, INormalize where T: struct
        {
            private readonly ICanConvert<T, double> _convertToDouble;
            private readonly NormalizeTransformation _normalize;
            private readonly ICanConvert<double, T> _convertBack;

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
            }

            public Type From => typeof(T);
            public Type To => typeof(T);

            public NormalizationType NormalizationType => _normalize.NormalizationType;
            public double Divide => _normalize.Divide;
            public double Subtract => _normalize.Subtract;
        }

        public ICanConvert GetConverter(ColumnType fromType, ISingleTypeTableSegment column, TempStreamManager tempStreams, uint inMemoryRowCount)
        {
            var columnType = ExtensionMethods.GetDataType(column.SingleType);
            var contextType = typeof(Normalizer<>).MakeGenericType(columnType);
            var analysedMetaData = column.Analyse();
            return (ICanConvert) Activator.CreateInstance(contextType, NormalizationType, analysedMetaData);
        }

        public uint? Index { get; }
        public NormalizationType NormalizationType { get; }

        public ColumnNormalization(uint? index, NormalizationType type)
        {
            Index = index;
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
