using System.Threading.Tasks;
using BrightData;

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
        public static Task<IDataTable> Get(BrightDataContext context)
        {
            var builder = context.CreateTableBuilder();
            builder.CreateColumn(BrightDataType.Float, "X");
            builder.CreateColumn(BrightDataType.Float, "Y");
            builder.CreateColumn(BrightDataType.Float, "AND").MetaData.SetTarget(true);

            builder.AddRow(0.0f, 0.0f, 0.0f);
            builder.AddRow(1.0f, 0.0f, 0.0f);
            builder.AddRow(0.0f, 1.0f, 0.0f);
            builder.AddRow(1.0f, 1.0f, 1.0f);

            return builder.BuildInMemory();
        }
    }
}
