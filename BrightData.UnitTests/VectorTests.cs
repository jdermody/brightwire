using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Helper;
using Xunit;
using FluentAssertions;
using MathNet.Numerics.Distributions;

namespace BrightData.UnitTests
{
    public class VectorTests : CudaBase
    {
        static IIndexableFloatVector Apply(ILinearAlgebraProvider lap, IIndexableFloatVector a, IIndexableFloatVector b, Func<IFloatVector, IFloatVector, IFloatVector> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            using var otherC = func(otherA, otherB);
            return otherC.AsIndexable();
        }

        static IIndexableFloatVector Apply(ILinearAlgebraProvider lap, IIndexableFloatVector a, IIndexableFloatVector b, Action<IFloatVector, IFloatVector> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            func(otherA, otherB);
            return otherA.AsIndexable();
        }

        static IIndexableFloatVector Apply(ILinearAlgebraProvider lap, IIndexableFloatVector a, Action<IFloatVector> func)
        {
            using var otherA = lap.CreateVector(a);
            func(otherA);
            return otherA.AsIndexable();
        }

        static IIndexableFloatVector Apply(ILinearAlgebraProvider lap, IIndexableFloatVector a, Func<IFloatVector, IFloatVector> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = func(otherA);
            return otherB.AsIndexable();
        }

        static T Apply<T>(ILinearAlgebraProvider lap, IIndexableFloatVector a, Func<IFloatVector, T> func)
        {
            using var otherA = lap.CreateVector(a);
            return func(otherA);
        }

        static float Apply(ILinearAlgebraProvider lap, IIndexableFloatVector a, IIndexableFloatVector b, Func<IFloatVector, IFloatVector, float> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            return func(otherA, otherB);
        }

        static IIndexableFloatMatrix Apply(ILinearAlgebraProvider lap, IIndexableFloatVector a, Func<IFloatVector, IFloatMatrix> func)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = func(otherA);
            return otherB.AsIndexable();
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

            using var simpleA = _simple.CreateVector(values);
            FloatMath.AreApproximatelyEqual(simpleA.AsIndexable(), a).Should().BeTrue();
        }

        [Fact]
        public void TestManhattanDistance()
        {
            var distribution = new Normal(0, 5);
            var vectors = Enumerable.Range(0, 10).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample()))).ToArray();
            var compareTo = Enumerable.Range(0, 20).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample()))).ToArray();
            var distances = _cpu.CalculateDistances(vectors, compareTo, DistanceMetric.Manhattan);

            var gpuVectors = vectors.Select(v => _cuda.CreateVector(v)).ToArray();
            var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v)).ToArray();
            var gpuDistances = _cuda.CalculateDistances(gpuVectors, gpuCompareTo, DistanceMetric.Manhattan);
            FloatMath.AreApproximatelyEqual(distances.AsIndexable(), gpuDistances.AsIndexable()).Should().BeTrue();

            var simpleVectors = vectors.Select(v => _simple.CreateVector(v)).ToArray();
            var simpleCompareTo = compareTo.Select(v => _simple.CreateVector(v)).ToArray();
            var simpleDistances = _simple.CalculateDistances(simpleVectors, simpleCompareTo, DistanceMetric.Manhattan);
            FloatMath.AreApproximatelyEqual(distances.AsIndexable(), simpleDistances.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void TestCosineDistance()
        {
            var distribution = new Normal(0, 5);
            var vectors = Enumerable.Range(0, 10).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample()))).ToArray();
            var compareTo = Enumerable.Range(0, 20).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToArray();
            var distances = _cpu.CalculateDistances(vectors, compareTo, DistanceMetric.Cosine);

            var gpuVectors = vectors.Select(v => _cuda.CreateVector(v)).ToArray();
            var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v)).ToArray();
            var gpuDistances = _cuda.CalculateDistances(gpuVectors, gpuCompareTo, DistanceMetric.Cosine);
            FloatMath.AreApproximatelyEqual(distances.AsIndexable(), gpuDistances.AsIndexable()).Should().BeTrue();

            var simpleVectors = vectors.Select(v => _simple.CreateVector(v)).ToArray();
            var simpleCompareTo = compareTo.Select(v => _simple.CreateVector(v)).ToArray();
            var simpleDistances = _simple.CalculateDistances(simpleVectors, simpleCompareTo, DistanceMetric.Cosine);
            FloatMath.AreApproximatelyEqual(distances.AsIndexable(), simpleDistances.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void VectorColumnMatrix()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var matrix = a.ReshapeAsColumnMatrix().AsIndexable();

            var gpuResults = Apply(_cuda, a, a => a.ReshapeAsColumnMatrix());
            FloatMath.AreApproximatelyEqual(matrix, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, a => a.ReshapeAsColumnMatrix());
            FloatMath.AreApproximatelyEqual(matrix, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorRowMatrix()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var matrix = a.ReshapeAsRowMatrix().AsIndexable();

            var gpuResults = Apply(_cuda, a, a => a.ReshapeAsRowMatrix());
            FloatMath.AreApproximatelyEqual(matrix, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, a => a.ReshapeAsRowMatrix());
            FloatMath.AreApproximatelyEqual(matrix, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorAdd()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var c = a.Add(b).AsIndexable();

            var gpuResults = Apply(_cuda, a, b, (a, b) => a.Add(b));
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, b, (a, b) => a.Add(b));
            FloatMath.AreApproximatelyEqual(c, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSubtract()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var c = a.Subtract(b).AsIndexable();

            var gpuResults = Apply(_cuda, a, b, (a, b) => a.Subtract(b));
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, b, (a, b) => a.Subtract(b));;
            FloatMath.AreApproximatelyEqual(c, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorPointwiseMultiply()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var c = a.PointwiseMultiply(b).AsIndexable();

            var gpuResults = Apply(_cuda, a, b, (a, b) => a.PointwiseMultiply(b));
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, b, (a, b) => a.PointwiseMultiply(b));;
            FloatMath.AreApproximatelyEqual(c, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorDotProduct()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var cpu = a.DotProduct(b);

            var gpu = Apply(_cuda, a, b, (a, b) => a.DotProduct(b));
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();

            var simple = Apply(_simple, a, b, (a, b) => a.DotProduct(b));
            FloatMath.AreApproximatelyEqual(cpu, simple).Should().BeTrue();
        }

        [Fact]
        public void VectorL2Norm()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var cpu = a.L2Norm();

            var gpu = Apply(_cuda, a, a => a.L2Norm());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();

            var simple = Apply(_simple, a, a => a.L2Norm());
            FloatMath.AreApproximatelyEqual(cpu, simple).Should().BeTrue();
        }

        [Fact]
        public void VectorMaximumIndex()
        {
            var a = _cpu.CreateVector(1.0f, 2.0f, 1.0f, 1.0f).AsIndexable();
            var cpu = a.MaximumAbsoluteIndex();

            var gpu = Apply(_cuda, a, a => a.MaximumAbsoluteIndex());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();

            var simple = Apply(_simple, a, a => a.MaximumAbsoluteIndex());
            FloatMath.AreApproximatelyEqual(cpu, simple).Should().BeTrue();
        }

        [Fact]
        public void VectorMinimumIndex()
        {
            var a = _cpu.CreateVector(3.0f, -2.0f, 1.0f, 2.0f).AsIndexable();
            var cpu = a.MinimumAbsoluteIndex();

            var gpu = Apply(_cuda, a, a => a.MinimumAbsoluteIndex());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();

            var simple = Apply(_simple, a, a => a.MinimumAbsoluteIndex());
            FloatMath.AreApproximatelyEqual(cpu, simple).Should().BeTrue();
        }

        [Fact]
        public void VectorAddInPlace()
        {
            var a = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var b = _cpu.CreateVector(5, i => i).AsIndexable();

            var gpuResults = Apply(_cuda, a, b, (a, b) => a.AddInPlace(b, 2.5f, 3.5f));
            var simpleResults = Apply(_simple, a, b, (a, b) => a.AddInPlace(b, 2.5f, 3.5f));;

            a.AddInPlace(b, 2.5f, 3.5f);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(a, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSubtractInPlace()
        {
            var a = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var b = _cpu.CreateVector(5, i => i).AsIndexable();

            var gpuResults = Apply(_cuda, a, b, (a, b) => a.SubtractInPlace(b, 2.5f, 3.5f));
            var simpleResults = Apply(_simple, a, b, (a, b) => a.SubtractInPlace(b, 2.5f, 3.5f));;

            a.SubtractInPlace(b, 2.5f, 3.5f);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(a, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSqrt()
        {
            var a = _cpu.CreateVector(10, i => i * 2).AsIndexable();
            var cpuResults = a.Sqrt().AsIndexable();

            var gpuResults = Apply(_cuda, a, a => a.Sqrt());
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, a => a.Sqrt());
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults, 30).Should().BeTrue();
        }

        [Fact]
        public void VectorGetNewVectorFromIndices()
        {
            var a = _cpu.CreateVector(10, i => i * 2).AsIndexable();
            var array = new uint[] { 2, 3, 5 };
            var cpuResults = a.GetNewVectorFromIndexes(array).AsIndexable();

            var gpuResults = Apply(_cuda, a, a => a.GetNewVectorFromIndexes(array));
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, a => a.GetNewVectorFromIndexes(array));;
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorCopyFrom()
        {
            var cpuResults = _cpu.CreateVector(10, i => i * 2).AsIndexable();
            var b = _cpu.CreateVector(10, 0).AsIndexable();
            b.CopyFrom(cpuResults);
            FloatMath.AreApproximatelyEqual(cpuResults, b);

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(cpuResults))
            using (var gpuB = _cuda.CreateVector(10, 0)) {
                gpuB.CopyFrom(gpuA);
                gpuResults = gpuB.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

            IIndexableFloatVector simpleResults;
            using (var simpleA = _simple.CreateVector(cpuResults))
            using (var simpleB = _simple.CreateVector(10, 0)) {
                simpleB.CopyFrom(simpleA);
                simpleResults = simpleB.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorClone()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var cpuResults = a.Clone().AsIndexable();
            FloatMath.AreApproximatelyEqual(a, cpuResults);

            var gpuResults = Apply(_cuda, a, a => a.Clone());
            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, a => a.Clone());
            FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorMultiply()
        {
            var cpuResults = _cpu.CreateVector(5, i => i).AsIndexable();
            const float OPERAND = 2f;

            var gpuResults = Apply(_cuda, cpuResults, a => a.Multiply(OPERAND));
            var simpleResults = Apply(_simple, cpuResults, a => a.Multiply(OPERAND));;

            cpuResults.Multiply(OPERAND);
            FloatMath.AreApproximatelyEqual(gpuResults, cpuResults).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(simpleResults, cpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorReadWrite()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();

            // test Numerics -> Numerics serialisation
            var serialised = a.Data;
            var b = _cpu.CreateVector(serialised);
            FloatMath.AreApproximatelyEqual(a.AsIndexable(), b.AsIndexable()).Should().BeTrue();

            // test Numerics -> Cuda serialisation
            using var c = _cuda.CreateVector(serialised);
            FloatMath.AreApproximatelyEqual(a.AsIndexable(), c.AsIndexable()).Should().BeTrue();

            // test Cuda -> Cuda serialisation
            var serialised2 = c.Data;
            using (var d = _cuda.CreateVector(serialised2))
                FloatMath.AreApproximatelyEqual(a.AsIndexable(), d.AsIndexable()).Should().BeTrue();

            // test Cuda -> Numerics serialisation
            var e = _cpu.CreateVector(c.Data);
            FloatMath.AreApproximatelyEqual(a.AsIndexable(), e.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void VectorEuclideanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpuDistance = a.EuclideanDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.EuclideanDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();

            var simpleDistance = Apply(_simple, a, b, (a, b) => a.EuclideanDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorCosineDistance()
        {
            var rand = new Random(0);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var cpuDistance = a.CosineDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.CosineDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();

            var simpleDistance = Apply(_simple, a, b, (a, b) => a.CosineDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorManhattanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpuDistance = a.ManhattanDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.ManhattanDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();

            var simpleDistance = Apply(_simple, a, b, (a, b) => a.ManhattanDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorMeanSquaredDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(1000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(1000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpuDistance = a.MeanSquaredDistance(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.MeanSquaredDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();

            var simpleDistance = Apply(_simple, a, b, (a, b) => a.MeanSquaredDistance(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorSquaredEuclideanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(1000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(1000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpuDistance = a.SquaredEuclidean(b);

            var gpuDistance = Apply(_cuda, a, b, (a, b) => a.SquaredEuclidean(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, gpuDistance, 10).Should().BeTrue();

            var simpleDistance = Apply(_simple, a, b, (a, b) => a.SquaredEuclidean(b));
            FloatMath.AreApproximatelyEqual(cpuDistance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorMinMax()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var (min, max) = a.GetMinMax();

            var gpuMinMax = Apply(_cuda, a, a => a.GetMinMax());
            FloatMath.AreApproximatelyEqual(min, gpuMinMax.Min).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(max, gpuMinMax.Max).Should().BeTrue();

            var simpleMinMax = Apply(_simple, a, a => a.GetMinMax());
            FloatMath.AreApproximatelyEqual(min, simpleMinMax.Min).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(max, simpleMinMax.Max).Should().BeTrue();
        }

        [Fact]
        public void VectorAverage()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpuAverage = a.Average();

            var gpuAverage = Apply(_cuda, a, a => a.Average());
            FloatMath.AreApproximatelyEqual(cpuAverage, gpuAverage, 7).Should().BeTrue();

            var simpleAverage = Apply(_simple, a, a => a.Average());
            FloatMath.AreApproximatelyEqual(cpuAverage, simpleAverage, 7).Should().BeTrue();
        }

        [Fact]
        public void VectorL1Norm()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpu = a.L1Norm();

            var gpu = Apply(_cuda, a, a => a.L1Norm());
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();

            var simple = Apply(_simple, a, a => a.L1Norm());;
            FloatMath.AreApproximatelyEqual(cpu, simple).Should().BeTrue();
        }

        [Fact]
        public void VectorAbs()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpuResults = a.Abs().AsIndexable();

            var gpuResults = Apply(_cuda, a, a => a.Abs());
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

            var simpleResults = Apply(_simple, a, a => a.Abs());
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorLog()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpuResults = a.Log().AsIndexable();

            var gpuResults = Apply(_cuda, a, a => a.Log());
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults, 14).Should().BeTrue();

            var simpleResults = Apply(_simple, a, a => a.Log());
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults, 14).Should().BeTrue();
        }

        [Fact]
        public void VectorStdDev()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var cpu = a.StdDev(null);

            float gpu = Apply(_cuda, a, a => a.StdDev(null));
            FloatMath.AreApproximatelyEqual(cpu, gpu).Should().BeTrue();

            float simple = Apply(_simple, a, a => a.StdDev(null));
            FloatMath.AreApproximatelyEqual(cpu, simple).Should().BeTrue();
        }

        void TestNormalise(NormalizationType type)
        {
            var distribution = new Normal(0, 5);

            IIndexableFloatVector gpuResults, simpleResults;
            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            using (var gpuA = _cuda.CreateVector(a.AsIndexable())) {
                gpuA.Normalize(type);
                gpuResults = gpuA.AsIndexable();
            }
            using (var simpleA = _simple.CreateVector(a.AsIndexable())) {
                simpleA.Normalize(type);
                simpleResults = simpleA.AsIndexable();
            }
            a.Normalize(type);
            var cpuResults = a.AsIndexable();
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults, 12).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults, 12).Should().BeTrue();
        }

        [Fact]
        public void VectorFeatureScaleNormalise()
        {
            TestNormalise(NormalizationType.FeatureScale);
        }

        [Fact]
        public void VectorStandardNormalise()
        {
            TestNormalise(NormalizationType.Standard);
        }

        [Fact]
        public void VectorManhattanNormalise()
        {
            TestNormalise(NormalizationType.Manhattan);
        }

        [Fact]
        public void VectorEuclideanNormalise()
        {
            TestNormalise(NormalizationType.Euclidean);
        }

        static IIndexableFloatVector TestMultiDistance(ILinearAlgebraProvider lap, IIndexableFloatVector a, IFloatVector b, IFloatVector c, DistanceMetric distanceMetric)
        {
            using var otherA = lap.CreateVector(a);
            using var otherB = lap.CreateVector(b);
            using var otherC = lap.CreateVector(c);
            using var temp = otherA.FindDistances(new[] { otherB, otherC }, distanceMetric);
            return temp.AsIndexable();
        }

        [Fact]
        public void MultiEuclideanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Euclidean).AsIndexable();

            IIndexableFloatVector gpuDistance = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Euclidean);
            FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

            IIndexableFloatVector simpleDistance = TestMultiDistance(_simple, a, b, c, DistanceMetric.Euclidean);;
            FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void MultiManhattanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Manhattan).AsIndexable();

            IIndexableFloatVector gpuDistance = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Manhattan);
            FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

            IIndexableFloatVector simpleDistance = TestMultiDistance(_simple, a, b, c, DistanceMetric.Manhattan);;
            FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void MultiCosineDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Cosine).AsIndexable();

            IIndexableFloatVector gpuDistance = TestMultiDistance(_cuda, a, b, c, DistanceMetric.Cosine);
            FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

            IIndexableFloatVector simpleDistance = TestMultiDistance(_simple, a, b, c, DistanceMetric.Cosine);;
            FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void MultiCosineDistance2()
        {
            var distribution = new Normal(0, 5);
            float[]? dataNorm1 = null;
            float[]? dataNorm2 = null;
            float[]? dataNorm3 = null;

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            var distance = a.CosineDistance(new[] { b, c }, ref dataNorm1).AsIndexable();

            IIndexableFloatVector gpuDistance;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = _cuda.CreateVector(c))
            using (var temp = gpuA.CosineDistance(new[] { gpuB, gpuC }, ref dataNorm2))
                gpuDistance = temp.AsIndexable();
            FloatMath.AreApproximatelyEqual(distance, gpuDistance, 10).Should().BeTrue();

            IIndexableFloatVector simpleDistance;
            using (var simpleA = _simple.CreateVector(a))
            using (var simpleB = _simple.CreateVector(b))
            using (var simpleC = _simple.CreateVector(c))
            using (var temp = simpleA.CosineDistance(new[] { simpleB, simpleC }, ref dataNorm3))
                simpleDistance = temp.AsIndexable();
            FloatMath.AreApproximatelyEqual(distance, simpleDistance, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorAddScalar()
        {
            var a = _cpu.CreateVector(1000, i => i).AsIndexable();

            IIndexableFloatVector gpuResults = Apply(_cuda, a, a => a.Add(0.5f));
            IIndexableFloatVector simpleResults = Apply(_simple, a, a => a.Add(0.5f));

            a.Add(0.5f);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(a, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSigmoid()
        {
            var a = _cpu.CreateVector(1000, i => i).AsIndexable();
            var cpuResults = a.Sigmoid().AsIndexable();

            IIndexableFloatVector gpuResults = Apply(_cuda, a, a => a.Sigmoid());
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

            IIndexableFloatVector simpleResults = Apply(_cuda, a, a => a.Sigmoid());
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixVectorMultiply()
        {
            var a = _cpu.CreateMatrix(256, 256, (x, y) => x * y).AsIndexable();
            var b = _cpu.CreateVector(256, i => i * 0.5f).AsIndexable();
            var cpuResults = a.Multiply(b).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = gpuA.Multiply(gpuB)) {
                gpuResults = gpuC.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(cpuResults, gpuResults).Should().BeTrue();

            IIndexableFloatMatrix simpleResults;
            using (var simpleA = _simple.CreateMatrix(a))
            using (var simpleB = _simple.CreateVector(b))
            using (var simpleC = simpleA.Multiply(simpleB)) {
                simpleResults = simpleC.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(cpuResults, simpleResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSplit()
        {
            const int BLOCK_COUNT = 3;
            var a = _cpu.CreateVector(12, i => i).AsIndexable();
            var cpuResult = a.Split(BLOCK_COUNT).Select(v => v.AsIndexable()).ToList();

            var gpuResult = new List<IIndexableFloatVector>();
            using (var gpuA = _cuda.CreateVector(a)) {
                var split = gpuA.Split(BLOCK_COUNT);
                foreach (var item in split) {
                    gpuResult.Add(item.AsIndexable());
                    item.Dispose();
                }
            }
            for (var i = 0; i < cpuResult.Count; i++) {
                cpuResult[i].Count.Should().Be(4);
                FloatMath.AreApproximatelyEqual(cpuResult[i], gpuResult[i]).Should().BeTrue();
            }

            var simpleResult = new List<IIndexableFloatVector>();
            using (var simpleA = _simple.CreateVector(a)) {
                var split = simpleA.Split(BLOCK_COUNT);
                foreach (var item in split) {
                    simpleResult.Add(item.AsIndexable());
                    item.Dispose();
                }
            }
            for (var i = 0; i < cpuResult.Count; i++) {
                FloatMath.AreApproximatelyEqual(cpuResult[i], simpleResult[i]).Should().BeTrue();
            }
        }

        [Fact]
        public void VectorSoftMax()
        {
            var distribution = new Normal(0, 5);
            var a = _cpu.CreateVector(128, i => (float)distribution.Sample()).AsIndexable();
            var cpuResult = a.Softmax().AsIndexable();

            IIndexableFloatVector gpuResult = Apply(_cuda, a, a => a.Softmax());
            FloatMath.AreApproximatelyEqual(gpuResult, cpuResult).Should().BeTrue();

            IIndexableFloatVector simpleResult = Apply(_simple, a, a => a.Softmax());
            FloatMath.AreApproximatelyEqual(simpleResult, cpuResult).Should().BeTrue();
        }

        [Fact]
        public void VectorSoftMaxDerivative()
        {
            var distribution = new Normal(0, 5);
            var a = _cpu.CreateVector(128, i => (float)distribution.Sample()).AsIndexable();
            var cpuResult = a.SoftmaxDerivative().AsIndexable();

            IIndexableFloatMatrix gpuResult;
            using (var gpuA = _cuda.CreateVector(a)) {
                using var softmaxDerivative = gpuA.SoftmaxDerivative();
                gpuResult = softmaxDerivative.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(gpuResult, cpuResult).Should().BeTrue();

            IIndexableFloatMatrix simpleResult;
            using (var simpleA = _simple.CreateVector(a)) {
                using var softmaxDerivative = simpleA.SoftmaxDerivative();
                simpleResult = softmaxDerivative.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(simpleResult, cpuResult).Should().BeTrue();
        }

        [Fact]
        public void VectorReverse()
        {
            var distribution = new Normal(0, 5);
            var a = _cpu.CreateVector(128, i => (float)distribution.Sample()).AsIndexable();
            var cpuResult = a.Reverse().AsIndexable();

            IIndexableFloatVector gpuResult = Apply(_cuda, a, a => a.Reverse());
            FloatMath.AreApproximatelyEqual(gpuResult, cpuResult).Should().BeTrue();

            IIndexableFloatVector simpleResult = Apply(_simple, a, a => a.Reverse());
            FloatMath.AreApproximatelyEqual(simpleResult, cpuResult).Should().BeTrue();
        }

        [Fact]
        public void VectorRotate()
        {
            var cpu = _cpu.CreateVector(4, i => i + 1).AsIndexable();
            using var gpuA = _cuda.CreateVector(cpu);
            using var simpleA = _simple.CreateVector(cpu);
            cpu.RotateInPlace();
            gpuA.RotateInPlace();
            simpleA.RotateInPlace();
            FloatMath.AreApproximatelyEqual(gpuA.AsIndexable(), cpu.AsIndexable()).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(simpleA.AsIndexable(), cpu.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void VectorRotate2()
        {
            const int BLOCK_COUNT = 2;
            var cpu = _cpu.CreateVector(8, i => i + 1).AsIndexable();
            using var gpuA = _cuda.CreateVector(cpu);
            using var simpleA = _simple.CreateVector(cpu);
            cpu.RotateInPlace(BLOCK_COUNT);
            gpuA.RotateInPlace(BLOCK_COUNT);
            simpleA.RotateInPlace(BLOCK_COUNT);
            FloatMath.AreApproximatelyEqual(gpuA.AsIndexable(), cpu.AsIndexable()).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(simpleA.AsIndexable(), cpu.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void TestFinite()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, -1f);
            vector.IsEntirelyFinite().Should().BeTrue();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeTrue();

            using var simpleVector = _simple.CreateVector(vector.AsIndexable());
            simpleVector.IsEntirelyFinite().Should().BeTrue();
        }

        [Fact]
        public void TestFinite2()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, -1f, float.Epsilon);
            vector.IsEntirelyFinite().Should().BeTrue();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeTrue();

            using var simpleVector = _simple.CreateVector(vector.AsIndexable());
            simpleVector.IsEntirelyFinite().Should().BeTrue();
        }

        [Fact]
        public void TestNotFinite()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.NaN);
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeFalse();

            using var simpleVector = _simple.CreateVector(vector.AsIndexable());
            simpleVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestNotFinite2()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.NegativeInfinity);
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeFalse();

            using var simpleVector = _simple.CreateVector(vector.AsIndexable());
            simpleVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestNotFinite3()
        {
            var vector = _cpu.CreateVector(0f, 1f, 2f, 3f, float.PositiveInfinity);
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeFalse();

            using var simpleVector = _simple.CreateVector(vector.AsIndexable());
            simpleVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestRoundInPlace()
        {
            using var vector = _cpu.CreateVector(0.5f, 0.75f, 1f, 1.5f, 0.25f, 0.1f, 0f, -1f);
            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            using var simpleVector = _simple.CreateVector(vector.AsIndexable());

            vector.RoundInPlace();
            gpuVector.RoundInPlace();
            simpleVector.RoundInPlace();
            FloatMath.AreApproximatelyEqual(gpuVector.AsIndexable(), vector.AsIndexable()).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(simpleVector.AsIndexable(), vector.AsIndexable()).Should().BeTrue();

            var data = vector.Data;
            data[0].Should().Be(1f);
            data[1].Should().Be(1f);
            data[2].Should().Be(1f);
            data[3].Should().Be(1f);
            data[4].Should().Be(0f);
            data[5].Should().Be(0f);
            data[6].Should().Be(0f);
            data[7].Should().Be(0f);
        }
    }
}
