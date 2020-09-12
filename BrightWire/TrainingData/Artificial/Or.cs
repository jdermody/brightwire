using BrightData;
using BrightTable;
using BrightTable.Builders;
using BrightWire.Helper;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Simple OR training data
    /// </summary>
    public static class Or
    {
        /// <summary>
        /// Generates a data table containing OR training data
        /// </summary>
        /// <returns></returns>
        public static IDataTable Get(IBrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.Float, "X");
            builder.AddColumn(ColumnType.Float, "Y");
            builder.AddColumn(ColumnType.Float, "OR").SetTargetColumn(true);

            builder.AddRow(0.0f, 0.0f, 0.0f);
            builder.AddRow(1.0f, 0.0f, 1.0f);
            builder.AddRow(0.0f, 1.0f, 1.0f);
            builder.AddRow(1.0f, 1.0f, 1.0f);

            return builder.Build();
        }
    }
}
