using System.Collections.Generic;

namespace BrightData.Helper
{
    /// <summary>
    /// Classifies data table column types
    /// </summary>
    public static class ColumnTypeClassifier
    {
        static readonly HashSet<BrightDataType> NumericType = new() {
            BrightDataType.Double,
            BrightDataType.Decimal,
            BrightDataType.Float,
            BrightDataType.Int,
            BrightDataType.Short,
            BrightDataType.Long,
            BrightDataType.SByte
        };
        static readonly HashSet<BrightDataType> ContinuousType = new(NumericType) {
            BrightDataType.Date,
            BrightDataType.DateOnly,
            BrightDataType.TimeOnly
        };
        static readonly HashSet<BrightDataType> BlittableType = new(ContinuousType) {
            BrightDataType.Boolean,
            BrightDataType.BinaryData,
            BrightDataType.IndexList,
            BrightDataType.WeightedIndexList,
        };
        static readonly HashSet<BrightDataType> CategoricalType = new() {
            BrightDataType.Boolean,
            BrightDataType.String,
        };
        static readonly HashSet<BrightDataType> DecimalType = new() {
            BrightDataType.Double,
            BrightDataType.Decimal,
            BrightDataType.Float
        };

        /// <summary>
        /// Checks for a decimal type (floating point)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsDecimal(BrightDataType columnType) => DecimalType.Contains(columnType);

        /// <summary>
        /// Checks for a numeric type (floating point or integer)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsNumeric(BrightDataType columnType) => NumericType.Contains(columnType);

        /// <summary>
        /// Checks for a continuous type (non categorical)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsContinuous(BrightDataType columnType) => ContinuousType.Contains(columnType);

        /// <summary>
        /// Checks for a categorical type (non continuous)
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsCategorical(BrightDataType columnType) => CategoricalType.Contains(columnType);

        /// <summary>
        /// Checks if the type has an independent memory layout across managed and unmanaged code
        /// </summary>
        /// <param name="columnType">Column type to check</param>
        /// <returns></returns>
        public static bool IsBlittable(BrightDataType columnType) => BlittableType.Contains(columnType);

        /// <summary>
        /// Returns the set of possible column classifications
        /// </summary>
        /// <param name="type">Column type to check</param>
        /// <param name="metaData">Column metadata</param>
        /// <returns></returns>
        public static ColumnClass GetClass(BrightDataType type, MetaData metaData)
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
                ret |= ColumnClass.DateTime;
            if (type.IsInteger())
                ret |= ColumnClass.Integer;
            return ret;
        }
    }
}
