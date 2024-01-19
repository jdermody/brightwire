using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    public unsafe class Dot
    {
        const int Length = 32 * 1024;
        float[] data1, data2;
        float* _ptr1, _ptr2;

        [GlobalSetup]
        public void GlobalSetup()
        {
            data1 = Enumerable.Range(0, Length).Select(x => (float)x).ToArray();
            data2 = Enumerable.Range(0, Length).Select(x => (float)(x + 1)).ToArray();
            _ptr1 = GetAligned(data1);
            _ptr2 = GetAligned(data2);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            NativeMemory.AlignedFree(_ptr1);
            NativeMemory.AlignedFree(_ptr2);
        }

        [Benchmark(Baseline = true)]
        public float SimpleDot() => SimpleDot(data1, data2);

        [Benchmark]
        public float VectorDot() => VectorDot(data1, data2);

        [Benchmark]
        public float VectorDotAligned() => VectorAligned(data1, data2);

        [Benchmark]
        public float VectorDotAligned2() => VectorAligned2(_ptr1, _ptr2, Length);

        [Benchmark]
        public float VectorDotAligned3() => VectorAligned3(_ptr1, _ptr2, Length);

        public static float SimpleDot(Span<float> data1, Span<float> data2)
        {
            var ret = 0f;
            var size = data1.Length;
            for (var i = 0; i < size; i++)
                ret += data1[i] * data2[i];
            return ret;
        }

        public static float* GetAligned(Span<float> data)
        {
            var size = (uint)(data.Length * sizeof(float));
            var ptr = (float*)NativeMemory.AlignedAlloc(size, 32);
            fixed(float* source = data)
                NativeMemory.Copy(source, ptr, size);
            return ptr;
        }

        public static unsafe float VectorDot(Span<float> data1, Span<float> data2)
        {
            const int VectorSize = 256 / sizeof(float) / 8;
            fixed (float* ptr1 = data1)
            fixed (float* ptr2 = data2) {
                var size = data1.Length;
                float* p1 = ptr1, end1 = ptr1 + size;
                float* p2 = ptr2;
                var vectorSum = Vector256<float>.Zero;
                for(int i = 0, numVectors = size / VectorSize; i < numVectors; i++) {
                    var left = Avx.LoadVector256(p1);
                    p1 += VectorSize;
                    var right = Avx.LoadVector256(p2);
                    p2 += VectorSize;
                    vectorSum += left * right;
                }

                // sum the result vector
                var temp = stackalloc float[VectorSize];
                Avx.Store(temp, vectorSum);
                var ret = 0f;
                for (var i = 0; i < VectorSize; i++)
                    ret += temp[i];

                // sum any remaining values
                while (p1 < end1)
                    ret += *p1++ * *p2++;

                return ret;
            }
        }

        public static float VectorAligned(Span<float> data1, Span<float> data2)
        {
            const int VectorSize = 256 / sizeof(float) / 8;
            var ptr1 = GetAligned(data1);
            var ptr2 = GetAligned(data2);

            try {
                var size = data1.Length;
                float* p1 = ptr1, end1 = ptr1 + size;
                float* p2 = ptr2;
                var vectorSum = Vector256<float>.Zero;
                for (int i = 0, numVectors = size / VectorSize; i < numVectors; i++) {
                    var left = Avx.LoadAlignedVector256(p1);
                    p1 += VectorSize;
                    var right = Avx.LoadAlignedVector256(p2);
                    p2 += VectorSize;
                    vectorSum += left * right;
                }

                // sum the result vector
                var temp = stackalloc float[VectorSize];
                Avx.Store(temp, vectorSum);
                var ret = 0f;
                for (var i = 0; i < VectorSize; i++)
                    ret += temp[i];

                // sum any remaining values
                while (p1 < end1)
                    ret += *p1++ * *p2++;
                return ret;
            }
            finally {
                NativeMemory.AlignedFree(ptr1);
                NativeMemory.AlignedFree(ptr2);
            }
        }

        public static float VectorAligned2(float* ptr1, float* ptr2, int size)
        {
            const int VectorSize = 256 / sizeof(float) / 8;

            float* p1 = ptr1, end1 = ptr1 + size;
            float* p2 = ptr2;
            var vectorSum = Vector256<float>.Zero;
            for (int i = 0, numVectors = size / VectorSize; i < numVectors; i++) {
                var left = Avx.LoadAlignedVector256(p1);
                p1 += VectorSize;
                var right = Avx.LoadAlignedVector256(p2);
                p2 += VectorSize;
                vectorSum += left * right;
            }

            // sum the result vector
            var temp = stackalloc float[VectorSize];
            Avx.Store(temp, vectorSum);
            var ret = 0f;
            for (var i = 0; i < VectorSize; i++)
                ret += temp[i];

            // sum any remaining values
            while (p1 < end1)
                ret += *p1++ * *p2++;
            return ret;
        }

        public static float VectorAligned3(float* ptr1, float* ptr2, int size)
        {
            const int VectorSize = 256 / sizeof(float) / 8;

            float* p1 = ptr1, end1 = ptr1 + size;
            float* p2 = ptr2;
            var vectorSum = Vector256<float>.Zero;

            // bulk vector sum
            var vectorIndex = 0;
            var numVectors = size / VectorSize;
            for (var numBulkVectors = numVectors / 4 * 4; vectorIndex < numBulkVectors; vectorIndex += 4) {
                var a1 = Avx.LoadAlignedVector256(p1);
                var b1 = Avx.LoadAlignedVector256(p1 + VectorSize);
                var c1 = Avx.LoadAlignedVector256(p1 + VectorSize * 2);
                var d1 = Avx.LoadAlignedVector256(p1 + VectorSize * 3);
                p1 += VectorSize * 4;

                var a2 = Avx.LoadAlignedVector256(p2);
                var b2 = Avx.LoadAlignedVector256(p2 + VectorSize);
                var c2 = Avx.LoadAlignedVector256(p2 + VectorSize * 2);
                var d2 = Avx.LoadAlignedVector256(p2 + VectorSize * 3);
                p2 += VectorSize * 4;

                vectorSum += a1 * a2;
                vectorSum += b1 * b2;
                vectorSum += c1 * c2;
                vectorSum += d1 * d2;
            }

            // vector sum
            for (; vectorIndex < numVectors; vectorIndex++) {
                var left = Avx.LoadAlignedVector256(p1);
                p1 += VectorSize;
                var right = Avx.LoadAlignedVector256(p2);
                p2 += VectorSize;
                vectorSum += left * right;
            }

            // sum the result vector
            var temp = stackalloc float[VectorSize];
            Avx.Store(temp, vectorSum);
            var ret = 0f;
            for (var i = 0; i < VectorSize; i++)
                ret += temp[i];

            // sum any remaining values
            while (p1 < end1)
                ret += *p1++ * *p2++;
            return ret;
        }
    }
}
