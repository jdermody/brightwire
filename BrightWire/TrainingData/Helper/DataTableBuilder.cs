using BrightData;
using BrightData.DataTable.Builders;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Creates standard data table builders
    /// </summary>
    public static class DataTableBuilder
    {
        static readonly string Input = "Input";
        static readonly string Target = "Target";

        /// <summary>
        /// Creates a data table builder with one feature matrix column and one target matrix column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inputRows"></param>
        /// <param name="inputColumns"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <returns></returns>
        public static InMemoryTableBuilder CreateTwoColumnMatrixTableBuilder(this IBrightDataContext context, uint? inputRows = null, uint? inputColumns = null, uint? outputRows = null, uint? outputColumns = null)
        {
            var ret = context.BuildTable();
            AddMatrix(ret, inputRows, inputColumns, Input);
            AddMatrix(ret, outputRows, outputColumns, Target).SetTarget(true);
            return ret;
        }

        /// <summary>
        /// Creates a data table builder with one feature vector column and one target vector column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inputSize">Size of the input vector</param>
        /// <param name="outputSize">Size of the output vector</param>
        /// <returns></returns>
        public static InMemoryTableBuilder CreateTwoColumnVectorTableBuilder(this IBrightDataContext context, uint? inputSize = null, uint? outputSize = null)
        {
            var ret = context.BuildTable();
            AddVector(ret, inputSize, Input);
            AddVector(ret, outputSize, Target).SetTarget(true);
            return ret;
        }

        /// <summary>
        /// Creates a data table builder with one feature tensor 3D columns and one target vector column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inputDepth"></param>
        /// <param name="inputRows"></param>
        /// <param name="inputColumns"></param>
        /// <param name="outputSize">Size of the output vector</param>
        /// <returns></returns>
        public static InMemoryTableBuilder Create3DTensorToVectorTableBuilder(this IBrightDataContext context, uint? inputDepth = null, uint? inputRows = null, uint? inputColumns = null, uint? outputSize = null)
        {
            var ret = context.BuildTable();
            Add3DTensor(ret, inputDepth, inputRows, inputColumns, Input);
            AddVector(ret, outputSize, Target).SetTarget(true);
            return ret;
        }

        static IMetaData AddVector(InMemoryTableBuilder builder, uint? size, string name)
        {
            if (size != null)
                return builder.AddFixedSizeVectorColumn(size.Value, name);
            return builder.AddColumn(BrightDataType.FloatVector, name);
        }

        static IMetaData AddMatrix(InMemoryTableBuilder builder, uint? rows, uint? columns, string name)
        {
            if (rows != null && columns != null)
                return builder.AddFixedSizeMatrixColumn(rows.Value, columns.Value, name);
            return builder.AddColumn(BrightDataType.FloatMatrix, name);
        }

        static IMetaData Add3DTensor(InMemoryTableBuilder builder, uint? depth, uint? rows, uint? columns, string name)
        {
            if (depth != null && rows != null && columns != null)
                return builder.AddFixedSize3DTensorColumn(depth.Value, rows.Value, columns.Value, name);
            return builder.AddColumn(BrightDataType.FloatTensor3D, name);
        }
    }
}
