using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using BrightData;

namespace BrightTable.Helper
{
    /// <summary>
    /// Classifies data table column types
    /// </summary>
    public static class ColumnTypeClassifier
    {
        static readonly HashSet<ColumnType> NumericType = new HashSet<ColumnType> {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Short,
            ColumnType.Long,
            ColumnType.Byte
        };
        static readonly HashSet<ColumnType> ContinuousType = new HashSet<ColumnType>(NumericType) {
            ColumnType.Date
        };
        static readonly HashSet<ColumnType> StructableType = new HashSet<ColumnType>(ContinuousType) {
            ColumnType.Boolean
        };
        static readonly HashSet<ColumnType> CategoricalType = new HashSet<ColumnType> {
            ColumnType.Boolean,
            ColumnType.String,
        };
        static readonly HashSet<ColumnType> DecimalType = new HashSet<ColumnType> {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float
        };

        public static bool IsDecimal(ColumnType columnType) => DecimalType.Contains(columnType);
        public static bool IsNumeric(ColumnType columnType) => NumericType.Contains(columnType);
        public static bool IsContinuous(ColumnType columnType) => ContinuousType.Contains(columnType);
        public static bool IsCategorical(ColumnType columnType) => CategoricalType.Contains(columnType);
        public static bool IsStructable(ColumnType columnType) => StructableType.Contains(columnType);

        public static ColumnClass GetClass(ColumnType type, IMetaData metaData)
        {
            var ret = ColumnClass.Unknown;
            if (metaData.IsCategorical() || IsCategorical(type))
                ret |= ColumnClass.Categorical;
            if (IsNumeric(type))
                ret |= ColumnClass.Numeric;
            if (IsDecimal(type))
                ret |= ColumnClass.Decimal;
            if (IsStructable(type))
                ret |= ColumnClass.Structable;
            if (type.IsTensor())
                ret |= ColumnClass.Tensor;
            if (type.IsIndexed())
                ret |= ColumnClass.IndexBased;
            if (type.IsContinuous())
                ret |= ColumnClass.Continuous;
            if (type.IsInteger())
                ret |= ColumnClass.Integer;
            return ret;
        }
    }
}
