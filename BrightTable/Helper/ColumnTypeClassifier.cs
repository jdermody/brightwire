using System;
using System.Collections.Generic;
using System.Text;

namespace BrightTable.Helper
{
    /// <summary>
    /// Classifies data table column types
    /// </summary>
    public static class ColumnTypeClassifier
    {
        static readonly HashSet<ColumnType> _continuousType = new HashSet<ColumnType> {
            ColumnType.Date,
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Short,
            ColumnType.Long,
            ColumnType.Byte,
        };
        static readonly HashSet<ColumnType> _categoricalType = new HashSet<ColumnType> {
            ColumnType.Boolean,
            ColumnType.String,
        };
        static readonly HashSet<ColumnType> _numericType = new HashSet<ColumnType> {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Short,
            ColumnType.Long,
            ColumnType.Byte
        };
        static readonly HashSet<ColumnType> _decimalType = new HashSet<ColumnType> {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float
        };
        static readonly HashSet<ColumnType> _structableType = new HashSet<ColumnType> {
            ColumnType.Boolean,
            ColumnType.Byte,
            ColumnType.Date,
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float,
            ColumnType.Short,
            ColumnType.Int,
            ColumnType.Long,
        };

        public static bool IsDecimal(ColumnType columnType) => _decimalType.Contains(columnType);
        public static bool IsNumeric(ColumnType columnType) => _numericType.Contains(columnType);
        public static bool IsContinuous(ColumnType columnType) => _continuousType.Contains(columnType);
        public static bool IsCategorical(ColumnType columnType) => _categoricalType.Contains(columnType);
        public static bool IsStructable(ColumnType columnType) => _structableType.Contains(columnType);
    }
}
