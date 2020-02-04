using System;
using System.Linq;
using BrightData;
using BrightTable;

namespace BrightML
{
    class DataTableAdaptor
    {
        enum InputType
        {
            Unsupported = 0,
            Integer,
            Decimal,
            Indexed,
            Tensor
        }

        public DataTableAdaptor(IRowOrientedDataTable inputTable)
        {
            var columns = inputTable.AllColumns();

            // find the classification target
            var classificationTarget = columns.SingleOrDefault(c => c.IsTarget());
            if (classificationTarget == null)
                throw new ArgumentException("Input table does not contain a target classification column");

            // analyse the classification type
            var targetType = classificationTarget.SingleType;
            if (targetType.IsInteger()) {

            }else if (targetType.IsDecimal()) {

            }else if (targetType == ColumnType.IndexList || targetType == ColumnType.WeightedIndexList) {

            }else if (targetType == ColumnType.Vector) {

            }else if (targetType == ColumnType.Matrix) {

            }
            else {
                throw new ArgumentException($"{targetType} is not a valid classification type");
            }

            // find the features
            var featureColumns = columns.Where(c => c.IsFeature()).ToList();
            if(!featureColumns.Any())
                throw new ArgumentException("Input table does not contain any feature columns");

            foreach (var featureColumn in featureColumns) {
                var featureType = featureColumn.SingleType;
                if (featureType.IsInteger()) {

                } else if (featureType.IsDecimal()) {

                } else if (featureType == ColumnType.IndexList || featureType == ColumnType.WeightedIndexList) {

                } else if (featureType == ColumnType.Vector) {

                } else if (featureType == ColumnType.Matrix) {

                } else
                    throw new ArgumentException($"{targetType} is not a valid feature column type");
                
            }
        }

        InputType _FindType(ColumnType columnType)
        {
            if (columnType.IsInteger()) {
                return InputType.Integer;
            } else if (columnType.IsDecimal()) {
                return InputType.Decimal;
            } else if (columnType.IsIndexed()) {
                return InputType.Indexed;
            } else if (columnType.IsTensor()) {
                return InputType.Tensor;
            }

            return InputType.Unsupported;
        }
    }
}
