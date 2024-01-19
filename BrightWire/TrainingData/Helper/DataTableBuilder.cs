using BrightData;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Creates standard data table builders
    /// </summary>
    public static class DataTableBuilder
    {
        const string Input = "Input";
        const string Target = "Target";

        /// <summary>
        /// Creates a data table builder with one feature matrix column and one target matrix column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inputRows"></param>
        /// <param name="inputColumns"></param>
        /// <param name="outputRows"></param>
        /// <param name="outputColumns"></param>
        /// <returns></returns>
        public static IBuildDataTables CreateTwoColumnMatrixTableBuilder(this BrightDataContext context, uint? inputRows = null, uint? inputColumns = null, uint? outputRows = null, uint? outputColumns = null)
        {
            var ret = context.CreateTableBuilder();
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
        public static IBuildDataTables CreateTwoColumnVectorTableBuilder(this BrightDataContext context, uint? inputSize = null, uint? outputSize = null)
        {
            var ret = context.CreateTableBuilder();
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
        public static IBuildDataTables Create3DTensorToVectorTableBuilder(this BrightDataContext context, uint? inputDepth = null, uint? inputRows = null, uint? inputColumns = null, uint? outputSize = null)
        {
            var ret = context.CreateTableBuilder();
            Add3DTensor(ret, inputDepth, inputRows, inputColumns, Input);
            AddVector(ret, outputSize, Target).MetaData.SetTarget(true);
            return ret;
        }

        static ICompositeBuffer<ReadOnlyVector> AddVector(this IBuildDataTables builder, uint? size, string name)
        {
            return size != null 
                ? builder.CreateFixedSizeVectorColumn(size.Value, name) 
                : (ICompositeBuffer<ReadOnlyVector>)builder.CreateColumn(BrightDataType.Vector, name);
        }

        static ICompositeBuffer<ReadOnlyMatrix> AddMatrix(this IBuildDataTables builder, uint? rows, uint? columns, string name)
        {
            return rows != null && columns != null 
                ? builder.CreateFixedSizeMatrixColumn(rows.Value, columns.Value, name) 
                : (ICompositeBuffer<ReadOnlyMatrix>)builder.CreateColumn(BrightDataType.Matrix, name);
        }

        static ICompositeBuffer<ReadOnlyTensor3D> Add3DTensor(this IBuildDataTables builder, uint? depth, uint? rows, uint? columns, string name)
        {
            return depth != null && rows != null && columns != null 
                ? builder.CreateFixedSize3DTensorColumn(depth.Value, rows.Value, columns.Value, name) 
                : (ICompositeBuffer<ReadOnlyTensor3D>)builder.CreateColumn(BrightDataType.Tensor3D, name);
        }

        static ICompositeBuffer<ReadOnlyTensor4D> Add4DTensor(this IBuildDataTables builder, uint? count, uint? depth, uint? rows, uint? columns, string name)
        {
            return count != null && depth != null && rows != null && columns != null 
                ? builder.CreateFixedSize4DTensorColumn(count.Value, depth.Value, rows.Value, columns.Value, name) 
                : (ICompositeBuffer<ReadOnlyTensor4D>)builder.CreateColumn(BrightDataType.Tensor4D, name);
        }
    }
}
