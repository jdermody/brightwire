using System;
using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using System.Threading.Tasks;
using BrightWire.Models;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace BrightWire.LinearAlgebra
{
    /// <summary>
    /// Creates vectors, matrices and tensors using the CPU based math.net numerics library
    /// </summary>
    class CpuProvider : ILinearAlgebraProvider
    {
	    public CpuProvider(bool stochastic = true)
        {
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

		public IVector CreateVector(int length, Func<int, float> init)
		{
			return new CpuVector(DenseVector.Create(length, init));
		}

		public IMatrix CreateMatrixFromRows(IReadOnlyList<IVector> vectorData)
		{
			var rows = vectorData.Select(r => r.AsIndexable()).ToList();
			var columns = rows.First().Count;
			return CreateMatrix(rows.Count, columns, (i, j) => rows[i][j]);
		}

	    public IMatrix CreateMatrixFromColumns(IReadOnlyList<IVector> vectorData)
	    {
		    var columns = vectorData.Select(r => r.AsIndexable()).ToList();
		    var rows = columns.First().Count;
		    return CreateMatrix(rows, columns.Count, (i, j) => columns[j][i]);
	    }

		public IVector CreateVector(int length, bool setToZero = false)
	    {
		    return new CpuVector(DenseVector.Create(length, 0f));
	    }

	    public IMatrix CreateMatrix(int rows, int columns, bool setToZero)
        {
	        return new CpuMatrix(DenseMatrix.Create(rows, columns, 0f));
        }

	    public I3DTensor Create3DTensor(int rows, int columns, int depth, bool setToZero = false)
	    {
		    return new Cpu3DTensor(Enumerable.Range(0, depth).Select(i => CreateMatrix(rows, columns, setToZero).AsIndexable()).ToList());
	    }

	    public I4DTensor Create4DTensor(int rows, int columns, int depth, int count, bool setToZero = false)
	    {
		    return new Cpu4DTensor(Enumerable.Range(0, count).Select(i => Create3DTensor(rows, columns, depth, setToZero).AsIndexable()).ToList());
	    }

		public IMatrix CreateMatrix(int rows, int columns, Func<int, int, float> init)
		{
			return new CpuMatrix(DenseMatrix.Create(rows, columns, init));
		}

		public I3DTensor Create3DTensor(IReadOnlyList<IMatrix> data)
		{
			return new Cpu3DTensor(data.Select(m => m.AsIndexable()).ToList());
		}

		public I4DTensor Create4DTensor(IReadOnlyList<FloatTensor> data)
		{
			return new Cpu4DTensor(data.Select(this.Create3DTensor).Select(t => t.AsIndexable()).ToList());
		}

		public I4DTensor Create4DTensor(IReadOnlyList<I3DTensor> tensorList)
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

	    public IMatrix CalculateDistances(IReadOnlyList<IVector> vectors, IReadOnlyList<IVector> compareTo, DistanceMetric distanceMetric)
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
    }
}
