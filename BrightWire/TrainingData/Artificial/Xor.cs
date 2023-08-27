using BrightData;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Simple XOR training data
    /// </summary>
    public static class Xor
    {
        /// <summary>
        /// Generates a data table containing XOR training data
        /// </summary>
        /// <returns></returns>
        public static BrightDataTable Get(BrightDataContext context)
        {
            var builder = context.CreateTableBuilder();
            builder.AddColumn(BrightDataType.Float, "X");
            builder.AddColumn(BrightDataType.Float, "Y");
            builder.AddColumn(BrightDataType.Float, "XOR").MetaData.SetTarget(true);

            builder.AddRow(0.0f, 0.0f, 0.0f);
            builder.AddRow(1.0f, 0.0f, 1.0f);
            builder.AddRow(0.0f, 1.0f, 1.0f);
            builder.AddRow(1.0f, 1.0f, 0.0f);

            return builder.BuildInMemory();
        }
    }
}
