using System;
using System.Collections.Generic;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using System.Threading.Tasks;
using BrightWire.Models;

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
		    return new Cpu3DTensor(Enumerable.Range(0, depth).Select(i => CreateMatrix(rows, columns, setToZero)).ToList());
	    }

	    public I4DTensor Create4DTensor(int rows, int columns, int depth, int count, bool setToZero = false)
	    {
		    return new Cpu4DTensor(Enumerable.Range(0, count).Select(i => Create3DTensor(rows, columns, depth, setToZero)).ToList());
	    }

		public IMatrix CreateMatrix(int rows, int columns, Func<int, int, float> init)
		{
			return new CpuMatrix(DenseMatrix.Create(rows, columns, init));
		}

		public I3DTensor Create3DTensor(IReadOnlyList<IMatrix> data)
		{
			return new Cpu3DTensor(data);
		}

		public I4DTensor Create4DTensor(IReadOnlyList<FloatTensor> data)
		{
			return new Cpu4DTensor(data.Select(this.Create3DTensor).ToList());
		}

		public I4DTensor Create4DTensor(IReadOnlyList<I3DTensor> tensorList)
		{
			return new Cpu4DTensor(tensorList);
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

	    public float[,] CalculateDistances(IReadOnlyList<IVector> vectors, IReadOnlyList<IVector> compareTo, DistanceMetric distanceMetric)
	    {
		    //var vectors2 = vectors.Select(v => v.AsIndexable()).ToList();
			//var compareTo2 = compareTo.Select(v => v.AsIndexable()).ToList();
		    int vectorsCount = vectors.Count, compareToCount = compareTo.Count, size = vectors[0].Count;
			Debug.Assert(vectors[0].Count == compareTo[0].Count);
		    var ret = new float[compareToCount, vectorsCount];
		  //  Parallel.For(0, vectorsCount * compareToCount * size, z => {
			 //   var j = z % compareToCount;
			 //   var z2 = z / compareToCount;
				//var i = z2 % vectorsCount;
				//var k = z2 / vectorsCount;

			 //   var aVal = vectors2[i][k];
			 //   var bVal = compareTo2[j][k];
			 //   var output = Math.Pow(aVal - bVal, 2);
			 //   lock (ret) {
				//    ret[i, j] += (float) output;
			 //   }

			 //   //ret[i, j] = vectors[i].FindDistance(compareTo[j], distanceMetric);
		  //  });
		  //  Parallel.For(0, vectorsCount * compareToCount, z => {
			 //   var j = z % compareToCount;
			 //   var i = z / compareToCount;

			 //   ret[i, j] = (float)Math.Sqrt(ret[i, j]);
		  //  });
			Parallel.ForEach(vectors, (column1, _, i) => {
				Parallel.ForEach(compareTo, (column2, __, j) => {
					ret[j, i] = column1.FindDistance(column2, distanceMetric);
				});
			});
			return ret;
	    }
    }
}
