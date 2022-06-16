using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BrightData.Cuda;
using BrightData.Helper;
using BrightData.LinearAlegbra2;
using BrightData.MKL;
using BrightData.Serialisation;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace BrightData.UnitTests
{
    public class TensorTests2 : IDisposable
    {
        readonly BrightDataContext _context = new(0);
        readonly ComputationUnit _computationUnit;
        readonly MklComputationUnit _mklComputationUnit;
        readonly ILinearAlgebraProvider _cuda;
        readonly CudaComputationUnit _cudaComputationUnit;
        readonly ITestOutputHelper _output;

        static readonly uint VectorSize = 1000;
        static readonly uint RowCount = 200;
        static readonly uint ColumnCount = 200;

        public TensorTests2(ITestOutputHelper output)
        {
            _output = output;
            _computationUnit = _context.NewComputationUnit();
            _mklComputationUnit = new MklComputationUnit(_context);

            _cuda = _context.UseCudaLinearAlgebra(Path.Combine(Environment.CurrentDirectory, "cuda", "brightwire.ptx"));
            _cudaComputationUnit = new CudaComputationUnit(_context, (CudaProvider)_cuda);
        }

        public void Dispose()
        {
            _computationUnit.Dispose();
        }

        [Fact]
        public void TestCudaDirectInitialization()
        {
            var span = new ReadOnlySpan<float>(new float[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 2, 2);
            var segment = _cudaComputationUnit.CreateSegment((uint)span.Length);
            segment.CopyFrom(span, null);

            var test = segment.ToNewArray();
        }

        [Fact]
        public void Serialisation()
        {
            using var vector = _computationUnit.CreateVector(10, i => i + 1);
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            vector.WriteTo(writer);

            buffer.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);
            using var vector2 = _context.Create<IVector>(reader);
            vector2.Should().BeEquivalentTo(vector);
            vector2.Segment.Should().BeEquivalentTo(vector.Segment);
        }

        void Test<TT, R>(TT simple, TT mkl, TT cuda, Func<TT, TT, R> test, Func<R, R, bool> verifyResult)
            where TT: ITensor2
        {
            try {
                simple.Segment.CopyTo(mkl.Segment);
                simple.Segment.CopyTo(cuda.Segment);
                using var simple2 = (TT)simple.Clone();
                using var mkl2 = (TT)mkl.Clone();
                using var cuda2 = (TT)cuda.Clone();

                var ret = new (R Result, string Type, TimeSpan Time)[3];

                var sw = Stopwatch.StartNew();
                var r2 = test(mkl, mkl2);
                sw.Stop();
                ret[1] = (r2, "mkl", sw.Elapsed);

                sw.Restart();
                var r3 = test(cuda, cuda2);
                sw.Stop();
                ret[2] = (r3, "cuda", sw.Elapsed);
                
                sw.Restart();
                var r1 = test(simple, simple2);
                sw.Stop();
                ret[0] = (r1, "default", sw.Elapsed);

                // make sure that all pairs of results validate
                var firstResult = ret[0].Result;
                for(var i = 1; i < ret.Length; i++) {
                    var nextResult = ret[i].Result;
                    verifyResult(firstResult, nextResult).Should().BeTrue();
                }

                // log the results
                foreach(var item in ret.OrderBy(d => d.Time))
                    _output.WriteLine($"{item.Time} - {item.Type}");
            }
            finally {
                mkl.Dispose();
                cuda.Dispose();
            }
        }

        [Fact]
        public void DotProduct()
        {
            using var vector = _computationUnit.CreateVector(VectorSize);
            vector.MapIndexedInPlace((i, v) => i);

            Test(
                vector, 
                _mklComputationUnit.CreateVector(VectorSize), 
                _cudaComputationUnit.CreateVector(VectorSize), 
                (v1, v2) => v1.DotProduct(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixMultiply()
        {
            using var matrix = _computationUnit.CreateMatrix(RowCount, ColumnCount);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            Test(
                matrix, 
                _mklComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                _cudaComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                (v1, v2) => v1.Multiply(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixTranspose()
        {
            const int RowCount = 2, ColumnCount = 3;
            using var matrix = _computationUnit.CreateMatrix(RowCount, ColumnCount);
            matrix.MapIndexedInPlace((i, j, v) => i + j + 1);

            Test(
                matrix, 
                _mklComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                _cudaComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                (v1, v2) => v1.Transpose(),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixTransposeThisAndMultiply()
        {
            using var matrix = _computationUnit.CreateMatrix(RowCount, ColumnCount);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            Test(
                matrix, 
                _mklComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                _cudaComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                (v1, v2) => v1.TransposeThisAndMultiply(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixTransposeAndMultiply()
        {
            using var matrix = _computationUnit.CreateMatrix(RowCount, ColumnCount);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            Test(
                matrix, 
                _mklComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                _cudaComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                (v1, v2) => v1.TransposeAndMultiply(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void SVD()
        {
            const int RowCount = 2, ColumnCount = 2;
            using var matrix = _computationUnit.CreateMatrix(RowCount, ColumnCount);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            var cudaMatrix = _cudaComputationUnit.CreateMatrix(matrix.Segment, RowCount, ColumnCount);
            var (u, s, vt) = cudaMatrix.Svd();

            var mklMatrix = _mklComputationUnit.CreateMatrix(matrix.Segment, RowCount, ColumnCount);
            var (u2, s2, vt2) = mklMatrix.Svd();
        }

        [Fact]
        public void L2Norm()
        {
            using var vector = _computationUnit.CreateVector(VectorSize);
            vector.MapIndexedInPlace((i, v) => i);

            Test(
                vector, 
                _mklComputationUnit.CreateVector(VectorSize), 
                _cudaComputationUnit.CreateVector(VectorSize), 
                (v1, v2) => v1.L2Norm(),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void PointwiseMultiply()
        {
            using var vector = _computationUnit.CreateVector(VectorSize);
            vector.MapIndexedInPlace((i, v) => i);

            Test(
                vector, 
                _mklComputationUnit.CreateVector(VectorSize), 
                _cudaComputationUnit.CreateVector(VectorSize), 
                (v1, v2) => v1.PointwiseMultiply(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void PointwiseDivide()
        {
            using var vector = _computationUnit.CreateVector(VectorSize);
            vector.MapIndexedInPlace((i, v) => i+1);

            Test(
                vector, 
                _mklComputationUnit.CreateVector(VectorSize), 
                _cudaComputationUnit.CreateVector(VectorSize), 
                (v1, v2) => v1.PointwiseDivide(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }
    }
}
