using BrightWire.Connectionist;
using BrightWire.Models;
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.LinearAlgebra
{
    public class NumericsProvider : ILinearAlgebraProvider
    {
        protected virtual void Dispose(bool disposing)
        {
            // nop
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IVector Create(IEnumerable<float> data)
        {
            return new CpuVector(DenseVector.OfEnumerable(data));
        }

        public IVector Create(float[] data)
        {
            return new CpuVector(DenseVector.OfArray(data));
        }

        public IVector Create(int length, float value)
        {
            return new CpuVector(DenseVector.Create(length, value));
        }

        public IVector Create(int length, Func<int, float> init)
        {
            return new CpuVector(DenseVector.Create(length, init));
        }

        public IMatrix Create(int rows, int columns, Func<int, int, float> init)
        {
            return new CpuMatrix(DenseMatrix.Create(rows, columns, init));
        }

        public IMatrix Create(int rows, int columns, float value)
        {
            return new CpuMatrix(DenseMatrix.Create(rows, columns, value));
        }

        public IMatrix Create(int rows, int columns, IList<IIndexableVector> vectorData)
        {
            return Create(rows, columns, (x, y) => vectorData[y][x]);
        }

        public IMatrix Create(IIndexableMatrix matrix)
        {
            return Create(matrix.RowCount, matrix.ColumnCount, (x, y) => matrix[x, y]);
        }

        public IVector Create(IIndexableVector vector)
        {
            return Create(vector.Count, x => vector[x]);
        }

        public IIndexableVector CreateIndexable(int length) { return Create(length, 0f).AsIndexable(); }
        public IIndexableVector CreateIndexable(int length, Func<int, float> init) { return Create(length, init).AsIndexable(); }
        public IIndexableMatrix CreateIndexable(int rows, int columns) { return Create(rows, columns, 0f).AsIndexable(); }
        public IIndexableMatrix CreateIndexable(int rows, int columns, Func<int, int, float> init) { return Create(rows, columns, init).AsIndexable(); }
        public IIndexableMatrix CreateIndexable(int rows, int columns, float value) { return Create(rows, columns, value).AsIndexable(); }

        public IVector CreateVector(FloatArray data)
        {
            var size = data.Data.Length;
            return new CpuVector(DenseVector.Create(size, i => data.Data[i]));
        }

        public IMatrix CreateMatrix(IReadOnlyList<FloatArray> data)
        {
            var rowCount = data.Count;
            var columnCount = data.First().Data.Length;
            var ret = new CpuMatrix(DenseMatrix.Create(rowCount, columnCount, 0f));

            for (var i = 0; i < rowCount; i++) {
                for (var j = 0; j < columnCount; j++) {
                    ret[i, j] = data[i].Data[j];
                }
            }
            return ret;
        }

        Factory _factory = null;
        public INeuralNetworkFactory NN
        {
            get
            {
                return _factory ?? (_factory = new Factory(this));
            }
        }
    }
}
