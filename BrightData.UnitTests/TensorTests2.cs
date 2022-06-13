using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BrightData.MKL;
using BrightData2;
using Xunit;

namespace BrightData.UnitTests
{
    public class TensorTests2 : IDisposable
    {
        readonly BrightDataContext2 _context = new(0);
        readonly ComputationUnit _computationUnit;
        readonly ComputationUnit _mklComputationUnit;

        public TensorTests2()
        {
            _computationUnit = _context.NewComputationUnit();
            _mklComputationUnit = new MklComputationUnit(_context);
        }

        public void Dispose()
        {
            _computationUnit.Dispose();
        }

        [Fact]
        public void TestVector()
        {
            const int SIZE = 100000;
            var vector = _computationUnit.CreateVector(SIZE);
            var vector2 = _mklComputationUnit.CreateVector(SIZE);
            vector.Segment.CopyFrom(SIZE.AsRange().Select(i => (float)i).ToArray());
            vector2.Segment.CopyFrom(vector.Segment.GetSpan());

            var sw = Stopwatch.StartNew();
            var result = vector.DotProduct(vector2);
            sw.Stop();
            var t1 = sw.Elapsed;

            sw.Restart();
            var result2 = vector2.DotProduct(vector);
            sw.Stop();
            var t2 = sw.Elapsed;
        }
    }
}
