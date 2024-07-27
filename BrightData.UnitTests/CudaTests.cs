using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra.Segments;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;

namespace BrightData.UnitTests
{
    public class CudaTests : CudaBase
    {
        [Fact]
        public void CopyToWrapperWithStride()
        {
            using var empty = _cuda.CreateSegment(24, true);
            using var ones = _cuda.CreateSegment(8, _ => 1);
            var wrapper = new MutableTensorSegmentWrapper<float>(empty, 0, 3, 8);
            ones.CopyTo(wrapper);
            empty.ToNewArray()[..6].Should().BeEquivalentTo([1, 0, 0, 1, 0, 0]);
        }

        void FindDistance(DistanceMetric distanceMetric)
        {
            using var gpuSegment = _cuda.CreateSegment(8, _ => _context.NextRandomFloat());
            var gpuSegments = 8.AsRange().Select(_ => _cuda.CreateSegment(8, _ => _context.NextRandomFloat())).ToArray();
            using var gpuDistance = _cuda.FindDistances(gpuSegment, gpuSegments, distanceMetric);

            using var cpuSegment = _cpu.CreateSegment(gpuSegment);
            var cpuSegments = gpuSegments.Select(_cpu.CreateSegment).ToArray();
            using var cpuDistance = _cpu.FindDistances(cpuSegment, gpuSegments, distanceMetric);

            foreach (var (g, c) in gpuDistance.Values.Zip(cpuDistance.Values))
                Math<float>.AreApproximatelyEqual(g, c).Should().BeTrue();

            foreach(var item in cpuSegments)
                item.Dispose();
            foreach(var item in gpuSegments)
                item.Dispose();
        }

        [Fact]
        public void FindCosineDistance()
        {
            FindDistance(DistanceMetric.Cosine);
        }

        [Fact]
        public void FindEuclideanDistance()
        {
            FindDistance(DistanceMetric.Euclidean);
        }

        [Fact]
        public void FindManhattanDistance()
        {
            FindDistance(DistanceMetric.Manhattan);
        }
    }
}
