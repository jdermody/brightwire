using System.Collections.Generic;

namespace BrightData.Helper
{
    /// <summary>
    /// Classifies data table column types
    /// </summary>
    public static class ColumnTypeClassifier
    {
        static readonly HashSet<ColumnType> NumericType = new() {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float,
            ColumnType.Int,
            ColumnType.Short,
            ColumnType.Long,
            ColumnType.Byte
        };
        static readonly HashSet<ColumnType> ContinuousType = new(NumericType) {
            ColumnType.Date
        };
        static readonly HashSet<ColumnType> BlittableType = new(ContinuousType) {
            ColumnType.Boolean
        };
        static readonly HashSet<ColumnType> CategoricalType = new() {
            ColumnType.Boolean,
            ColumnType.String,
        };
        static readonly HashSet<ColumnType> DecimalType = new() {
            ColumnType.Double,
            ColumnType.Decimal,
            ColumnType.Float
        };

        /// <summary>
        /// Checks for a decimal type (floating point)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsDecimal(ColumnType columnType) => DecimalType.Contains(columnType);

        /// <summary>
        /// Checks for a numeric type (floating point or integer)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsNumeric(ColumnType columnType) => NumericType.Contains(columnType);

        /// <summary>
        /// Checks for a continuous type (non categorical)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsContinuous(ColumnType columnType) => ContinuousType.Contains(columnType);

        /// <summary>
        /// Checks for a categorical type (non continuous)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsCategorical(ColumnType columnType) => CategoricalType.Contains(columnType);

        /// <summary>
        /// Checks if the type has an independent memory layout across managed and unmanaged code
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsBlittable(ColumnType columnType) => BlittableType.Contains(columnType);

        /// <summary>
        /// Returns the set of possible column classifications
        /// </summary>
        /// <param name="type">Column type to check</param>
        /// <param name="metaData">Column metadata</param>
        /// <returns></returns>
        public static ColumnClass GetClass(ColumnType type, IMetaData metaData)
        {
            var ret = ColumnClass.Unknown;
            if (metaData.IsCategorical() || IsCategorical(type))
                ret |= ColumnClass.Categorical;
            if (IsNumeric(type))
                ret |= ColumnClass.Numeric;
            if (IsDecimal(type))
                ret |= ColumnClass.Decimal;
            if (IsBlittable(type))
                ret |= ColumnClass.Structable;
            if (type.IsTensor())
                ret |= ColumnClass.Tensor;
            if (type.IsIndexedList())
                ret |= ColumnClass.IndexBased;
            if (type.IsContinuous())
                ret |= ColumnClass.Continuous;
            if (type.IsInteger())
                ret |= ColumnClass.Integer;
            return ret;
        }
    }
}
