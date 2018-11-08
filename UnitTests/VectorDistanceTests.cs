using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire;
using MathNet.Numerics.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Helper;

namespace UnitTests
{
	[TestClass]
	public class VectorDistanceTests
	{
		static ILinearAlgebraProvider _cuda;
		static ILinearAlgebraProvider _cpu;

		[ClassInitialize]
		public static void Load(TestContext context)
		{
			_cuda = BrightWireGpuProvider.CreateLinearAlgebra(false);
			_cpu = BrightWireProvider.CreateLinearAlgebra(false);
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			_cuda.Dispose();
			_cpu.Dispose();
		}

		[TestMethod]
		public void TestManhattanDistance()
		{
			var distribution = new Normal(0, 5);
			var vectors = Enumerable.Range(0, 10).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToList();
			var compareTo = Enumerable.Range(0, 20).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToList();
			var distances = _cpu.CalculateDistances(vectors, compareTo, DistanceMetric.Manhattan);

			var gpuVectors = vectors.Select(v => _cuda.CreateVector(v)).ToList();
			var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v)).ToList();
			var gpuDistances = _cuda.CalculateDistances(gpuVectors, gpuCompareTo, DistanceMetric.Manhattan);

			FloatingPointHelper.AssertEqual(distances.AsIndexable(), gpuDistances.AsIndexable());
		}

		[TestMethod]
		public void TestEuclideanDistance()
		{
			var distribution = new Normal(0, 5);
			var vectors = Enumerable.Range(0, 10).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToList();
			var compareTo = Enumerable.Range(0, 20).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToList();
			var distances = _cpu.CalculateDistances(vectors, compareTo, DistanceMetric.Euclidean);

			var gpuVectors = vectors.Select(v => _cuda.CreateVector(v)).ToList();
			var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v)).ToList();
			var gpuDistances = _cuda.CalculateDistances(gpuVectors, gpuCompareTo, DistanceMetric.Euclidean);

			FloatingPointHelper.AssertEqual(distances.AsIndexable(), gpuDistances.AsIndexable());
		}

		[TestMethod]
		public void TestCosineDistance()
		{
			var distribution = new Normal(0, 5);
			var vectors = Enumerable.Range(0, 10).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToList();
			var compareTo = Enumerable.Range(0, 20).Select(i => _cpu.CreateVector(100, j => Convert.ToSingle(distribution.Sample())).AsIndexable()).ToList();

			var distances = _cpu.CalculateDistances(vectors, compareTo, DistanceMetric.Cosine);

			var gpuVectors = vectors.Select(v => _cuda.CreateVector(v)).ToList();
			var gpuCompareTo = compareTo.Select(v => _cuda.CreateVector(v)).ToList();
			var gpuDistances = _cuda.CalculateDistances(gpuVectors, gpuCompareTo, DistanceMetric.Cosine);

			FloatingPointHelper.AssertEqual(distances.AsIndexable(), gpuDistances.AsIndexable());
		}
	}
}
