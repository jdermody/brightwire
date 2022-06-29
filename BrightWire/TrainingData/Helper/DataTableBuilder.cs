using BrightData;
using BrightData.DataTable2;

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
        public static BrightDataTableBuilder CreateTwoColumnMatrixTableBuilder(this BrightDataContext context, uint? inputRows = null, uint? inputColumns = null, uint? outputRows = null, uint? outputColumns = null)
        {
            var ret = new BrightDataTableBuilder(context);
            AddMatrix(ret, inputRows, inputColumns, Input);
            AddMatrix(ret, outputRows, outputColumns, Target).MetaData.SetTarget(true);
            return ret;
        }

        /// <summary>
        /// Creates a data table builder with one feature vector column and one target vector column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inputSize">Size of the input vector</param>
        /// <param name="outputSize">Size of the output vector</param>
        /// <returns></returns>
        public static BrightDataTableBuilder CreateTwoColumnVectorTableBuilder(this BrightDataContext context, uint? inputSize = null, uint? outputSize = null)
        {
            var ret = new BrightDataTableBuilder(context);
            AddVector(ret, inputSize, Input);
            AddVector(ret, outputSize, Target).MetaData.SetTarget(true);
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
        public static BrightDataTableBuilder Create3DTensorToVectorTableBuilder(this BrightDataContext context, uint? inputDepth = null, uint? inputRows = null, uint? inputColumns = null, uint? outputSize = null)
        {
            var ret = new BrightDataTableBuilder(context);
            Add3DTensor(ret, inputDepth, inputRows, inputColumns, Input);
            AddVector(ret, outputSize, Target).MetaData.SetTarget(true);
            return ret;
        }

        static IHybridBufferWithMetaData AddVector(this BrightDataTableBuilder builder, uint? size, string name)
        {
            return size != null 
                ? (IHybridBufferWithMetaData)builder.AddFixedSizeVectorColumn(size.Value, name) 
                : builder.AddColumn(BrightDataType.Vector, name);
        }

        static IHybridBufferWithMetaData AddMatrix(this BrightDataTableBuilder builder, uint? rows, uint? columns, string name)
        {
            return rows != null && columns != null 
                ? (IHybridBufferWithMetaData)builder.AddFixedSizeMatrixColumn(rows.Value, columns.Value, name) 
                : builder.AddColumn(BrightDataType.Matrix, name);
        }

        static IHybridBufferWithMetaData Add3DTensor(this BrightDataTableBuilder builder, uint? depth, uint? rows, uint? columns, string name)
        {
            return depth != null && rows != null && columns != null 
                ? (IHybridBufferWithMetaData)builder.AddFixedSize3DTensorColumn(depth.Value, rows.Value, columns.Value, name) 
                : builder.AddColumn(BrightDataType.Tensor3D, name);
        }

        static IHybridBufferWithMetaData Add4DTensor(this BrightDataTableBuilder builder, uint? count, uint? depth, uint? rows, uint? columns, string name)
        {
            return count != null && depth != null && rows != null && columns != null 
                ? (IHybridBufferWithMetaData)builder.AddFixedSize4DTensorColumn(count.Value, depth.Value, rows.Value, columns.Value, name) 
                : builder.AddColumn(BrightDataType.Tensor4D, name);
        }
    }
}
