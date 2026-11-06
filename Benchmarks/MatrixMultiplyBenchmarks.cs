using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BrightData;
using BrightData.LinearAlgebra;
using CommunityToolkit.HighPerformance.Buffers;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    /// <summary>
    /// Benchmarks the current (AVX2 + FMA microkernel) matrix multiplication against the previous
    /// (generic tiled dot product) implementation, operating on identical column-major-compatible
    /// layouts (row vectors contiguous) exactly as <c>MultiplyWithThisTransposed</c> feeds them.
    /// </summary>
    public unsafe class MatrixMultiplyBenchmarks
    {
        [Params(256, 512, 1024, 2048)]
        public int Size { get; set; }

        float* _a, _b, _retOld, _retNew;
        IMatrix<float> _m, _n;
        LinearAlgebraProvider<float> _lap;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _a = Allocated(Size);
            _b = Allocated(Size);
            _retOld = Allocated(Size);
            _retNew = Allocated(Size);

            // deterministic small values so dot products stay well within float range
            var size = (long)Size;
            for (var i = 0; i < size * size; i++) {
                _a[i] = ((i % 16) * 0.0625f) - 0.4f;
                _b[i] = (((i * 7) % 16) * 0.0625f) - 0.4f;
            }

            _lap = new LinearAlgebraProvider<float>(new BrightDataContext());
            _m = _lap.CreateMatrix((uint)Size, (uint)Size, (x, y) => _a[(long)x + y * Size]);
            _n = _lap.CreateMatrix((uint)Size, (uint)Size, (x, y) => _b[(long)x + y * Size]);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            NativeMemory.AlignedFree(_a);
            NativeMemory.AlignedFree(_b);
            NativeMemory.AlignedFree(_retOld);
            NativeMemory.AlignedFree(_retNew);
            _m?.Dispose();
            _n?.Dispose();
        }

        static unsafe float* Allocated(int size)
        {
            var ptr = (float*)NativeMemory.AlignedAlloc((nuint)(size * size * sizeof(float)), 32);
            // zero it so the kernels start from a deterministic state
            for (var i = 0; i < size * size; i++) ptr[i] = 0f;
            return ptr;
        }

        /// <summary>
        /// The previous approach: generic tiled dot product (MatrixMultiplyTiled3 specialised to float)
        /// </summary>
        [Benchmark(Baseline = true)]
        public double OldGenericTiled()
        {
            MatrixMultiplyTiled3(_a, _b, Size, (uint)Size, (uint)Size, _retOld);
            return Checksum(_retOld);
        }

        /// <summary>
        /// The new approach: AVX2 + FMA 4x16 microkernel (MatrixMultiplyFloat)
        /// </summary>
        [Benchmark]
        public double NewFmaAvx2()
        {
            if (Avx2.IsSupported && Fma.IsSupported)
                MatrixMultiplyFloat(_a, _b, Size, Size, Size, _retNew);
            else
                MatrixMultiplyTiled3(_a, _b, Size, (uint)Size, (uint)Size, _retNew);
            return Checksum(_retNew);
        }

        /// <summary>
        /// End-to-end multiply through the public API (includes transpose + allocation), new approach
        /// </summary>
        [Benchmark]
        public double NewFullMultiply()
        {
            using var ret = _m.Multiply(_n);
            return ret[0, 0] + ret[1, 1] + ret[2, 2];
        }

        [IterationSetup]
        public void Reset()
        {
            NativeMemory.Clear(_retOld, (nuint)(Size * Size * sizeof(float)));
            NativeMemory.Clear(_retNew, (nuint)(Size * Size * sizeof(float)));
        }

        static double Checksum(float* data)
        {
            var sum = 0.0;
            for (var i = 0; i < 32; i++)
                sum += data[i];
            return sum;
        }

        // ====== The OLD approach: generic tiled dot product (float specialisation of MatrixMultiplyTiled3) ======

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MatrixMultiplyTiled3(float* a, float* b, int size, uint rows, uint cols, float* ret)
        {
            const int L1BlockSize = 32;
            const int L2BlockSize = 64;
            var vectorSize = Vector<float>.Count;
            var numVectors = size / vectorSize;
            var ceiling = numVectors * vectorSize;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void MultiplyBlock(uint rowStart, uint colStart, uint rowEnd, uint colEnd)
            {
                for (var i = rowStart; i < rowEnd; i += L1BlockSize) {
                    for (var j = colStart; j < colEnd; j += L1BlockSize) {
                        for (uint ii = i, iLen = Math.Min(i + L1BlockSize, rows); ii < iLen; ii++) {
                            var xPtr = &a[ii * size];
                            for (uint jj = j, jLen = Math.Min(j + L1BlockSize, cols); jj < jLen; jj++) {
                                var yPtr = &b[jj * size];
                                var vSum = Vector<float>.Zero;
                                for (var z = 0; z < numVectors; z++)
                                    vSum += Vector.Load(xPtr + z * vectorSize) * Vector.Load(yPtr + z * vectorSize);

                                var sum = Vector.Dot(vSum, Vector<float>.One);
                                for (var z = ceiling; z < size; z++)
                                    sum += xPtr[z] * yPtr[z];
                                ret[jj * rows + ii] = sum;
                            }
                        }
                    }
                }
            }

            if (rows * cols >= Consts.MinimumSizeForParallel) {
                Parallel.For(0, (int)Math.Ceiling((double)rows / L2BlockSize), rowTile =>
                {
                    var rowStart = (uint)rowTile * L2BlockSize;
                    var rowEnd = rowStart + L2BlockSize;
                    for (var colTile = 0U; colTile < cols; colTile += L2BlockSize)
                        MultiplyBlock(rowStart, colTile, rowEnd, colTile + L2BlockSize);
                });
            }
            else {
                for (var rowTile = 0U; rowTile < rows; rowTile += L2BlockSize)
                    for (var colTile = 0U; colTile < cols; colTile += L2BlockSize)
                        MultiplyBlock(rowTile, colTile, rowTile + L2BlockSize, colTile + L2BlockSize);
            }
        }

        // ====== The NEW approach: AVX2 + FMA microkernel (verbatim from MutableMatrix.MatrixMultiplyFloat) ======

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static unsafe void MatrixMultiplyFloat(float* a, float* b, int K, int M, int N, float* ret)
        {
            const int BLOCK_SIZE = 64;

            Parallel.For(0, (int)Math.Ceiling((double)N / BLOCK_SIZE), jBlockIndex =>
            {
                var jjStart = jBlockIndex * BLOCK_SIZE;
                var jjEnd = Math.Min(jjStart + BLOCK_SIZE, N);
                for (int iiStart = 0; iiStart < M; iiStart += BLOCK_SIZE) {
                    int iiEnd = Math.Min(iiStart + BLOCK_SIZE, M);
                    ProcessBlock(a, b, ret, K, M, iiStart, iiEnd, jjStart, jjEnd);
                }
            });

            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            static unsafe void ProcessBlock(float* a, float* b, float* ret, int K, int strideRet, int iStart, int iEnd, int jStart, int jEnd)
            {
                for (var jj = jStart; jj < jEnd; jj++) {
                    var ptrB = b + (long)jj * K;
                    var ii = iStart;

                    for (; ii < iEnd - 3; ii += 4) {
                        var ptrA0 = a + (long)ii * K;
                        var ptrA1 = a + (long)(ii + 1) * K;
                        var ptrA2 = a + (long)(ii + 2) * K;
                        var ptrA3 = a + (long)(ii + 3) * K;

                        var sum0 = Vector256<float>.Zero;
                        var sum1 = Vector256<float>.Zero;
                        var sum2 = Vector256<float>.Zero;
                        var sum3 = Vector256<float>.Zero;

                        var k = 0;
                        var kLimit = K - 15;
                        for (; k < kLimit; k += 16) {
                            var bVec1 = Avx.LoadVector256(ptrB + k);
                            var bVec2 = Avx.LoadVector256(ptrB + k + 8);

                            sum0 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA0 + k), bVec1, sum0);
                            sum0 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA0 + k + 8), bVec2, sum0);
                            sum1 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA1 + k), bVec1, sum1);
                            sum1 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA1 + k + 8), bVec2, sum1);
                            sum2 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA2 + k), bVec1, sum2);
                            sum2 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA2 + k + 8), bVec2, sum2);
                            sum3 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA3 + k), bVec1, sum3);
                            sum3 = Fma.MultiplyAdd(Avx.LoadVector256(ptrA3 + k + 8), bVec2, sum3);
                        }
                        var s0 = HorizontalAdd(sum0);
                        var s1 = HorizontalAdd(sum1);
                        var s2 = HorizontalAdd(sum2);
                        var s3 = HorizontalAdd(sum3);

                        for (; k < K; k++) {
                            var bVal = ptrB[k];
                            s0 += ptrA0[k] * bVal;
                            s1 += ptrA1[k] * bVal;
                            s2 += ptrA2[k] * bVal;
                            s3 += ptrA3[k] * bVal;
                        }

                        var baseIdx = (long)jj * strideRet + ii;
                        ret[baseIdx] = s0;
                        ret[baseIdx + 1] = s1;
                        ret[baseIdx + 2] = s2;
                        ret[baseIdx + 3] = s3;
                    }

                    for (; ii < iEnd; ii++) {
                        var ptrA = a + (long)ii * K;
                        var vSum = Vector256<float>.Zero;
                        var k = 0;
                        for (; k <= K - 8; k += 8)
                            vSum = Fma.MultiplyAdd(Avx.LoadVector256(ptrA + k), Avx.LoadVector256(ptrB + k), vSum);

                        var sum = HorizontalAdd(vSum);
                        for (; k < K; k++)
                            sum += ptrA[k] * ptrB[k];
                        ret[(long)jj * strideRet + ii] = sum;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float HorizontalAdd(Vector256<float> v)
            {
                var vLow = v.GetLower();
                var vHigh = Avx.ExtractVector128(v, 1);
                var v128 = Sse.Add(vLow, vHigh);
                v128 = Sse3.HorizontalAdd(v128, v128);
                v128 = Sse3.HorizontalAdd(v128, v128);
                return v128.ToScalar();
            }
        }
    }
}
