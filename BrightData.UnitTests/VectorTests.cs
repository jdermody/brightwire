using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.Helper;
using BrightData.LinearAlegbra2;
using BrightData.UnitTests.Helper;
using Xunit;
using FluentAssertions;

namespace BrightData.UnitTests
{
    public class VectorTests : CudaBase
    {
        static IVector Apply(LinearAlgebraProvider lap, IVector a, IVector b, Func<IVector, IVector, IVector> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            var otherC = func(otherA, otherB);
            return otherC;
        }

        static IVector Apply(LinearAlgebraProvider lap, IVector a, IVector b, Action<IVector, IVector> func)
        {
            var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            func(otherA, otherB);
            return otherA;
        }

        static IVector Apply(LinearAlgebraProvider lap, IVector a, Action<IVector> func)
        {
            var otherA = lap.CreateVector(a);
            func(otherA);
            return otherA;
        }

        static IVector Apply(LinearAlgebraProvider lap, IVector a, Func<IVector, IVector> func)
        {
            using var otherA = lap.CreateVector(a);
            var otherB = func(otherA);
            return otherB;
        }

        static T Apply<T>(LinearAlgebraProvider lap, IVector a, Func<IVector, T> func)
        {
            using var otherA = lap.CreateVector(a);
            return func(otherA);
        }

        static float Apply(LinearAlgebraProvider lap, IVector a, IVector b, Func<IVector, IVector, float> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            return func(otherA, otherB);
        }

        static IMatrix Apply(LinearAlgebraProvider lap, IVector a, Func<IVector, IMatrix> func)
        {
            using var otherA = lap.CreateVector(a);
            var otherB = func(otherA);
            return otherB;
        }

        [Fact]
        public void TestVectorCreation()
        {
            var values = Enumerable.Range(0, 10).Select(v => (float)v).ToArray();

            using var a = _lap.CreateVector(values);
            a[4].Should().Be(4f);
            a[0].Should().Be(0f);
            a[9].Should().Be(9f);

            using var b = _cuda.CreateVector(values);
            FloatMath.AreApproximatelyEqual(a, b).Should().BeTrue();
        }

        [Fact]
        public void TestManhattanDistance()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);
            var vectors = Enumerable.Range(0, 10).Select(_ => _lap.CreateVector(100, _ => distribution.Sample())).ToArray();
            var compareTo = Enumerable.Range(0, 20).Select(_ => _lap.CreateVector(100, _ => distribution.Sample())).ToArray();
            var gpuVectors = vectors.Select(v => _cuda.CreateVector(v.Segment)).ToArray();
            var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v.Segment)).ToArray();
            try {
                using var distances = _lap.FindDistances(vectors, compareTo, DistanceMetric.Manhattan);
                using var gpuDistances = _cuda.FindDistances(gpuVectors, gpuCompareTo, DistanceMetric.Manhattan);
                FloatMath.AreApproximatelyEqual(distances, gpuDistances).Should().BeTrue();
            }
            finally {
                vectors.DisposeAll();
                compareTo.DisposeAll();
                gpuVectors.DisposeAll();
                gpuCompareTo.DisposeAll();
            }
        }

        [Fact]
        public void TestCosineDistance()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);
            var vectors = Enumerable.Range(0, 10).Select(_ => _lap.CreateVector(100, _ => distribution.Sample())).ToArray();
            var compareTo = Enumerable.Range(0, 20).Select(_ => _lap.CreateVector(100, _ => distribution.Sample())).ToArray();
            var gpuVectors = vectors.Select(v => _cuda.CreateVector(v)).ToArray();
            var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v)).ToArray();
            try {
                var distances = _lap.FindDistances(vectors, compareTo, DistanceMetric.Cosine);
                var gpuDistances = _cuda.FindDistances(gpuVectors, gpuCompareTo, DistanceMetric.Cosine);
                FloatMath.AreApproximatelyEqual(distances, gpuDistances).Should().BeTrue();
            }
            finally {
                vectors.DisposeAll();
                compareTo.DisposeAll();
                gpuVectors.DisposeAll();
                gpuCompareTo.DisposeAll();
            }
        }

        [Fact]
        public void VectorColumnMatrix()
        {
            using var a = _lap.CreateVector(5, i => i);
            using var matrix = a.Reshape(null, 1);
            matrix.ColumnCount.Should().Be(1);
            matrix.RowCount.Should().Be(5);

            using var gpuResults = Apply(_cuda, a, a => a.Reshape(null, 1));
            FloatMath.AreApproximatelyEqual(matrix, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorRowMatrix()
        {
            using var a = _lap.CreateVector(5, i => i);
            using var matrix = a.Reshape(1, null);
            matrix.ColumnCount.Should().Be(5);
            matrix.RowCount.Should().Be(1);

            using var gpuResults = Apply(_cuda, a, a => a.Reshape(1, null));
            FloatMath.AreApproximatelyEqual(matrix, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorAdd()
        {
            using var a = _lap.CreateVector(5, i => i);
            using var b = _lap.CreateVector(5, i => i * 2);
            using var c = a.Add(b);

            using var gpuResults = Apply(_cuda, a, b, (a, b) => a.Add(b));
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSubtract()
        {
            using var a = _lap.CreateVector(5, i => i);
            using var b = _lap.CreateVector(5, i => i * 2);
            using var c = a.Subtract(b);

            using var gpuResults = Apply(_cuda, a, b, (a, b) => a.Subtract(b));
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorPointwiseMultiply()
        {
            using var a = _lap.CreateVector(5, i => i);
            using var b = _lap.CreateVector(5, i => i * 2);
            using var c = a.PointwiseMultiply(b);

            using var gpuResults = Apply(_cuda, a, b, (a, b) => a.PointwiseMultiply(b));
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorDotProduct()
        {
            using var a = _lap.CreateVector(5, i => i);
            using var b = _lap.CreateVector(5, i => i * 2);
            var cpu = a.DotProduct(b);

            var gpu = Apply(_cuda, a, b, (a, b) => a.DotProduct(b));
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();
        }

        [Fact]
        public void VectorL2Norm()
        {
            using var a = _lap.CreateVector(5, i => i);
            var cpu = a.L2Norm();

            var gpu = Apply(_cuda, a, a => a.L2Norm());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();
        }

        [Fact]
        public void VectorMaximumIndex()
        {
            using var a = _lap.CreateVector(1.0f, 2.0f, 1.0f, 1.0f, -5f);
            var cpu = a.GetMaxIndex();

            var gpu = Apply(_cuda, a, a => a.GetMaxIndex());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();
        }

        [Fact]
        public void VectorMinimumIndex()
        {
            using var a = _lap.CreateVector(3.0f, -2.0f, 1.0f, 2.0f);
            var cpu = a.GetMinIndex();

            var gpu = Apply(_cuda, a, a => a.GetMinIndex());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();
        }

        [Fact]
        public void VectorAddInPlace()
        {
            using var a = _lap.CreateVector(5, i => i * 2);
            using var b = _lap.CreateVector(5, i => i);

            using var gpuResults = Apply(_cuda, a, b, (a, b) => a.AddInPlace(b, 2, 3));
            a.AddInPlace(b, 2, 3);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSubtractInPlace()
        {
            using var a = _lap.CreateVector(5, i => i * 2);
            using var b = _lap.CreateVector(5, i => i);

            var gpuResults = Apply(_cuda, a, b, (a, b) => a.SubtractInPlace(b, 2, 3));
            a.SubtractInPlace(b, 2, 3);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSqrt()
        {
            using var a = _lap.CreateVector(10, i => i * 2);
            using var cpuResults = a.Sqrt();

            using var gpuResults = Apply(_cuda, a, a => a.Sqrt());
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorGetNewVectorFromIndices()
        {
            using var a = _lap.CreateVector(10, i => i * 2);
            var array = new uint[] { 2, 3, 5 };
            using var cpuResults = a.CherryPick(array);

            using var gpuResults = Apply(_cuda, a, a => a.CherryPick(array));
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorCopyTo()
        {
            using var cpuResults = _lap.CreateVector(10, i => i * 2);
            using var b = _lap.CreateVector(10, 0);
            cpuResults.CopyTo(b);
            FloatMath.AreApproximatelyEqual(cpuResults, b);

            using var gpuA = _cuda.CreateVector(cpuResults);
            using var gpuB = _cuda.CreateVector(10, 0);
            gpuA.CopyTo(gpuB);
            FloatMath.AreApproximatelyEqual(b, gpuB).Should().BeTrue();
        }

        [Fact]
        public void VectorClone()
        {
            using var a = _lap.CreateVector(5, i => i);
            var cpuResults = a.Clone();
            FloatMath.AreApproximatelyEqual(a, cpuResults);

            var gpuResults = Apply(_cuda, a, a => a.Clone());
            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorMultiply()
        {
            using var a = _lap.CreateVector(5, i => i);
            const float OPERAND = 2f;

            using var gpuResults = Apply(_cuda, a, a => a.Multiply(OPERAND));
            using var cpuResults = a.Multiply(OPERAND);
            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();
        }

        //[Fact]
        //public void VectorReadWrite()
        //{
        //    var a = _lap.CreateVector(5, i => i);

        //    using var buffer = new MemoryStream();
        //    using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
        //    using var reader = new BinaryReader(buffer, Encoding.UTF8, true);
        //    a.WriteTo(writer);
        //    writer.Flush();
        //    buffer.Position = 0;

        //    var b = _context.ReadVectorFrom(reader);
        //    FloatMath.AreApproximatelyEqual(a, b).Should().BeTrue();

        //    // test Numerics -> Cuda serialisation
        //    using var c = _cuda.CreateVector(serialised);
        //    FloatMath.AreApproximatelyEqual(a, c).Should().BeTrue();

        //    // test Cuda -> Cuda serialisation
        //    var serialised2 = c.Data;
        //    using (var d = _cuda.CreateVector(serialised2))
        //        FloatMath.AreApproximatelyEqual(a, d).Should().BeTrue();

        //    // test Cuda -> Numerics serialisation
        //    var e = _cpu.CreateVector(c.Data);
        //    FloatMath.AreApproximatelyEqual(a, e).Should().BeTrue();
        //}

        [Fact]
        public void VectorEuclideanDistance()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(500, _ => distribution.Sample());
            using var b = _lap.CreateVector(500, _ => distribution.Sample());
            var cpuDistance = a.EuclideanDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.EuclideanDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorCosineDistance()
        {
            var rand = new Random(0);

            using var a = _lap.CreateVector(5000, _ => rand.NextSingle());
            using var b = _lap.CreateVector(5000, _ => rand.NextSingle());
            var cpuDistance = a.CosineDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.CosineDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorManhattanDistance()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            using var b = _lap.CreateVector(5000, _ => distribution.Sample());
            var cpuDistance = a.ManhattanDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.ManhattanDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorMeanSquaredDistance()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(1000, _ => distribution.Sample());
            using var b = _lap.CreateVector(1000, _ => distribution.Sample());
            var cpuDistance = a.MeanSquaredDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.MeanSquaredDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorSquaredEuclideanDistance()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(1000, _ => distribution.Sample());
            using var b = _lap.CreateVector(1000, _ => distribution.Sample());
            var cpuDistance = a.SquaredEuclideanDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.SquaredEuclideanDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorMinMax()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            var (min, max, minIndex, maxIndex) = a.GetMinAndMaxValues();

            var gpuMinMax = Apply(_cuda, a, a => a.GetMinAndMaxValues());
            FloatMath.AreApproximatelyEqual(min, gpuMinMax.Min).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(max, gpuMinMax.Max).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(minIndex, gpuMinMax.MinIndex).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(maxIndex, gpuMinMax.MaxIndex).Should().BeTrue();
        }

        [Fact]
        public void VectorAverage()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            var cpuAverage = a.Average();

            var gpuAverage = Apply(_cuda, a, a => a.Average());
            FloatMath.AreApproximatelyEqual(cpuAverage, gpuAverage, 7).Should().BeTrue();
        }

        [Fact]
        public void VectorL1Norm()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.L1Norm();

            var gpu = Apply(_cuda, a, a => a.L1Norm());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();
        }

        [Fact]
        public void VectorAbs()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            var cpuResults = a.Abs();

            var gpuResults = Apply(_cuda, a, a => a.Abs());
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorLog()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            var cpuResults = a.Log();

            var gpuResults = Apply(_cuda, a, a => a.Log());
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults, 14).Should().BeTrue();
        }

        [Fact]
        public void VectorStdDev()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.StdDev(null);

            var gpu = Apply(_cuda, a, a => a.StdDev(null));
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();
        }

        //void TestNormalise(NormalizationType type)
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);

        //    using var a = _lap.CreateVector(5000, _ => distribution.Sample());
        //    using (var gpuA = _cuda.CreateVector(a)) {
        //        gpuA.(type);
        //        gpuResults = gpuA;
        //    }
        //    using (var simpleA = _simple.CreateVector(a)) {
        //        simpleA.Normalize(type);
        //        simpleResults = simpleA;
        //    }
        //    a.Normalize(type);
        //    var cpuResults = a;
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults, 12).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults, 12).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorFeatureScaleNormalise()
        //{
        //    TestNormalise(NormalizationType.FeatureScale);
        //}

        //[Fact]
        //public void VectorStandardNormalise()
        //{
        //    TestNormalise(NormalizationType.Standard);
        //}

        //[Fact]
        //public void VectorManhattanNormalise()
        //{
        //    TestNormalise(NormalizationType.Manhattan);
        //}

        //[Fact]
        //public void VectorEuclideanNormalise()
        //{
        //    TestNormalise(NormalizationType.Euclidean);
        //}

        static IVector TestMultiDistance(LinearAlgebraProvider lap, IVector a, IVector b, IVector c, DistanceMetric distanceMetric)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            using var otherC = lap.CreateVector(c);
            return otherA.FindDistances(new[] { otherB, otherC }, distanceMetric);
        }

        [Fact]
        public void MultiEuclideanDistance()
        {
            var distribution = _context.CreateNormalDistribution(0, 5);

            using var a = _lap.CreateVector(5000, _ => distribution.Sample());
            using var b = _lap.CreateVector(5000, _ => distribution.Sample());
            using var c = _lap.CreateVector(5000, _ => distribution.Sample());
            using var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Euclidean);

            using var gpuDistance = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Euclidean);
            FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();
        }

        //[Fact]
        //public void MultiManhattanDistance()
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);

        //    var a = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var b = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var c = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Manhattan);

        //    IIndexableFloatVector gpuDistance = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Manhattan);
        //    FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

        //    IIndexableFloatVector simpleDistance = TestMultiDistance(_simple, a, b, c, DistanceMetric.Manhattan);
        //    FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        //}

        //[Fact]
        //public void MultiCosineDistance()
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);

        //    var a = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var b = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var c = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Cosine);

        //    IIndexableFloatVector gpuDistance = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Cosine);
        //    FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

        //    IIndexableFloatVector simpleDistance = TestMultiDistance(_simple, a, b, c, DistanceMetric.Cosine);
        //    FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        //}

        //[Fact]
        //public void MultiCosineDistance2()
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);
        //    float[]? dataNorm1 = null;
        //    float[]? dataNorm2 = null;
        //    float[]? dataNorm3 = null;

        //    var a = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var b = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var c = _cpu.CreateVector(5000, _ => distribution.Sample());
        //    var distance = a.CosineDistance(new[] { b, c }, ref dataNorm1);

        //    IIndexableFloatVector gpuDistance;
        //    using (var gpuA = _cuda.CreateVector(a))
        //    using (var gpuB = _cuda.CreateVector(b))
        //    using (var gpuC = _cuda.CreateVector(c))
        //    using (var temp = gpuA.CosineDistance(new[] { gpuB, gpuC }, ref dataNorm2))
        //        gpuDistance = temp;
        //    FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

        //    IIndexableFloatVector simpleDistance;
        //    using (var simpleA = _simple.CreateVector(a))
        //    using (var simpleB = _simple.CreateVector(b))
        //    using (var simpleC = _simple.CreateVector(c))
        //    using (var temp = simpleA.CosineDistance(new[] { simpleB, simpleC }, ref dataNorm3))
        //        simpleDistance = temp;
        //    FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorAddScalar()
        //{
        //    var a = _cpu.CreateVector(1000, i => i);

        //    IIndexableFloatVector gpuResults = Apply(_cuda, a, a => a.Add(0.5f));
        //    IIndexableFloatVector simpleResults = Apply(_simple, a, a => a.Add(0.5f));

        //    a.Add(0.5f);
        //    FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(a, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorSigmoid()
        //{
        //    var a = _cpu.CreateVector(1000, i => i);
        //    var cpuResults = a.Sigmoid();

        //    IIndexableFloatVector gpuResults = Apply(_cuda, a, a => a.Sigmoid());
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    IIndexableFloatVector simpleResults = Apply(_cuda, a, a => a.Sigmoid());
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void MatrixVectorMultiply()
        //{
        //    var a = _cpu.CreateMatrix(256, 256, (x, y) => x * y);
        //    var b = _cpu.CreateVector(256, i => i * 0.5f);
        //    var cpuResults = a.Multiply(b);

        //    IIndexableFloatMatrix gpuResults;
        //    using (var gpuA = _cuda.CreateMatrix(a))
        //    using (var gpuB = _cuda.CreateVector(b))
        //    using (var gpuC = gpuA.Multiply(gpuB)) {
        //        gpuResults = gpuC;
        //    }
        //    FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

        //    IIndexableFloatMatrix simpleResults;
        //    using (var simpleA = _simple.CreateMatrix(a))
        //    using (var simpleB = _simple.CreateVector(b))
        //    using (var simpleC = simpleA.Multiply(simpleB)) {
        //        simpleResults = simpleC;
        //    }
        //    FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorSplit()
        //{
        //    const int BLOCK_COUNT = 3;
        //    var a = _cpu.CreateVector(12, i => i);
        //    var cpuResult = a.Split(BLOCK_COUNT).Select(v => v).ToList();

        //    var gpuResult = new List<IIndexableFloatVector>();
        //    using (var gpuA = _cuda.CreateVector(a)) {
        //        var split = gpuA.Split(BLOCK_COUNT);
        //        foreach (var item in split) {
        //            gpuResult.Add(item);
        //            item.Dispose();
        //        }
        //    }
        //    for (var i = 0; i < cpuResult.Count; i++) {
        //        cpuResult[i].Count.Should().Be(4);
        //        FloatMath.AreApproximatelyEqual(cpuResult[i], gpuResult[i]).Should().BeTrue();
        //    }

        //    var simpleResult = new List<IIndexableFloatVector>();
        //    using (var simpleA = _simple.CreateVector(a)) {
        //        var split = simpleA.Split(BLOCK_COUNT);
        //        foreach (var item in split) {
        //            simpleResult.Add(item);
        //            item.Dispose();
        //        }
        //    }
        //    for (var i = 0; i < cpuResult.Count; i++) {
        //        FloatMath.AreApproximatelyEqual(cpuResult[i], simpleResult[i]).Should().BeTrue();
        //    }
        //}

        //[Fact]
        //public void VectorSoftMax()
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);
        //    var a = _cpu.CreateVector(128, _ => (float)distribution.Sample());
        //    var cpuResult = a.Softmax();

        //    IIndexableFloatVector gpuResult = Apply(_cuda, a, a => a.Softmax());
        //    FloatMath.AreApproximatelyEqual(gpuResult, cpuResult).Should().BeTrue();

        //    IIndexableFloatVector simpleResult = Apply(_simple, a, a => a.Softmax());
        //    FloatMath.AreApproximatelyEqual(simpleResult, cpuResult).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorSoftMaxDerivative()
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);
        //    var a = _cpu.CreateVector(128, _ => (float)distribution.Sample());
        //    var cpuResult = a.SoftmaxDerivative();

        //    IIndexableFloatMatrix gpuResult;
        //    using (var gpuA = _cuda.CreateVector(a)) {
        //        using var softmaxDerivative = gpuA.SoftmaxDerivative();
        //        gpuResult = softmaxDerivative;
        //    }
        //    FloatMath.AreApproximatelyEqual(gpuResult, cpuResult).Should().BeTrue();

        //    IIndexableFloatMatrix simpleResult;
        //    using (var simpleA = _simple.CreateVector(a)) {
        //        using var softmaxDerivative = simpleA.SoftmaxDerivative();
        //        simpleResult = softmaxDerivative;
        //    }
        //    FloatMath.AreApproximatelyEqual(simpleResult, cpuResult).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorReverse()
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);
        //    var a = _cpu.CreateVector(128, _ => (float)distribution.Sample());
        //    var cpuResult = a.Reverse();

        //    IIndexableFloatVector gpuResult = Apply(_cuda, a, a => a.Reverse());
        //    FloatMath.AreApproximatelyEqual(gpuResult, cpuResult).Should().BeTrue();

        //    IIndexableFloatVector simpleResult = Apply(_simple, a, a => a.Reverse());
        //    FloatMath.AreApproximatelyEqual(simpleResult, cpuResult).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorRotate()
        //{
        //    var cpu = _cpu.CreateVector(4, i => i + 1);
        //    using var gpuA = _cuda.CreateVector(cpu);
        //    using var simpleA = _simple.CreateVector(cpu);
        //    cpu.RotateInPlace();
        //    gpuA.RotateInPlace();
        //    simpleA.RotateInPlace();
        //    FloatMath.AreApproximatelyEqual(gpuA, cpu).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleA, cpu).Should().BeTrue();
        //}

        //[Fact]
        //public void VectorRotate2()
        //{
        //    const int BLOCK_COUNT = 2;
        //    var cpu = _cpu.CreateVector(8, i => i + 1);
        //    using var gpuA = _cuda.CreateVector(cpu);
        //    using var simpleA = _simple.CreateVector(cpu);
        //    cpu.RotateInPlace(BLOCK_COUNT);
        //    gpuA.RotateInPlace(BLOCK_COUNT);
        //    simpleA.RotateInPlace(BLOCK_COUNT);
        //    FloatMath.AreApproximatelyEqual(gpuA, cpu).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleA, cpu).Should().BeTrue();
        //}

        //[Fact]
        //public void TestFinite()
        //{
        //    var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, -1f);
        //    vector.IsEntirelyFinite().Should().BeTrue();

        //    using var gpuVector = _cuda.CreateVector(vector);
        //    gpuVector.IsEntirelyFinite().Should().BeTrue();

        //    using var simpleVector = _simple.CreateVector(vector);
        //    simpleVector.IsEntirelyFinite().Should().BeTrue();
        //}

        //[Fact]
        //public void TestFinite2()
        //{
        //    var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, -1f, float.Epsilon);
        //    vector.IsEntirelyFinite().Should().BeTrue();

        //    using var gpuVector = _cuda.CreateVector(vector);
        //    gpuVector.IsEntirelyFinite().Should().BeTrue();

        //    using var simpleVector = _simple.CreateVector(vector);
        //    simpleVector.IsEntirelyFinite().Should().BeTrue();
        //}

        //[Fact]
        //public void TestNotFinite()
        //{
        //    var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.NaN);
        //    vector.IsEntirelyFinite().Should().BeFalse();

        //    using var gpuVector = _cuda.CreateVector(vector);
        //    gpuVector.IsEntirelyFinite().Should().BeFalse();

        //    using var simpleVector = _simple.CreateVector(vector);
        //    simpleVector.IsEntirelyFinite().Should().BeFalse();
        //}

        //[Fact]
        //public void TestNotFinite2()
        //{
        //    var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.NegativeInfinity);
        //    vector.IsEntirelyFinite().Should().BeFalse();

        //    using var gpuVector = _cuda.CreateVector(vector);
        //    gpuVector.IsEntirelyFinite().Should().BeFalse();

        //    using var simpleVector = _simple.CreateVector(vector);
        //    simpleVector.IsEntirelyFinite().Should().BeFalse();
        //}

        //[Fact]
        //public void TestNotFinite3()
        //{
        //    var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.PositiveInfinity);
        //    vector.IsEntirelyFinite().Should().BeFalse();

        //    using var gpuVector = _cuda.CreateVector(vector);
        //    gpuVector.IsEntirelyFinite().Should().BeFalse();

        //    using var simpleVector = _simple.CreateVector(vector);
        //    simpleVector.IsEntirelyFinite().Should().BeFalse();
        //}

        //[Fact]
        //public void TestRoundInPlace()
        //{
        //    using var vector = _cpu.CreateVector(0.5f, 0.75f, 1f, 1.5f, 0.25f, 0.1f, 0f, -1f);
        //    using var gpuVector = _cuda.CreateVector(vector);
        //    using var simpleVector = _simple.CreateVector(vector);

        //    vector.RoundInPlace();
        //    gpuVector.RoundInPlace();
        //    simpleVector.RoundInPlace();
        //    FloatMath.AreApproximatelyEqual(gpuVector, vector).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleVector, vector).Should().BeTrue();

        //    var data = vector.Data;
        //    data[0].Should().Be(1f);
        //    data[1].Should().Be(1f);
        //    data[2].Should().Be(1f);
        //    data[3].Should().Be(1f);
        //    data[4].Should().Be(0f);
        //    data[5].Should().Be(0f);
        //    data[6].Should().Be(0f);
        //    data[7].Should().Be(0f);
        //}
    }
}
