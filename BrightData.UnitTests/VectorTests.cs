﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.UnitTests.Helper;
using Xunit;
using FluentAssertions;

namespace BrightData.UnitTests
{
    public class VectorTests : CudaBase
    {
        static IVector<float> Apply(LinearAlgebraProvider<float> lap, IVector<float> a, IVector<float> b, Func<IVector<float>, IVector<float>, IVector<float>> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            var otherC = func(otherA, otherB);
            return otherC;
        }

        static IVector<float> Apply(LinearAlgebraProvider<float> lap, IVector<float> a, IVector<float> b, Action<IVector<float>, IVector<float>> func)
        {
            var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            func(otherA, otherB);
            return otherA;
        }

        static IVector<float> Apply(LinearAlgebraProvider<float> lap, IVector<float> a, Action<IVector<float>> func)
        {
            var otherA = lap.CreateVector(a);
            func(otherA);
            return otherA;
        }

        static IVector<float> Apply(LinearAlgebraProvider<float> lap, IVector<float> a, Func<IVector<float>, IVector<float>> func)
        {
            using var otherA = lap.CreateVector(a);
            var otherB = func(otherA);
            return otherB;
        }

        static T Apply<T>(LinearAlgebraProvider<float> lap, IVector<float> a, Func<IVector<float>, T> func)
        {
            using var otherA = lap.CreateVector(a);
            return func(otherA);
        }

        static float Apply(LinearAlgebraProvider<float> lap, IVector<float> a, IVector<float> b, Func<IVector<float>, IVector<float>, float> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            return func(otherA, otherB);
        }

        static IMatrix<float> Apply(LinearAlgebraProvider<float> lap, IVector<float> a, Func<IVector<float>, IMatrix<float>> func)
        {
            using var otherA = lap.CreateVector(a);
            var otherB = func(otherA);
            return otherB;
        }

        [Fact]
        public void TestVectorCreation()
        {
            var values = Enumerable.Range(0, 10).Select(v => (float)v).ToArray();

            var a = _cpu.CreateVector(values);
            a[4].Should().Be(4f);
            a[0].Should().Be(0f);
            a[9].Should().Be(9f);

            AssertSameAndThenDispose(a, _cuda.CreateVector(values), _mkl.CreateVector(values));
        }

        void TestDistances(DistanceMetric distanceMetric)
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);
            var vectors = Enumerable.Range(0, 10).Select(_ => (IReadOnlyNumericSegment<float>)_cpu.CreateSegment(100, _ => distribution.Sample())).ToArray();
            var compareTo = Enumerable.Range(0, 20).Select(_ => (IReadOnlyNumericSegment<float>)_cpu.CreateSegment(100, _ => distribution.Sample())).ToArray();

            var gpuVectors = vectors.Select(_cuda.CreateSegment).ToArray();
            var gpuCompareTo = compareTo.Select(_cuda.CreateSegment).ToArray();

            try {
                AssertSameAndThenDispose(
                    _cpu.FindDistances(vectors, compareTo, distanceMetric), 
                    _cuda.FindDistances(gpuVectors, gpuCompareTo, distanceMetric), 
                    _mkl.FindDistances(vectors, compareTo, distanceMetric)
                );
            }
            finally {
                vectors.DisposeAll();
                compareTo.DisposeAll();
                gpuVectors.DisposeAll();
                gpuCompareTo.DisposeAll();
            }
        }

        [Fact]
        public void TestManhattanDistance()
        {
            TestDistances(DistanceMetric.Manhattan);
        }

        [Fact]
        public void TestCosineDistance()
        {
            TestDistances(DistanceMetric.Cosine);
        }

        [Fact]
        public void VectorColumnMatrix()
        {
            using var a = _cpu.CreateVector(5, i => i);
            using var cpu = a.Reshape(null, 1);
            cpu.ColumnCount.Should().Be(1);
            cpu.RowCount.Should().Be(5);

            using var gpu = Apply(_cuda, a, a => a.Reshape(null, 1));
            using var mkl = Apply(_mkl, a, a => a.Reshape(null, 1));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorRowMatrix()
        {
            using var a = _cpu.CreateVector(5, i => i);
            using var cpu = a.Reshape(1, null);
            cpu.ColumnCount.Should().Be(5);
            cpu.RowCount.Should().Be(1);

            using var gpu = Apply(_cuda, a, a => a.Reshape(1, null));
            using var mkl = Apply(_mkl, a, a => a.Reshape(1, null));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorAdd()
        {
            using var a = _cpu.CreateVector(5, i => i);
            using var b = _cpu.CreateVector(5, i => i * 2);
            using var cpu = a.Add(b);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.Add(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.Add(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorSubtract()
        {
            using var a = _cpu.CreateVector(5, i => i);
            using var b = _cpu.CreateVector(5, i => i * 2);
            using var cpu = a.Subtract(b);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.Subtract(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.Subtract(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorPointwiseMultiply()
        {
            using var a = _cpu.CreateVector(5, i => i);
            using var b = _cpu.CreateVector(5, i => i * 2);
            using var cpu = a.PointwiseMultiply(b);

            using var gpu = Apply(_cuda, a, b, (a, b) => a.PointwiseMultiply(b));
            using var mkl = Apply(_mkl, a, b, (a, b) => a.PointwiseMultiply(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorDotProduct()
        {
            using var a = _cpu.CreateVector(5, i => i);
            using var b = _cpu.CreateVector(5, i => i * 2);
            var cpu = a.DotProduct(b);
            var gpu = Apply(_cuda, a, b, (a, b) => a.DotProduct(b));
            var mkl = Apply(_mkl, a, b, (a, b) => a.DotProduct(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorL2Norm()
        {
            using var a = _cpu.CreateVector(5, i => i);
            var cpu = a.L2Norm();
            var gpu = Apply(_cuda, a, a => a.L2Norm());
            var mkl = Apply(_cuda, a, a => a.L2Norm());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorAddInPlace()
        {
            using var cpu = _cpu.CreateVector(5, i => i * 2);
            using var b = _cpu.CreateVector(5, i => i);

            using var gpu = Apply(_cuda, cpu, b, (a, b) => a.AddInPlace(b, 2, 3));
            using var mkl = Apply(_mkl, cpu, b, (a, b) => a.AddInPlace(b, 2, 3));
            cpu.AddInPlace(b, 2, 3);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorSubtractInPlace()
        {
            using var cpu = _cpu.CreateVector(5, i => i * 2);
            using var b = _cpu.CreateVector(5, i => i);

            var gpu = Apply(_cuda, cpu, b, (a, b) => a.SubtractInPlace(b, 2, 3));
            var mkl = Apply(_mkl, cpu, b, (a, b) => a.SubtractInPlace(b, 2, 3));
            cpu.SubtractInPlace(b, 2, 3);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorSqrt()
        {
            using var a = _cpu.CreateVector(10, i => i * 2);
            using var cpu = a.Sqrt();
            using var gpu = Apply(_cuda, a, a => a.Sqrt());
            using var mkl = Apply(_mkl, a, a => a.Sqrt());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorGetNewVectorFromIndices()
        {
            using var a = _cpu.CreateVector(10, i => i * 2);
            var array = new uint[] { 2, 3, 5 };
            using var cpu = a.CherryPick(array);
            using var gpu = Apply(_cuda, a, a => a.CherryPick(array));
            using var mkl = Apply(_mkl, a, a => a.CherryPick(array));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorCopyTo()
        {
            using var cpuA = _cpu.CreateVector(10, i => i * 2);
            using var cpuB = _cpu.CreateVector(10, 0);
            cpuA.CopyTo(cpuB);
            AssertSame(cpuA, cpuB);

            using var gpuA = _cuda.CreateVector(cpuA);
            using var gpuB = _cuda.CreateVector(10, 0);
            gpuA.CopyTo(gpuB);

            using var mklA = _mkl.CreateVector(cpuA);
            using var mklB = _mkl.CreateVector(10, 0);
            mklA.CopyTo(mklB);

            AssertSame(cpuB, gpuB, mklB);
        }

        [Fact]
        public void VectorClone()
        {
            using var a = _cpu.CreateVector(5, i => i);
            var cpu = a.Clone();
            Math<float>.AreApproximatelyEqual(a, cpu);

            var gpu = Apply(_cuda, a, a => a.Clone());
            var mkl = Apply(_mkl, a, a => a.Clone());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorMultiply()
        {
            using var a = _cpu.CreateVector(5, i => i);
            const float operand = 2f;

            using var gpu = Apply(_cuda, a, a => a.Multiply(operand));
            using var mkl = Apply(_mkl, a, a => a.Multiply(operand));
            using var cpu = a.Multiply(operand);
            AssertSame(cpu, gpu, mkl);
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
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(500, _ => distribution.Sample());
            using var b = _cpu.CreateVector(500, _ => distribution.Sample());
            var cpu = a.EuclideanDistance(b);

            var gpu = Apply(_cuda, a, b, (a, b) => a.EuclideanDistance(b));
            var mkl = Apply(_mkl, a, b, (a, b) => a.EuclideanDistance(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorCosineDistance()
        {
            var rand = new Random(0);

            using var a = _cpu.CreateVector(5000, _ => rand.NextSingle());
            using var b = _cpu.CreateVector(5000, _ => rand.NextSingle());
            var cpu = a.CosineDistance(b);
            var gpu = Apply(_cuda, a, b, (a, b) => a.CosineDistance(b));
            var mkl = Apply(_mkl, a, b, (a, b) => a.CosineDistance(b));
            AssertSameWithMaxDifference(10, cpu, gpu, mkl);
        }

        [Fact]
        public void VectorManhattanDistance()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var b = _cpu.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.ManhattanDistance(b);

            var gpu = Apply(_cuda, a, b, (a, b) => a.ManhattanDistance(b));
            var mkl = Apply(_mkl, a, b, (a, b) => a.ManhattanDistance(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorMeanSquaredDistance()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(1000, _ => distribution.Sample());
            using var b = _cpu.CreateVector(1000, _ => distribution.Sample());
            var cpu = a.MeanSquaredDistance(b);
            var gpu = Apply(_cuda, a, b, (a, b) => a.MeanSquaredDistance(b));
            var mkl = Apply(_mkl, a, b, (a, b) => a.MeanSquaredDistance(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorSquaredEuclideanDistance()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(1000, _ => distribution.Sample());
            using var b = _cpu.CreateVector(1000, _ => distribution.Sample());
            var cpu = a.SquaredEuclideanDistance(b);

            var gpu = Apply(_cuda, a, b, (a, b) => a.SquaredEuclideanDistance(b));
            var mkl = Apply(_cuda, a, b, (a, b) => a.SquaredEuclideanDistance(b));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorAverage()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.Average();
            var gpu = Apply(_cuda, a, a => a.Average());
            var mkl = Apply(_mkl, a, a => a.Average());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorL1Norm()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.L1Norm();
            var gpu = Apply(_cuda, a, a => a.L1Norm());
            var mkl = Apply(_mkl, a, a => a.L1Norm());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorAbs()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.Abs();
            using var gpu = Apply(_cuda, a, a => a.Abs());
            using var mkl = Apply(_mkl, a, a => a.Abs());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorLog()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.Log();

            var gpu = Apply(_cuda, a, a => a.Log());
            var mkl = Apply(_mkl, a, a => a.Log());
            AssertSameWithMaxDifference(14, cpu, gpu, mkl);
        }

        [Fact]
        public void VectorStdDev()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            var cpu = a.StdDev(null);
            var gpu = Apply(_cuda, a, a => a.StdDev(null));
            var mkl = Apply(_mkl, a, a => a.StdDev(null));
            AssertSame(cpu, gpu, mkl);
        }

        //void TestNormalise(NormalizationType type)
        //{
        //    var distribution = _context.CreateNormalDistribution(0, 5);

        //    using var a = _lap.CreateVector(5000, _ => distribution.Sample());
        //    using (var gpuA = _cuda.CreateVector(a)) {
        //        gpuA.(type);
        //        gpuResults = gpuA;
        //    }
        //    using (var simpleA = _mkl.CreateVector(a)) {
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

        static IVector<float> TestMultiDistance(LinearAlgebraProvider<float> lap, IVector<float> a, IVector<float> b, IVector<float> c, DistanceMetric distanceMetric)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            using var otherC = lap.CreateVector(c);
            return otherA.FindDistances([otherB, otherC], distanceMetric);
        }

        [Fact]
        public void MultiEuclideanDistance()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var b = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var c = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var cpu = TestMultiDistance(_cpu, a, b, c , DistanceMetric.Euclidean);
            using var gpu = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Euclidean);
            using var mkl = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Euclidean);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MultiManhattanDistance()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var b = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var c = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var cpu = TestMultiDistance(_cpu, a, b, c , DistanceMetric.Manhattan);
            using var gpu = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Manhattan);
            using var mkl = TestMultiDistance(_mkl, a, b, c, DistanceMetric.Manhattan);
            AssertSame(cpu, gpu, mkl);        }

        [Fact]
        public void MultiCosineDistance()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);

            using var a = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var b = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var c = _cpu.CreateVector(5000, _ => distribution.Sample());
            using var cpu = TestMultiDistance(_cpu, a, b, c, DistanceMetric.Cosine);
            using var gpu = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Cosine);
            using var mkl = TestMultiDistance(_mkl, a, b, c, DistanceMetric.Cosine);
            AssertSameWithMaxDifference(18, cpu, gpu, mkl);
        }

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

        //    using var gpuDistance;
        //    using (var gpuA = _cuda.CreateVector(a))
        //    using (var gpuB = _cuda.CreateVector(b))
        //    using (var gpuC = _cuda.CreateVector(c))
        //    using (var temp = gpuA.CosineDistance(new[] { gpuB, gpuC }, ref dataNorm2))
        //        gpuDistance = temp;
        //    FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

        //    using var simpleDistance;
        //    using (var simpleA = _mkl.CreateVector(a))
        //    using (var simpleB = _mkl.CreateVector(b))
        //    using (var simpleC = _mkl.CreateVector(c))
        //    using (var temp = simpleA.CosineDistance(new[] { simpleB, simpleC }, ref dataNorm3))
        //        simpleDistance = temp;
        //    FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        //}

        [Fact]
        public void VectorAddScalar()
        {
            using var vector = _cpu.CreateVector(1000, i => i);
            using var cpu = vector.Add(0.5f);
            using var gpu = Apply(_cuda, vector, a => a.Add(0.5f));
            using var mkl = Apply(_mkl, vector, a => a.Add(0.5f));
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorSigmoid()
        {
            using var a = _cpu.CreateVector(1000, i => i);
            using var cpu = a.Sigmoid();
            using var gpu = Apply(_cuda, a, a => a.Sigmoid());
            using var mkl = Apply(_mkl, a, a => a.Sigmoid());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void MatrixVectorMultiply()
        {
            using var a = _cpu.CreateMatrix(256, 256, (x, y) => x * y);
            using var b = _cpu.CreateVector(256, i => i * 0.5f);
            using var cpu = a.Multiply(b);

            using var gpuA = _cuda.CreateMatrix(a);
            using var gpuB = _cuda.CreateVector(b);
            using var gpu = gpuA.Multiply(gpuB);

            using var mklA = _mkl.CreateMatrix(a);
            using var mklB = _mkl.CreateVector(b);
            using var mkl = mklA.Multiply(mklB);
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorSplit()
        {
            const int blockCount = 3;
            using var a = _cpu.CreateVector(12, i => i);
            var cpu = a.Split(blockCount).Select(v => v).ToList();
            foreach (var item in cpu)
                item.Size.Should().Be(4);

            var gpu = new List<IVector<float>>();
            using (var gpuA = _cuda.CreateVector(a)) {
                gpu.AddRange(gpuA.Split(blockCount));
            }
            
            var mkl = new List<IVector<float>>();
            using (var mklA = _mkl.CreateVector(a)) {
                mkl.AddRange(mklA.Split(blockCount));
            }
            for (var i = 0; i < cpu.Count; i++)
                AssertSameAndThenDispose(cpu[i], gpu[i], mkl[i]);
        }

        [Fact]
        public void VectorSoftMax()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);
            using var a = _cpu.CreateVector(128, _ => distribution.Sample());
            using var cpu = a.Softmax();

            using var gpu = Apply(_cuda, a, a => a.Softmax());
            using var mkl = Apply(_mkl, a, a => a.Softmax());
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorSoftMaxDerivative()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);
            using var a = _cpu.CreateVector(128, _ => distribution.Sample());
            using var cpu = a.SoftmaxDerivative();

            using var gpuA = _cuda.CreateVector(a);
            using var gpu = gpuA.SoftmaxDerivative();

            using var mklA = _mkl.CreateVector(a);
            using var mkl = mklA.SoftmaxDerivative();
            AssertSame(cpu, gpu, mkl);
        }

        [Fact]
        public void VectorReverse()
        {
            var distribution = _context.CreateNormalDistribution<float>(0, 5);
            var a = _cpu.CreateVector(128, _ => distribution.Sample());
            var cpu = a.Reverse();

            using var gpu = Apply(_cuda, a, a => a.Reverse());
            using var mkl = Apply(_mkl, a, a => a.Reverse());
            AssertSame(cpu, gpu, mkl);
        }

        //[Fact]
        //public void VectorRotate()
        //{
        //    var cpu = _cpu.CreateVector(4, i => i + 1);
        //    using var gpuA = _cuda.CreateVector(cpu);
        //    using var simpleA = _mkl.CreateVector(cpu);
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
        //    using var simpleA = _mkl.CreateVector(cpu);
        //    cpu.RotateInPlace(BLOCK_COUNT);
        //    gpuA.RotateInPlace(BLOCK_COUNT);
        //    simpleA.RotateInPlace(BLOCK_COUNT);
        //    FloatMath.AreApproximatelyEqual(gpuA, cpu).Should().BeTrue();
        //    FloatMath.AreApproximatelyEqual(simpleA, cpu).Should().BeTrue();
        //}

        [Fact]
        public void TestFinite()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, -1f);
            vector.IsEntirelyFinite().Should().BeTrue();

            using var gpuVector = _cuda.CreateVector(vector);
            gpuVector.IsEntirelyFinite().Should().BeTrue();

            using var mklVector = _mkl.CreateVector(vector);
            mklVector.IsEntirelyFinite().Should().BeTrue();
        }

        [Fact]
        public void TestFinite2()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, -1f, float.Epsilon);
            vector.IsEntirelyFinite().Should().BeTrue();

            using var gpuVector = _cuda.CreateVector(vector);
            gpuVector.IsEntirelyFinite().Should().BeTrue();

            using var mklVector = _mkl.CreateVector(vector);
            mklVector.IsEntirelyFinite().Should().BeTrue();
        }

        [Fact]
        public void TestNotFinite()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.NaN);
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector);
            gpuVector.IsEntirelyFinite().Should().BeFalse();

            using var mklVector = _mkl.CreateVector(vector);
            mklVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestNotFinite2()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.NegativeInfinity);
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector);
            gpuVector.IsEntirelyFinite().Should().BeFalse();

            using var mklVector = _mkl.CreateVector(vector);
            mklVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestNotFinite3()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.PositiveInfinity);
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector);
            gpuVector.IsEntirelyFinite().Should().BeFalse();

            using var mklVector = _mkl.CreateVector(vector);
            mklVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestRoundInPlace()
        {
            using var cpu = _cpu.CreateVector(0.5f, 0.75f, 1f, 1.5f, 0.25f, 0.1f, 0f, -1f);
            using var gpu = _cuda.CreateVector(cpu);
            using var mkl = _mkl.CreateVector(cpu);

            cpu.RoundInPlace(0f, 1f);
            gpu.RoundInPlace(0f, 1f);
            mkl.RoundInPlace(0f, 1f);
            AssertSame(cpu, gpu, mkl);

            var data = cpu.Segment;
            data[0].Should().Be(1f);
            data[1].Should().Be(1f);
            data[2].Should().Be(1f);
            data[3].Should().Be(1f);
            data[4].Should().Be(0f);
            data[5].Should().Be(0f);
            data[6].Should().Be(0f);
            data[7].Should().Be(0f);
        }

        [Fact]
        public void TestConstrain()
        {
            using var cpu = _cpu.CreateVector(0f, 1f, -1f, float.NaN, float.NegativeInfinity, float.PositiveInfinity);
            using var gpu = _cuda.CreateVector(cpu);
            using var mkl = _mkl.CreateVector(cpu);

            cpu.ConstrainInPlace(null, null);
            gpu.ConstrainInPlace(null, null);
            mkl.ConstrainInPlace(null, null);
            AssertSame(cpu, gpu, mkl);

            cpu[0].Should().Be(0f);
            cpu[1].Should().Be(1f);
            cpu[2].Should().Be(-1f);
            cpu[3].Should().Be(0f);
            cpu[4].Should().Be(float.MinValue);
            cpu[5].Should().Be(float.MaxValue);
        }

        [Fact]
        public void TestConstrain2()
        {
            using var cpu = _cpu.CreateVector(0f, 2f, -2f, float.NaN, float.NegativeInfinity, float.PositiveInfinity);
            using var gpu = _cuda.CreateVector(cpu);
            using var mkl = _mkl.CreateVector(cpu);

            cpu.ConstrainInPlace(-1f, 1f);
            gpu.ConstrainInPlace(-1f, 1f);
            mkl.ConstrainInPlace(-1f, 1f);
            AssertSame(cpu, gpu, mkl);

            cpu[0].Should().Be(0f);
            cpu[1].Should().Be(1f);
            cpu[2].Should().Be(-1f);
            cpu[3].Should().Be(0f);
            cpu[4].Should().Be(-1f);
            cpu[5].Should().Be(1f);
        }
    }
}
