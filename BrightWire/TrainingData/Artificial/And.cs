using BrightData;
using BrightTable;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Simple AND training data
    /// </summary>
    public static class And
    {
        /// <summary>
        /// Generates a data table containing AND training data
        /// </summary>
        /// <returns></returns>
        public static IRowOrientedDataTable Get(IBrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(ColumnType.Float, "X");
            builder.AddColumn(ColumnType.Float, "Y");
            builder.AddColumn(ColumnType.Float, "AND").SetTargetColumn(true);

            builder.AddRow(0.0f, 0.0f, 0.0f);
            builder.AddRow(1.0f, 0.0f, 0.0f);
            builder.AddRow(0.0f, 1.0f, 0.0f);
            builder.AddRow(1.0f, 1.0f, 1.0f);

            return builder.BuildRowOriented();
        }
    }
}
