using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using BrightWire.Models;

namespace BrightWire.LinearAlgebra
{
    internal class CpuProvider : ILinearAlgebraProvider
    {
        readonly bool _isStochastic;

        public CpuProvider(bool stochastic = true)
        {
            _isStochastic = stochastic;
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

        public I3DTensor CreateTensor(IReadOnlyList<IMatrix> data)
        {
            return new Cpu3DTensor(data);
        }

        public I4DTensor CreateTensor(IReadOnlyList<FloatTensor> data)
        {
            return new Cpu4DTensor(data.Select(t => this.CreateTensor(t)).ToList());
        }

        public I4DTensor CreateTensor(IMatrix tensorAsMatrix, int rows, int columns, int depth)
        {
            var list = new List<I3DTensor>();
            for(var i = 0; i < tensorAsMatrix.ColumnCount; i++)
                list.Add(CreateTensor(tensorAsMatrix.Column(i).Split(depth).Select(v => v.ConvertInPlaceToMatrix(rows, columns)).ToList()));
            return new Cpu4DTensor(list);
        }

        public I4DTensor CreateTensor(IReadOnlyList<I3DTensor> tensorList)
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

        public bool IsStochastic => _isStochastic;
        public bool IsGpu => false;
    }
}
