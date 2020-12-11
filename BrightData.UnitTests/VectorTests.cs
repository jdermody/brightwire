using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.Cuda;
using BrightData.FloatTensors;
using BrightData.Helper;
using BrightData.Numerics;
using Xunit;
using FluentAssertions;
using MathNet.Numerics.Distributions;

namespace BrightData.UnitTests
{
    public class VectorTests : CudaBase
    {
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

        [Fact]
        public void TestManhattanDistance()
        {
            var distribution = new Normal(0, 5);
            var vectors = Enumerable.Range(0, 10).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToArray();
            var compareTo = Enumerable.Range(0, 20).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToArray();
            var distances = _cpu.CalculateDistances(vectors, compareTo, DistanceMetric.Manhattan);

            var gpuVectors = vectors.Select(v => _cuda.CreateVector(v)).ToArray();
            var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v)).ToArray();
            var gpuDistances = _cuda.CalculateDistances(gpuVectors, gpuCompareTo, DistanceMetric.Manhattan);

            FloatMath.AreApproximatelyEqual(distances.AsIndexable(), gpuDistances.AsIndexable()).Should().BeTrue();
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
        }

        [Fact]
        public void VectorColumnMatrix()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var matrix = a.ReshapeAsColumnMatrix().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = gpuA.ReshapeAsColumnMatrix())
                gpuResults = gpuB.AsIndexable();

            FloatMath.AreApproximatelyEqual(matrix, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorRowMatrix()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var matrix = a.ReshapeAsRowMatrix().AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var m = gpuA.ReshapeAsRowMatrix()) {
                gpuResults = m.AsIndexable();
            }

            FloatMath.AreApproximatelyEqual(matrix, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorAdd()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var c = a.Add(b).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = gpuA.Add(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSubtract()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var c = a.Subtract(b).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = gpuA.Subtract(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorPointwiseMultiply()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var c = a.PointwiseMultiply(b).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = gpuA.PointwiseMultiply(gpuB))
                gpuResults = gpuC.AsIndexable();

            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorDotProduct()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var dot1 = a.DotProduct(b);

            float dot2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
                dot2 = gpuA.DotProduct(gpuB);

            FloatMath.AreApproximatelyEqual(dot1, dot2).Should().BeTrue();
        }

        [Fact]
        public void VectorL2Norm()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var res1 = a.L2Norm();

            float res2;
            using (var gpuA = _cuda.CreateVector(a))
                res2 = gpuA.L2Norm();
            FloatMath.AreApproximatelyEqual(res1, res2).Should().BeTrue();
        }

        [Fact]
        public void VectorMaximumIndex()
        {
            var a = _cpu.CreateVector(new[] { 1.0f, 2.0f, 1.0f, 1.0f }).AsIndexable();
            var res1 = a.MaximumIndex();

            uint res2;
            using (var gpuA = _cuda.CreateVector(a))
                res2 = gpuA.MaximumIndex();
            FloatMath.AreApproximatelyEqual(res1, res2).Should().BeTrue();
        }

        [Fact]
        public void VectorMinimumIndex()
        {
            var a = _cpu.CreateVector(new[] { 3.0f, -2.0f, 1.0f, 2.0f }).AsIndexable();
            var res1 = a.MinimumIndex();

            uint res2;
            using (var gpuA = _cuda.CreateVector(a))
                res2 = gpuA.MinimumIndex();
            FloatMath.AreApproximatelyEqual(res1, res2).Should().BeTrue();
        }

        [Fact]
        public void VectorAddInPlace()
        {
            var a = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var b = _cpu.CreateVector(5, i => i).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b)) {
                gpuA.AddInPlace(gpuB, 2.5f, 3.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.AddInPlace(b, 2.5f, 3.5f);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSubtractInPlace()
        {
            var a = _cpu.CreateVector(5, i => i * 2).AsIndexable();
            var b = _cpu.CreateVector(5, i => i).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b)) {
                gpuA.SubtractInPlace(gpuB, 2.5f, 3.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.SubtractInPlace(b, 2.5f, 3.5f);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSqrt()
        {
            var a = _cpu.CreateVector(10, i => i * 2).AsIndexable();
            var b = a.Sqrt().AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = gpuA.Sqrt()) {
                gpuResults = gpuB.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(b, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorGetNewVectorFromIndices()
        {
            var a = _cpu.CreateVector(10, i => i * 2).AsIndexable();
            var array = new uint[] { 2, 3, 5 };
            var b = a.GetNewVectorFromIndexes(array).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = gpuA.GetNewVectorFromIndexes(array)) {
                gpuResults = gpuB.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(b, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorCopyFrom()
        {
            var a = _cpu.CreateVector(10, i => i * 2).AsIndexable();
            var b = _cpu.CreateVector(10, 0).AsIndexable();
            b.CopyFrom(a);
            FloatMath.AreApproximatelyEqual(a, b);

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(10, 0)) {
                gpuB.CopyFrom(gpuA);
                gpuResults = gpuB.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorClone()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            var b = a.Clone().AsIndexable();
            FloatMath.AreApproximatelyEqual(a, b);

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var clone = gpuA.Clone()) {
                gpuResults = clone.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(gpuResults, b).Should().BeTrue();
        }

        [Fact]
        public void VectorMultiply()
        {
            var a = _cpu.CreateVector(5, i => i).AsIndexable();
            const float OPERAND = 2f;

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a)) {
                gpuA.Multiply(OPERAND);
                gpuResults = gpuA.AsIndexable();
            }

            a.Multiply(OPERAND);
            FloatMath.AreApproximatelyEqual(gpuResults, a).Should().BeTrue();
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
            var distance = a.EuclideanDistance(b);

            float distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
                distance2 = gpuA.EuclideanDistance(gpuB);

            FloatMath.AreApproximatelyEqual(distance, distance2, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorCosineDistance()
        {
            var rand = new Random(0);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(rand.NextDouble())).AsIndexable();
            var distance = a.CosineDistance(b);

            float distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
                distance2 = gpuA.CosineDistance(gpuB);

            FloatMath.AreApproximatelyEqual(distance, distance2, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorManhattanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.ManhattanDistance(b);

            float distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
                distance2 = gpuA.ManhattanDistance(gpuB);

            FloatMath.AreApproximatelyEqual(distance, distance2, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorMeanSquaredDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.MeanSquaredDistance(b);

            float distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
                distance2 = gpuA.MeanSquaredDistance(gpuB);

            FloatMath.AreApproximatelyEqual(distance, distance2, 12).Should().BeTrue();
        }

        [Fact]
        public void VectorSquaredEuclideanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.SquaredEuclidean(b);

            float distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
                distance2 = gpuA.SquaredEuclidean(gpuB);

            FloatMath.AreApproximatelyEqual(distance, distance2, 12).Should().BeTrue();
        }

        [Fact]
        public void VectorMinMax()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var minMax = a.GetMinMax();

            (float Min, float Max) minMax2;
            using (var gpuA = _cuda.CreateVector(a))
                minMax2 = gpuA.GetMinMax();

            FloatMath.AreApproximatelyEqual(minMax.Min, minMax2.Min).Should().BeTrue();
            FloatMath.AreApproximatelyEqual(minMax.Max, minMax2.Max).Should().BeTrue();
        }

        [Fact]
        public void VectorAverage()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var average = a.Average();

            float average2;
            using (var gpuA = _cuda.CreateVector(a))
                average2 = gpuA.Average();

            FloatMath.AreApproximatelyEqual(average, average2, 7).Should().BeTrue();
        }

        [Fact]
        public void VectorL1Norm()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var v1 = a.L1Norm();

            float v2;
            using (var gpuA = _cuda.CreateVector(a))
                v2 = gpuA.L1Norm();

            FloatMath.AreApproximatelyEqual(v1, v2).Should().BeTrue();
        }

        [Fact]
        public void VectorAbs()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var v1 = a.Abs().AsIndexable();

            IIndexableFloatVector v2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = gpuA.Abs())
                v2 = gpuB.AsIndexable();

            FloatMath.AreApproximatelyEqual(v1, v2).Should().BeTrue();
        }

        [Fact]
        public void VectorLog()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var v1 = a.Log().AsIndexable();

            IIndexableFloatVector v2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = gpuA.Log())
                v2 = gpuB.AsIndexable();

            FloatMath.AreApproximatelyEqual(v1, v2, 14).Should().BeTrue();
        }

        [Fact]
        public void VectorStdDev()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var stdDev = a.StdDev(null);

            float stdDev2;
            using (var gpuA = _cuda.CreateVector(a))
                stdDev2 = gpuA.StdDev(null);

            FloatMath.AreApproximatelyEqual(stdDev, stdDev2).Should().BeTrue();
        }

        void _TestNormalise(NormalizationType type)
        {
            var distribution = new Normal(0, 5);

            IIndexableFloatVector v2;
            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample()));
            using (var gpuA = _cuda.CreateVector(a.AsIndexable())) {
                gpuA.Normalise(type);
                v2 = gpuA.AsIndexable();
            }
            a.Normalise(type);
            var v1 = a.AsIndexable();
            FloatMath.AreApproximatelyEqual(v1, v2, 12).Should().BeTrue();
        }

        [Fact]
        public void VectorFeatureScaleNormalise()
        {
            _TestNormalise(NormalizationType.FeatureScale);
        }

        [Fact]
        public void VectorStandardNormalise()
        {
            _TestNormalise(NormalizationType.Standard);
        }

        [Fact]
        public void VectorManhattanNormalise()
        {
            _TestNormalise(NormalizationType.Manhattan);
        }

        [Fact]
        public void VectorEuclideanNormalise()
        {
            _TestNormalise(NormalizationType.Euclidean);
        }

        [Fact]
        public void MultiEuclideanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Euclidean).AsIndexable();

            IIndexableFloatVector distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = _cuda.CreateVector(c))
            using (var temp = gpuA.FindDistances(new[] { gpuB, gpuC }, DistanceMetric.Euclidean))
                distance2 = temp.AsIndexable();

            FloatMath.AreApproximatelyEqual(distance, distance2, 10).Should().BeTrue();
        }

        [Fact]
        public void MultiManhattanDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Manhattan).AsIndexable();

            IIndexableFloatVector distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = _cuda.CreateVector(c))
            using (var temp = gpuA.FindDistances(new[] { gpuB, gpuC }, DistanceMetric.Manhattan))
                distance2 = temp.AsIndexable();

            FloatMath.AreApproximatelyEqual(distance, distance2, 18).Should().BeTrue();
        }

        [Fact]
        public void MultiCosineDistance()
        {
            var distribution = new Normal(0, 5);

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.FindDistances(new[] { b, c }, DistanceMetric.Cosine).AsIndexable();

            IIndexableFloatVector distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = _cuda.CreateVector(c))
            using (var temp = gpuA.FindDistances(new[] { gpuB, gpuC }, DistanceMetric.Cosine))
                distance2 = temp.AsIndexable();

            FloatMath.AreApproximatelyEqual(distance, distance2, 10).Should().BeTrue();
        }

        [Fact]
        public void MultiCosineDistance2()
        {
            var distribution = new Normal(0, 5);
            float[] dataNorm1 = null;
            float[] dataNorm2 = null;

            var a = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var b = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var c = _cpu.CreateVector(5000, i => Convert.ToSingle(distribution.Sample())).AsIndexable();
            var distance = a.CosineDistance(new[] { b, c }, ref dataNorm1).AsIndexable();

            IIndexableFloatVector distance2;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = _cuda.CreateVector(c))
            using (var temp = gpuA.CosineDistance(new[] { gpuB, gpuC }, ref dataNorm2))
                distance2 = temp.AsIndexable();

            FloatMath.AreApproximatelyEqual(distance, distance2, 10).Should().BeTrue();
        }

        [Fact]
        public void VectorAddScalar()
        {
            var a = _cpu.CreateVector(1000, i => i).AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a)) {
                gpuA.Add(0.5f);
                gpuResults = gpuA.AsIndexable();
            }

            a.Add(0.5f);
            FloatMath.AreApproximatelyEqual(a, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void VectorSigmoid()
        {
            var a = _cpu.CreateVector(1000, i => i).AsIndexable();
            var results = a.Sigmoid().AsIndexable();

            IIndexableFloatVector gpuResults;
            using (var gpuA = _cuda.CreateVector(a))
            using (var gpuB = gpuA.Sigmoid()) {
                gpuResults = gpuB.AsIndexable();
            }

            FloatMath.AreApproximatelyEqual(results, gpuResults).Should().BeTrue();
        }

        [Fact]
        public void MatrixVectorMultiply()
        {
            var a = _cpu.CreateMatrix(256, 256, (x, y) => x * y).AsIndexable();
            var b = _cpu.CreateVector(256, i => i * 0.5f).AsIndexable();
            var c = a.Multiply(b).AsIndexable();

            IIndexableFloatMatrix gpuResults;
            using (var gpuA = _cuda.CreateMatrix(a))
            using (var gpuB = _cuda.CreateVector(b))
            using (var gpuC = gpuA.Multiply(gpuB)) {
                gpuResults = gpuC.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(c, gpuResults).Should().BeTrue();
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
        }

        [Fact]
        public void VectorSoftMax()
        {
            var distribution = new Normal(0, 5);
            var a = _cpu.CreateVector(128, i => (float)distribution.Sample()).AsIndexable();
            var cpuResult = a.Softmax().AsIndexable();
            IIndexableFloatVector result;
            using (var gpuA = _cuda.CreateVector(a)) {
                var gpuResult = gpuA.Softmax();
                result = gpuResult.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(result, cpuResult).Should().BeTrue();
        }

        [Fact]
        public void VectorSoftMaxDerivative()
        {
            var distribution = new Normal(0, 5);
            var a = _cpu.CreateVector(128, i => (float)distribution.Sample()).AsIndexable();
            var cpuResult = a.SoftmaxDerivative().AsIndexable();
            IIndexableFloatMatrix result;
            using (var gpuA = _cuda.CreateVector(a)) {
                var gpuResult = gpuA.SoftmaxDerivative();
                result = gpuResult.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(result, cpuResult).Should().BeTrue();
        }

        [Fact]
        public void VectorReverse()
        {
            var distribution = new Normal(0, 5);
            var a = _cpu.CreateVector(128, i => (float)distribution.Sample()).AsIndexable();
            var cpuResult = a.Reverse().AsIndexable();
            IIndexableFloatVector result;
            using (var gpuA = _cuda.CreateVector(a)) {
                var gpuResult = gpuA.Reverse();
                result = gpuResult.AsIndexable();
            }
            FloatMath.AreApproximatelyEqual(result, cpuResult).Should().BeTrue();
        }

        [Fact]
        public void VectorRotate()
        {
            const int blockCount = 1;
            var a = _cpu.CreateVector(4, i => i + 1).AsIndexable();
            using var gpuA = _cuda.CreateVector(a);
            a.RotateInPlace(blockCount);
            gpuA.RotateInPlace(blockCount);
            FloatMath.AreApproximatelyEqual(gpuA.AsIndexable(), a.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void VectorRotate2()
        {
            const int blockCount = 2;
            var a = _cpu.CreateVector(8, i => i + 1).AsIndexable();
            using var gpuA = _cuda.CreateVector(a);
            a.RotateInPlace(blockCount);
            gpuA.RotateInPlace(blockCount);
            FloatMath.AreApproximatelyEqual(gpuA.AsIndexable(), a.AsIndexable()).Should().BeTrue();
        }

        [Fact]
        public void TestFinite()
        {
            var vector = _cpu.CreateVector(new[] { 0f, 1f, 2f, 3f, -1f });
            vector.IsEntirelyFinite().Should().BeTrue();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeTrue();
        }

        [Fact]
        public void TestFinite2()
        {
            var vector = _cpu.CreateVector(new[] { 0f, 1f, 2f, 3f, -1f, float.Epsilon });
            vector.IsEntirelyFinite().Should().BeTrue();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeTrue();
        }

        [Fact]
        public void TestNotFinite()
        {
            var vector = _cpu.CreateVector(new[] { 0f, 1f, 2f, 3f, float.NaN });
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestNotFinite2()
        {
            var vector = _cpu.CreateVector(new[] { 0f, 1f, 2f, 3f, float.NegativeInfinity });
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeFalse();
        }

        [Fact]
        public void TestNotFinite3()
        {
            var vector = _cpu.CreateVector(new[] { 0f, 1f, 2f, 3f, float.PositiveInfinity });
            vector.IsEntirelyFinite().Should().BeFalse();

            using var gpuVector = _cuda.CreateVector(vector.AsIndexable());
            gpuVector.IsEntirelyFinite().Should().BeFalse();
        }
    }
}
