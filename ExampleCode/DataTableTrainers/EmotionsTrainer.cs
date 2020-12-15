using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable;
using BrightTable.Transformations;

namespace ExampleCode.DataTableTrainers
{
    class EmotionsTrainer : DataTableTrainer
    {
        public EmotionsTrainer(IRowOrientedDataTable table, IRowOrientedDataTable training, IRowOrientedDataTable test) : base(table, training, test)
        {
        }

        public static IRowOrientedDataTable Parse(IBrightDataContext context, string filePath)
        {
            const int targetColumnCount = 6;

            // read the data as CSV, skipping the header
            using var reader = new StreamReader(filePath);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "@data")
                    break;
            }
            using var table = context.ParseCsv(reader, false);

            // convert the feature columns to numeric and the target columns to boolean
            var featureColumns = (table.ColumnCount - targetColumnCount).AsRange().ToArray();
            var targetColumns = targetColumnCount.AsRange((int)table.ColumnCount - targetColumnCount).ToArray();
            var columnConversions = featureColumns
                .Select(i => new ColumnConversion(i, ColumnConversionType.ToNumeric))
                .Concat(targetColumns.Select(i => new ColumnConversion(i, ColumnConversionType.ToBoolean)))
                .ToArray();
            using var converted = table.Convert(columnConversions);

            // convert the many feature columns to an index list and set that as the feature column
            using var reinterpeted = converted.ReinterpretColumns(new ReinterpretColumns(ColumnType.IndexList, "Targets", targetColumns));
            reinterpeted.SetTargetColumn(reinterpeted.ColumnCount-1);

            // return as row oriented
            return reinterpeted.ToRowOriented();
        }
    }
}
