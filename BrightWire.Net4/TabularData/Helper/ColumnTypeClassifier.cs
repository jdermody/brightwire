using System.Collections.Generic;

namespace BrightWire.TabularData.Helper
{
    /// <summary>
    /// Classifies data table column types
    /// </summary>
    internal static class ColumnTypeClassifier
    {
        const int MAX_CATEGORICAL_VALUES = 256;

        static HashSet<ColumnType> _continuousType = new HashSet<ColumnType> {
            ColumnType.Date,
            ColumnType.Double,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Long,
            ColumnType.Byte,
        };
        static HashSet<ColumnType> _categoricalType = new HashSet<ColumnType> {
            ColumnType.Boolean,
            ColumnType.String,
        };
        static HashSet<ColumnType> _numericType = new HashSet<ColumnType> {
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
