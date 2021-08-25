using BrightData;

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
        public static IRowOrientedDataTable Get(IBrightDataContext context)
        {
            var builder = context.BuildTable();
            builder.AddColumn(BrightDataType.Float, "X");
            builder.AddColumn(BrightDataType.Float, "Y");
            builder.AddColumn(BrightDataType.Float, "XOR").SetTarget(true);

            builder.AddRow(0.0f, 0.0f, 0.0f);
            builder.AddRow(1.0f, 0.0f, 1.0f);
            builder.AddRow(0.0f, 1.0f, 1.0f);
            builder.AddRow(1.0f, 1.0f, 0.0f);

            return builder.BuildRowOriented();
        }
    }
}
