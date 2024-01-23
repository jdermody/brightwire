using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Reshapes to a vector
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyVector Reshape(this IHaveReadOnlyTensorSegment<float> vector)
        {
            return new ReadOnlyVector(vector.ReadOnlySegment);
        }

        /// <summary>
        /// Reshapes to a matrix
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="rows">Row count of each matrix (one parameter is optional null)</param>
        /// <param name="columns">Column count of each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        public static IReadOnlyMatrix Reshape(this IHaveReadOnlyTensorSegment<float> vector, uint? rows, uint? columns)
        {
            var shape = vector.ReadOnlySegment.Size.ResolveShape(rows, columns);
            return new ReadOnlyMatrix(vector.ReadOnlySegment, shape[0], shape[1]);
        }

        /// <summary>
        /// Reshapes to a 3D tensor
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="depth">Number of matrices (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        public static IReadOnlyTensor3D Reshape(this IHaveReadOnlyTensorSegment<float> vector, uint? depth, uint? rows, uint? columns)
        {
            var shape = vector.ReadOnlySegment.Size.ResolveShape(depth, rows, columns);
            return new ReadOnlyTensor3D(vector.ReadOnlySegment, shape[0], shape[1], shape[2]);
        }

        /// <summary>
        /// Reshapes to a 4D tensor
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="count">Number of 3D tensors (one parameter is optional null)</param>
        /// <param name="depth">Number of matrices in each 3D tensor (one parameter is optional null)</param>
        /// <param name="rows">Number of rows in each matrix (one parameter is optional null)</param>
        /// <param name="columns">Number of columns in each matrix (one parameter is optional null)</param>
        /// <returns></returns>
        public static IReadOnlyTensor4D Reshape(this IHaveReadOnlyTensorSegment<float> vector, uint? count, uint? depth, uint? rows, uint? columns)
        {
            var shape = vector.ReadOnlySegment.Size.ResolveShape(count, depth, rows, columns);
            return new ReadOnlyTensor4D(vector.ReadOnlySegment, shape[0], shape[1], shape[2], shape[3]);
        }
    }
}
