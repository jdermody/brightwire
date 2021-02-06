using BrightData;
using BrightData.DataTable.Builders;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Creates standard data table builders
    /// </summary>
    public static class DataTableBuilder
    {
        /// <summary>
        /// Creates a data table builder with one feature matrix column and one target matrix column
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static InMemoryTableBuilder CreateTwoColumnMatrixTableBuilder(this IBrightDataContext context)
        {
            var ret = context.BuildTable();
            ret.AddColumn(ColumnType.Matrix, "Input");
            ret.AddColumn(ColumnType.Matrix, "Target").SetTarget(true);
            return ret;
        }

        /// <summary>
        /// Creates a data table builder with one feature vector column and one target vector column
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static InMemoryTableBuilder CreateTwoColumnVectorTableBuilder(this IBrightDataContext context)
        {
            var ret = context.BuildTable();
            ret.AddColumn(ColumnType.Vector, "Input");
            ret.AddColumn(ColumnType.Vector, "Target").SetTarget(true);
            return ret;
        }

        /// <summary>
        /// Creates a data table builder with one feature tensor 3D columna and one target vector column
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static InMemoryTableBuilder Create3DTensorToVectorTableBuilder(this IBrightDataContext context)
        {
            var ret = context.BuildTable();
            ret.AddColumn(ColumnType.Tensor3D, "Input");
            ret.AddColumn(ColumnType.Vector, "Target").SetTarget(true);
            return ret;
        }
    }
}
