using BrightData;
using BrightData.DataTable2;

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
        public static BrightDataTable Get(BrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(BrightDataType.Float, "X");
            builder.AddColumn(BrightDataType.Float, "Y");
            builder.AddColumn(BrightDataType.Float, "OR").MetaData.SetTarget(true);

            builder.AddRow(0.0f, 0.0f, 0.0f);
            builder.AddRow(1.0f, 0.0f, 1.0f);
            builder.AddRow(0.0f, 1.0f, 1.0f);
            builder.AddRow(1.0f, 1.0f, 1.0f);

            return builder.BuildInMemory();
        }
    }
}
