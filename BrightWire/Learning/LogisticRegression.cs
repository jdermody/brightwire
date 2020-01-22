using BrightData;
using BrightTable;
using System;
using System.Linq;

namespace BrightWire.Learning
{
    public static class LogisticRegression
    {
        public static void TrainLogisticRegression(this IDataTable dataTable)
        {
            var columns = dataTable.AllColumns();

            // find the classification target
            var classificationTarget = columns.SingleOrDefault(c => c.IsTarget() && c.IsNumeric());
            if(classificationTarget == null)
                throw new ArgumentException("Table does not contain a numeric target column");

            // find the numeric input columns
            var numericColumns = columns.Where(c => c != classificationTarget && c.IsNumeric()).ToList();
            if(!numericColumns.Any())
                throw new ArgumentException("Table does not contain any numeric data columns");

            // copy the feature vectors
            var context = dataTable.Context;
            var feature = context.CreateMatrix<float>(dataTable.RowCount, (uint)numericColumns.Count+1);
            uint columnIndex = 1;
            foreach(var column in numericColumns)
                column.CopyTo(feature.Column(columnIndex++));
            feature.Column(0).Initialize(1f);

            // copy the target vector
            var target = context.CreateVector<float>(dataTable.ColumnCount);
            classificationTarget.CopyTo(target.Data);
        }
    }
}
