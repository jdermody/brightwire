using System.Collections.Generic;

namespace BrightWire.TabularData.Helper
{
    /// <summary>
    /// Classifies data table column types
    /// </summary>
    static class ColumnTypeClassifier
    {
        static readonly HashSet<ColumnType> _continuousType = new HashSet<ColumnType> {
            ColumnType.Date,
            ColumnType.Double,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Long,
            ColumnType.Byte,
        };
        static readonly HashSet<ColumnType> _categoricalType = new HashSet<ColumnType> {
            ColumnType.Boolean,
            ColumnType.String,
        };
        static readonly HashSet<ColumnType> _numericType = new HashSet<ColumnType> {
            ColumnType.Boolean,
            ColumnType.Date,
            ColumnType.Double,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Long,
            ColumnType.Byte
        };

        public static bool IsNumeric(IColumn column)
        {
            return _numericType.Contains(column.Type);
        }
        public static bool IsContinuous(IColumn column)
        {
            return _continuousType.Contains(column.Type);
        }
        public static bool IsCategorical(IColumn column)
        {
            return _categoricalType.Contains(column.Type);
        }
    }
}
