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
        static readonly HashSet<ColumnType> _numericType = new HashSet<ColumnType> {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Short,
            ColumnType.Long,
            ColumnType.Byte
        };
        static readonly HashSet<ColumnType> _continuousType = new HashSet<ColumnType>(_numericType) {
            ColumnType.Date
        };
        static readonly HashSet<ColumnType> _structableType = new HashSet<ColumnType>(_continuousType) {
            ColumnType.Boolean
        };
        static readonly HashSet<ColumnType> _categoricalType = new HashSet<ColumnType> {
            ColumnType.Boolean,
            ColumnType.String,
        };
        static readonly HashSet<ColumnType> _decimalType = new HashSet<ColumnType> {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float
        };

        public static bool IsDecimal(ColumnType columnType) => _decimalType.Contains(columnType);
        public static bool IsNumeric(ColumnType columnType) => _numericType.Contains(columnType);
        public static bool IsContinuous(ColumnType columnType) => _continuousType.Contains(columnType);
        public static bool IsCategorical(ColumnType columnType) => _categoricalType.Contains(columnType);
        public static bool IsStructable(ColumnType columnType) => _structableType.Contains(columnType);

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
