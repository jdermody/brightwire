using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BrightData.Cuda;
using BrightData.MKL;
using BrightData2;
using Xunit;

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

        public TensorTests2()
        {
            _computationUnit = _context.NewComputationUnit();
            _mklComputationUnit = new MklComputationUnit(_context);

            _cuda = _baseContext.UseCudaLinearAlgebra(Path.Combine(Environment.CurrentDirectory, "cuda", "brightwire.ptx"));
            _cudaComputationUnit = new CudaComputationUnit(_context, (CudaProvider)_cuda);
        }

        public void Dispose()
        {
            _computationUnit.Dispose();
        }

        [Fact]
        public void TestVector()
        {
            const int SIZE = 100000;
            using var vector = _computationUnit.CreateVector(SIZE);
            using var vector2 = _mklComputationUnit.CreateVector(SIZE);
            using var vector3 = _cudaComputationUnit.CreateVector(SIZE);
            using var vector4 = _cudaComputationUnit.CreateVector(SIZE);

            vector.Segment.CopyFrom(SIZE.AsRange().Select(i => (float)i).ToArray());
            var ptr = vector.Segment.GetSpan();
            vector2.Segment.CopyFrom(ptr);
            vector3.Segment.CopyFrom(ptr);
            vector4.Segment.CopyFrom(ptr);

            var sw = Stopwatch.StartNew();
            var result = vector.DotProduct(vector2);
            sw.Stop();
            var t1 = sw.Elapsed;

            sw.Restart();
            var result2 = vector2.DotProduct(vector);
            sw.Stop();
            var t2 = sw.Elapsed;

            sw.Restart();
            var result3 = vector3.DotProduct(vector4);
            sw.Stop();
            var t3 = sw.Elapsed;
        }
    }
}
