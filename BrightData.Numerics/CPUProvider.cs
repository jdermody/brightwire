using System;
using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using System.Threading.Tasks;
using BrightWire.Models;
using MathNet.Numerics.LinearAlgebra.Storage;
using BrightData;
using BrightData.Numerics;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// Creates vectors, matrices and tensors using the CPU based math.net numerics library
    /// </summary>
    public class CpuProvider : ILinearAlgebraProvider
    {
	    public CpuProvider(IBrightDataContext context, bool stochastic = true)
        {
            Context = context;
			IsStochastic = stochastic;
        }

        protected virtual void Dispose(bool disposing)
        {
            // nop
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

		public IFloatVector CreateVector(uint length, Func<uint, float> init)
		{
			return new CpuVector(DenseVector.Create((int)length, i => init((uint)i)));
		}

		public IFloatMatrix CreateMatrixFromRows(IReadOnlyList<IFloatVector> vectorData)
		{
			var rows = vectorData.Select(r => r.AsIndexable()).ToArray();
			var columns = rows.First().Count;
			return CreateMatrix((uint)rows.Length, columns, (i, j) => rows[i][j]);
		}

	    public IFloatMatrix CreateMatrixFromColumns(IReadOnlyList<IFloatVector> vectorData)
	    {
		    var columns = vectorData.Select(r => r.AsIndexable()).ToArray();
		    var rows = columns.First().Count;
		    return CreateMatrix(rows, (uint)columns.Length, (i, j) => columns[j][i]);
	    }

        public IBrightDataContext Context { get; }

        public IFloatVector CreateVector(uint length, bool setToZero = false)
	    {
		    return new CpuVector(DenseVector.Create((int)length, 0f));
	    }

	    public IFloatMatrix CreateMatrix(uint rows, uint columns, bool setToZero)
        {
	        return new CpuMatrix(DenseMatrix.Create((int)rows, (int)columns, 0f));
        }

	    public I3DFloatTensor Create3DTensor(uint rows, uint columns, uint depth, bool setToZero = false)
	    {
		    return new Cpu3DTensor(depth.AsRange().Select(i => CreateMatrix(rows, columns, setToZero).AsIndexable()).ToList());
	    }

	    public I4DFloatTensor Create4DTensor(uint rows, uint columns, uint depth, uint count, bool setToZero = false)
	    {
		    return new Cpu4DTensor(count.AsRange().Select(i => Create3DTensor(rows, columns, depth, setToZero).AsIndexable()).ToList());
	    }

		public IFloatMatrix CreateMatrix(uint rows, uint columns, Func<uint, uint, float> init)
		{
			return new CpuMatrix(DenseMatrix.Create((int)rows, (int)columns, (x, y) => init((uint)x, (uint)y)));
		}

        public IFloatMatrix CreateMatrix(Matrix<float> matrix)
        {
            return CreateMatrix(matrix.RowCount, matrix.ColumnCount, (i, j) => matrix[i, j]);
        }

		public I3DFloatTensor Create3DTensor(IReadOnlyList<IFloatMatrix> data)
		{
			return new Cpu3DTensor(data.Select(m => m.AsIndexable()).ToList());
		}

		public I4DFloatTensor Create4DTensor(IReadOnlyList<Tensor3D<float>> data)
		{
			return new Cpu4DTensor(data.Select(t => Create3DTensor(t.Matrices.Select(m => CreateMatrix(m)).ToList()).AsIndexable()).ToList());
		}

        private I3DFloatTensor Create3DTensor(Tensor3D<float> tensor)
        {
            return new Cpu3DTensor(tensor.Matrices.Select(m => CreateMatrix(m).AsIndexable()).ToList());
        }

        public I4DFloatTensor Create4DTensor(IReadOnlyList<I3DFloatTensor> tensorList)
		{
			return new Cpu4DTensor(tensorList.Select(m => m.AsIndexable()).ToList());
		}

		public void PushLayer()
        {
            // nop
        }
        public void PopLayer()
        {
            // nop
        }

        public bool IsStochastic { get; }
	    public bool IsGpu => false;

	    public IFloatMatrix CalculateDistances(IReadOnlyList<IFloatVector> vectors, IReadOnlyList<IFloatVector> compareTo, DistanceMetric distanceMetric)
	    {
		    var rows = compareTo.Count;
		    var columns = vectors.Count;
			Debug.Assert(vectors[0].Count == compareTo[0].Count);
		    var ret = new float[rows * columns];

			Parallel.ForEach(vectors, (column1, _, i) => {
				Parallel.ForEach(compareTo, (column2, __, j) => {
					ret[i * rows + j] = column1.FindDistance(column2, distanceMetric);
				});
			});

		    return new CpuMatrix(DenseMatrix.Build.Dense(rows, columns, ret));
	    }

        public IFloatVector CreateVector(ITensorSegment<float> data)
        {
            return CreateVector(data.Size, i => data[i]);
        }
    }
}
