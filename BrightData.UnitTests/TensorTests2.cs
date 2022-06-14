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
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace BrightData.UnitTests
{
    public class TensorTests2 : IDisposable
    {
        readonly BrightDataContext _baseContext = new(0);
        readonly BrightDataContext2 _context = new(0);
        readonly ComputationUnit _computationUnit;
        readonly MklComputationUnit _mklComputationUnit;
        readonly ILinearAlgebraProvider _cuda;
        readonly CudaComputationUnit _cudaComputationUnit;
        readonly ITestOutputHelper _output;

        public TensorTests2(ITestOutputHelper output)
        {
            _output = output;
            _computationUnit = _context.NewComputationUnit();
            _mklComputationUnit = new MklComputationUnit(_context);

            _cuda = _baseContext.UseCudaLinearAlgebra(Path.Combine(Environment.CurrentDirectory, "cuda", "brightwire.ptx"));
            _cudaComputationUnit = new CudaComputationUnit(_context, (CudaProvider)_cuda);
        }

        public void Dispose()
        {
            _computationUnit.Dispose();
        }

        (R Result, string Type, TimeSpan Time)[] Test<TT, R>(TT simple, TT mkl, TT cuda, Func<TT, TT, R> test, Func<R, R, bool> verifyResult)
            where TT: ITensor2
        {
            try {
                simple.Segment.CopyTo(mkl.Segment);
                simple.Segment.CopyTo(cuda.Segment);
                using var cuda2 = (TT)cuda.Clone();

                var ret = new (R Result, string Type, TimeSpan Time)[3];
                var sw = Stopwatch.StartNew();
                var r1 = test(simple, mkl);
                sw.Stop();
                ret[0] = (r1, "default", sw.Elapsed);

                sw.Restart();
                var r2 = test(mkl, simple);
                sw.Stop();
                ret[1] = (r2, "mkl", sw.Elapsed);

                sw.Restart();
                var r3 = test(cuda, cuda2);
                sw.Stop();
                ret[2] = (r3, "cuda", sw.Elapsed);

                // make sure that all pairs of results validate
                var firstResult = ret[0].Result;
                for(var i = 1; i < ret.Length; i++) {
                    var nextResult = ret[i].Result;
                    verifyResult(firstResult, nextResult).Should().BeTrue();
                }

                // log the results
                foreach(var item in ret.OrderBy(d => d.Time))
                    _output.WriteLine($"{item.Time} - {item.Type}");

                return ret;
            }
            finally {
                mkl.Dispose();
                cuda.Dispose();
            }
        }

        [Fact]
        public void TestDotProduct()
        {
            const int SIZE = 100000;
            using var vector = _computationUnit.CreateVector(SIZE);
            vector.MapIndexedInPlace((i, v) => i);

            Test(
                vector, 
                _mklComputationUnit.CreateVector(SIZE), 
                _cudaComputationUnit.CreateVector(SIZE), 
                (v1, v2) => v1.DotProduct(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixMultiply()
        {
            const int RowCount = 200, ColumnCount = 200;
            using var matrix = _computationUnit.CreateMatrix(RowCount, ColumnCount);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            var results = Test(
                matrix, 
                _mklComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                _cudaComputationUnit.CreateMatrix(RowCount, ColumnCount), 
                (v1, v2) => v1.Multiply(v2),
                (r1, r2) => FloatMath.AreApproximatelyEqual(r1, r2)
            );
        }
    }
}
