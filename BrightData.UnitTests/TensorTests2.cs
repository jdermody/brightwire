﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.Cuda;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightData.MKL;
using BrightData.UnitTests.Helper;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace BrightData.UnitTests
{
    public class TensorTests2 : UnitTestBase
    {
        readonly LinearAlgebraProvider<float> _linearAlgebraProvider;
        readonly MklLinearAlgebraProvider     _mklLinearAlgebraProvider;
        readonly CudaProvider                 _cudaProvider;
        readonly CudaLinearAlgebraProvider    _cudaLinearAlgebraProvider;
        readonly ITestOutputHelper            _output;

        static readonly uint DefaultVectorSize  = 1000;
        static readonly uint DefaultRowCount    = 200;
        static readonly uint DefaultColumnCount = 200;

        public TensorTests2(ITestOutputHelper output)
        {
            _output                    = output;
            _linearAlgebraProvider     = new LinearAlgebraProvider<float>(_context);
            _mklLinearAlgebraProvider  = new MklLinearAlgebraProvider(_context);
            _cudaProvider              = _context.CreateCudaProvider(Path.Combine(Environment.CurrentDirectory, "cuda", "brightwire.ptx"));
            _cudaLinearAlgebraProvider = new CudaLinearAlgebraProvider(_context, _cudaProvider);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            _linearAlgebraProvider.Dispose();
            _cudaLinearAlgebraProvider.Dispose();
            _cudaProvider.Dispose();
            _mklLinearAlgebraProvider.Dispose();
            base.Dispose();
        }

        [Fact]
        public void TestCudaDirectInitialization()
        {
            var span = new ReadOnlySpan<float>([1, 2, 3, 4, 5, 6, 7, 8], 2, 2);
            var segment = _cudaLinearAlgebraProvider.CreateSegment((uint)span.Length, false);
            segment.CopyFrom(span);
            var test = segment.ToNewArray();
            test.Should().BeEquivalentTo(span.ToArray());
        }

        [Fact]
        public void Serialisation()
        {
            using var vector = _linearAlgebraProvider.CreateVector(10, i => i + 1);
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer, Encoding.UTF8, true);
            vector.WriteTo(writer);

            buffer.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(buffer, Encoding.UTF8, true);
            using var vector2 = _context.Create<IVector<float>>(reader);
            vector2.ToArray().Should().BeEquivalentTo(vector.ToArray());
        }

        void Test<TT, R>(TT simple, TT mkl, TT cuda, Func<TT, TT, R> test, Func<R, R, bool> verifyResult)
            where TT: ITensor<float>
        {
            try {
                simple.Segment.CopyTo(mkl.Segment);
                simple.Segment.CopyTo(cuda.Segment);
                using var simple2 = (TT)simple.Clone();
                using var mkl2 = (TT)mkl.Clone();
                using var cuda2 = (TT)cuda.Clone();

                var ret = new (R Result, string Type, TimeSpan Time)[3];

                var sw = Stopwatch.StartNew();
                var r1 = test(simple, simple2);
                sw.Stop();
                ret[0] = (r1, "default", sw.Elapsed);

                sw.Restart();
                var r2 = test(mkl, mkl2);
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
            }
            finally {
                mkl.Dispose();
                cuda.Dispose();
            }
        }

        [Fact]
        public void DotProduct()
        {
            using var vector = _linearAlgebraProvider.CreateVector(DefaultVectorSize, false);
            vector.MapIndexedInPlace((i, v) => i);

            Test(
                vector, 
                _mklLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                _cudaLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                (v1, v2) => v1.DotProduct(v2),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixTranspose()
        {
            var index = 1;
            const int rowCount = 3, columnCount = 2;
            using var matrix = _linearAlgebraProvider.CreateMatrix(rowCount, columnCount, (i, j) => index++);

            Test(
                matrix, 
                _mklLinearAlgebraProvider.CreateMatrix(rowCount, columnCount, false), 
                _cudaLinearAlgebraProvider.CreateMatrix(rowCount, columnCount, false), 
                (v1, v2) => v1.Transpose(),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixMultiply()
        {
            var index = 1;
            uint rowCount = 2;
            uint columnCount = 2;
            using var matrix = _linearAlgebraProvider.CreateMatrix(rowCount, columnCount, (i, j) => index++);

            Test(
                matrix, 
                _mklLinearAlgebraProvider.CreateMatrix(rowCount, columnCount, false), 
                _cudaLinearAlgebraProvider.CreateMatrix(rowCount, columnCount, false), 
                (v1, v2) => v1.Multiply(v2),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixTransposeThisAndMultiply()
        {
            using var matrix = _linearAlgebraProvider.CreateMatrix(DefaultRowCount, DefaultColumnCount, false);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            Test(
                matrix, 
                _mklLinearAlgebraProvider.CreateMatrix(DefaultRowCount, DefaultColumnCount, false), 
                _cudaLinearAlgebraProvider.CreateMatrix(DefaultRowCount, DefaultColumnCount, false), 
                (v1, v2) => v1.TransposeThisAndMultiply(v2),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void MatrixTransposeAndMultiply()
        {
            using var matrix = _linearAlgebraProvider.CreateMatrix(DefaultRowCount, DefaultColumnCount, false);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            Test(
                matrix, 
                _mklLinearAlgebraProvider.CreateMatrix(DefaultRowCount, DefaultColumnCount, false), 
                _cudaLinearAlgebraProvider.CreateMatrix(DefaultRowCount, DefaultColumnCount, false), 
                (v1, v2) => v1.TransposeAndMultiply(v2),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void Svd()
        {
            const int rowCount = 2, columnCount = 2;
            using var matrix = _linearAlgebraProvider.CreateMatrix(rowCount, columnCount, false);
            matrix.MapIndexedInPlace((i, j, v) => (i+1) * (j+1));

            var cudaMatrix = _cudaLinearAlgebraProvider.CreateMatrix(rowCount, columnCount, matrix.Segment);
            var (u, s, vt) = cudaMatrix.Svd();

            var mklMatrix = _mklLinearAlgebraProvider.CreateMatrix(rowCount, columnCount, matrix.Segment);
            var (u2, s2, vt2) = mklMatrix.Svd();
        }

        [Fact]
        public void L2Norm()
        {
            using var vector = _linearAlgebraProvider.CreateVector(DefaultVectorSize, false);
            vector.MapIndexedInPlace((i, v) => i);

            Test(
                vector, 
                _mklLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                _cudaLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                (v1, v2) => v1.L2Norm(),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void PointwiseMultiply()
        {
            using var vector = _linearAlgebraProvider.CreateVector(DefaultVectorSize, false);
            vector.MapIndexedInPlace((i, v) => i);

            Test(
                vector, 
                _mklLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                _cudaLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                (v1, v2) => v1.PointwiseMultiply(v2),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact]
        public void PointwiseDivide()
        {
            using var vector = _linearAlgebraProvider.CreateVector(DefaultVectorSize, false);
            vector.MapIndexedInPlace((i, v) => i+1);

            Test(
                vector, 
                _mklLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                _cudaLinearAlgebraProvider.CreateVector(DefaultVectorSize, false), 
                (v1, v2) => v1.PointwiseDivide(v2),
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2, 14)
            );
        }

        [Fact]
        public void TestEuclideanDistance()
        {
            using var vector1 = _linearAlgebraProvider.CreateVector(4, 1f);
            using var vector2 = _linearAlgebraProvider.CreateVector(4, 2f);
            using var vector3 = _linearAlgebraProvider.CreateVector(4, 3f);
            using var vector4 = _linearAlgebraProvider.CreateVector(4, 4f);

            var distance1 = vector1.EuclideanDistance(vector3);
            var distance2 = vector1.EuclideanDistance(vector4);
            var distance3 = vector2.EuclideanDistance(vector3);
            var distance4 = vector2.EuclideanDistance(vector4);

            var vectorGroup1 = new[] { vector1, vector2 };
            var vectorGroup2 = new[] { vector3, vector4 };

            using var vector = vector1.FindDistances(vectorGroup2, DistanceMetric.Euclidean);
            vector[0].Should().Be(distance1);
            vector[1].Should().Be(distance2);

            using var matrix = _linearAlgebraProvider.FindDistances(vectorGroup1, vectorGroup2, DistanceMetric.Euclidean);

            matrix[0, 0].Should().Be(distance1);
            matrix[1, 0].Should().Be(distance2);
            matrix[0, 1].Should().Be(distance3);
            matrix[1, 1].Should().Be(distance4);
        }

        [Fact]
        public void TestSum()
        {
            using var vector = _linearAlgebraProvider.CreateVector(1025.AsRange().Select(i => (float)(i + 1)).ToArray());
            Test(
                vector, 
                _mklLinearAlgebraProvider.CreateVector(vector), 
                _cudaLinearAlgebraProvider.CreateVector(vector), 
                (a, b) => a.Sum(), 
                (r1, r2) => Math<float>.AreApproximatelyEqual(r1, r2)
            );
        }

        [Fact] public void TestTensorMultiply()
        {
            uint index = 1;
            using var tensor = _linearAlgebraProvider.CreateTensor3D(
                _linearAlgebraProvider.CreateMatrix(2, 2, (_, _) => index++),
                _linearAlgebraProvider.CreateMatrix(2, 2, (_, _) => index++)
            );
            index = 1;

            using var matrix = _linearAlgebraProvider.CreateMatrix(2, 2, (_, _) => index++);
            using var multiply1 = tensor.MultiplyEachMatrixBy(matrix);

            using var matrix2 = tensor.Reshape(tensor.Depth * tensor.RowCount, tensor.ColumnCount);
            using var transposed = matrix2.Transpose();
            using var matrix3 = transposed.Reshape(tensor.Depth * tensor.RowCount, tensor.ColumnCount);
            using var multiply2 = matrix3.Multiply(matrix);
            using var tensor2 = multiply2.Reshape(2, 2, 2);
        }
    }
}
