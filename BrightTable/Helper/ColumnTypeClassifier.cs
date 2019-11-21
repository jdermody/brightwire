using System;
using System.Collections.Generic;
using System.Text;

namespace BrightTable.Helper
{
    /// <summary>
    /// Classifies data table column types
    /// </summary>
    static class ColumnTypeClassifier
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

        public static bool IsDecimal(ColumnType columnType)
        {
            return _decimalType.Contains(columnType);
        }

        public static bool IsNumeric(ColumnType columnType)
        {
            return _numericType.Contains(columnType);
        }

        public static bool IsContinuous(ColumnType columnType)
        {
            return _continuousType.Contains(columnType);
        }

        public static bool IsCategorical(ColumnType columnType)
        {
            return _categoricalType.Contains(columnType);
        }
    }
}
