using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Cuda;
using BrightData.Helper;
using BrightData.Numerics;
using FluentAssertions;
using Xunit;

namespace BrightWire.UnitTests
{
    public class VectorTests : IDisposable
    {
        readonly IBrightDataContext _context;
        readonly ILinearAlgebraProvider _cuda;
        readonly ILinearAlgebraProvider _cpu;

        public VectorTests()
        {
            var cwd = Environment.CurrentDirectory;

            _context = new BrightDataContext();
            _cpu = _context.UseNumericsLinearAlgebra();
            _cuda = _context.UseCudaLinearAlgebra(Path.Combine(cwd, "brightwire.ptx"));
        }

        public void Dispose()
        {
            _cuda.Dispose();
            _cpu.Dispose();
            _context.Dispose();
        }

        [Fact]
        public void TestVectorCreation()
        {
            var values = Enumerable.Range(0, 10).Select(v => (float)v).ToList();

            var a = _cpu.CreateVector(values).AsIndexable();
            a[4].Should().Be(4f);
            a[0].Should().Be(0f);
            a[9].Should().Be(9f);

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(values))
                gpuResults = gpuA.AsIndexable();
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
        }
    }
}
