using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    /// <summary>
    /// Creates vectors, matrices and tensors using the CPU based math.net numerics library
    /// </summary>
    public class NumericsProvider : ILinearAlgebraProvider
    {
	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="context"></param>
	    public NumericsProvider(IBrightDataContext context)
        {
            Context = context;
        }

        protected virtual void Dispose(bool disposing)
        {
            // nop
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IFloatVector CreateVector(uint length, Func<uint, float> init)
		{
			return new NumericsVector(Context, DenseVector.Create((int)length, i => init((uint)i)));
		}

        /// <inheritdoc />
		public IFloatMatrix CreateMatrixFromRows(params IFloatVector[] vectorData)
		{
			var rows = vectorData.Select(r => r.AsIndexable()).ToArray();
			var columns = rows.First().Count;
			return CreateMatrix((uint)rows.Length, columns, (i, j) => rows[i][j]);
		}

        /// <inheritdoc />
	    public IFloatMatrix CreateMatrixFromColumns(params IFloatVector[] vectorData)
	    {
		    var columns = vectorData.Select(r => r.AsIndexable()).ToArray();
		    var rows = columns.First().Count;
		    return CreateMatrix(rows, (uint)columns.Length, (i, j) => columns[j][i]);
	    }

        /// <inheritdoc />
        public string Name { get; } = "Numerics";

        /// <inheritdoc />
        public IBrightDataContext Context { get; }

        /// <inheritdoc />
        public IFloatVector CreateVector(uint length, bool setToZero = false)
	    {
		    return new NumericsVector(Context, DenseVector.Create((int)length, 0f));
	    }

        /// <inheritdoc />
	    public IFloatMatrix CreateMatrix(uint rows, uint columns, bool setToZero)
        {
	        return new NumericsMatrix(Context, DenseMatrix.Create((int)rows, (int)columns, 0f));
        }

        /// <inheritdoc />
	    public I3DFloatTensor Create3DTensor(uint rows, uint columns, uint depth, bool setToZero = false)
	    {
		    return new Numerics3DTensor(Context, depth.AsRange().Select(i => CreateMatrix(rows, columns, setToZero).AsIndexable()).ToArray());
	    }

        /// <inheritdoc />
	    public I4DFloatTensor Create4DTensor(uint rows, uint columns, uint depth, uint count, bool setToZero = false)
	    {
		    return new Numerics4DTensor(Context, count.AsRange().Select(i => Create3DTensor(rows, columns, depth, setToZero).AsIndexable()).ToArray());
	    }

        /// <inheritdoc />
		public IFloatMatrix CreateMatrix(uint rows, uint columns, Func<uint, uint, float> init)
		{
			return new NumericsMatrix(Context, DenseMatrix.Create((int)rows, (int)columns, (x, y) => init((uint)x, (uint)y)));
		}

        public IFloatMatrix CreateMatrix(Matrix<float> matrix)
        {
            return CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix[i, j]);
        }

        /// <inheritdoc />
		public I3DFloatTensor Create3DTensor(params IFloatMatrix[] data)
		{
			return new Numerics3DTensor(Context, data.Select(m => m.AsIndexable()).ToArray());
		}

        /// <inheritdoc />
		public I4DFloatTensor Create4DTensor(params Tensor3D<float>[] data)
		{
			return new Numerics4DTensor(Context, data.Select(t => Create3DTensor(t.Matrices.Select(CreateMatrix).ToArray()).AsIndexable()).ToArray());
		}

        /// <inheritdoc />
        public I4DFloatTensor Create4DTensor(I3DFloatTensor[] tensorList)
		{
			return new Numerics4DTensor(Context, tensorList.Select(m => m.AsIndexable()).ToArray());
		}

        /// <inheritdoc />
		public void PushLayer()
        {
            // nop
        }

        /// <inheritdoc />
        public void PopLayer()
        {
            // nop
        }

        /// <inheritdoc />
	    public bool IsGpu => false;

        /// <inheritdoc />
	    public IFloatMatrix CalculateDistances(IFloatVector[] vectors, IReadOnlyList<IFloatVector> compareTo, DistanceMetric distanceMetric)
	    {
		    var rows = compareTo.Count;
		    var columns = vectors.Length;
			Debug.Assert(vectors[0].Count == compareTo[0].Count);
		    var ret = new float[rows * columns];

			Parallel.ForEach(vectors, (column1, _, i) => {
				Parallel.ForEach(compareTo, (column2, __, j) => {
					ret[i * rows + j] = column1.FindDistance(column2, distanceMetric);
				});
			});

		    return new NumericsMatrix(Context, DenseMatrix.Build.Dense(rows, columns, ret));
	    }

        /// <inheritdoc />
        public IFloatVector CreateVector(ITensorSegment<float> data)
        {
            return CreateVector(data.Size, i => data[i]);
        }
    }
}
