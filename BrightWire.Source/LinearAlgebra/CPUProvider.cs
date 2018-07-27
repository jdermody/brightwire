using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
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

        public IVector CreateVector(IEnumerable<float> data)
        {
            return new CpuVector(DenseVector.OfEnumerable(data));
        }

        public IVector CreateVector(int length, Func<int, float> init)
        {
            return new CpuVector(DenseVector.Create(length, init));
        }

        public IMatrix CreateMatrix(IReadOnlyList<IVector> vectorData)
        {
            var rows = vectorData.Select(r => r.AsIndexable()).ToList();
            var columns = rows.First().Count;
            return CreateMatrix(rows.Count, columns, (i, j) => rows[i][j]);
        }

        public IMatrix CreateZeroMatrix(int rows, int columns)
        {
            return new CpuMatrix(DenseMatrix.Create(rows, columns, 0f));
        }

        public IMatrix CreateMatrix(int rows, int columns)
        {
            return CreateZeroMatrix(rows, columns);
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

        //public I4DTensor Create4DTensor(IMatrix tensorAsMatrix, int rows, int columns, int depth)
        //{
        //    var list = new List<I3DTensor>();
        //    for(var i = 0; i < tensorAsMatrix.ColumnCount; i++)
        //        list.Add(Create3DTensor(tensorAsMatrix.Column(i).Split(depth).Select(v => v.ConvertInPlaceToMatrix(rows, columns)).ToList()));
        //    return new Cpu4DTensor(list);
        //}

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
    }
}
