using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Single;

namespace BrightData.Numerics
{
    class NumericsComputableFactory //: IComputableFactory
    {
        private readonly IBrightDataContext _context;

        public NumericsComputableFactory(IBrightDataContext context)
        {
            _context = context;
        }

        //public IComputableVector Create(Vector<float> vector)
        //{
        //    return new NumericsVector(_context, new DenseVector(vector.ToArray()));
        //}

        //public IComputableVector Create(int size, Func<int, float> initializer)
        //{
        //    return new NumericsVector(_context, DenseVector.Create(size, initializer));
        //}

        //public IComputableMatrix Create(Matrix<float> matrix)
        //{
        //    return new NumericsMatrix(DenseMatrix.Create((int)matrix.RowCount, (int)matrix.ColumnCount, (i, j) => matrix[i, j]));
        //}

        //public IComputableMatrix Create(int rows, int columns, Func<int, int, float> initializer)
        //{
        //    return new NumericsMatrix(DenseMatrix.Create(rows, columns, initializer));
        //}

        //public IComputable3DTensor Create(Tensor3D<float> tensor)
        //{
        //    var matrices = tensor.AllMatrices().Select(Create).ToList();
        //    return new Numerics3DTensor(matrices);
        //}

        //public IComputable4DTensor Create(Tensor4D<float> tensor)
        //{
        //    var tensors = tensor.AllTensors().Select(Create).ToList();
        //    return new Numerics4DTensor(tensors);
        //}
    }
}
