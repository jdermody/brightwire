using System;
using System.Linq;
using BrightData;
using BrightTable;

namespace BrightML
{
    class DataTableAdaptor
    {
        public DataTableAdaptor(IRowOrientedDataTable inputTable)
        {
            var columns = inputTable.AllColumns();

            // find the classification target
            var classificationTarget = columns.SingleOrDefault(c => c.IsTarget());
            if (classificationTarget == null)
                throw new ArgumentException("Table does not contain a target classification column");

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

            // find the inputs
        }
    }
}
