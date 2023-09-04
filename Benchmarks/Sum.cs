using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using BrightData;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    public class Sum
    {
        const int Length = 32 * 1024;
        float[] data;

        [GlobalSetup]
        public void GlobalSetup()
        {
            data = Enumerable.Range(0, Length).Select(x => (float)x).ToArray();
        }

        [Benchmark(Baseline = true)]
        public float SimpleSum() => SimpleSum(data);

        [Benchmark]
        public float VectorSum() => VectorSum(data);

        [Benchmark]
        public float VectorSum2() => VectorSum2(data);

        [Benchmark]
        public float VectorSum3() => VectorSum3(data);

        [Benchmark]
        public float VectorAligned() => VectorAligned(data);

        [Benchmark]
        public float VectorBulkAligned() => VectorBulkAligned(data);

        public static float SimpleSum(Span<float> data)
        {
            float ret = 0;
            foreach(var item in data)
                ret += item;
            return ret;
        }

        public static unsafe float VectorSum(Span<float> source)
        {
            const int VectorSize = 256 / sizeof(float) / 8;
            fixed (float* ptr = source) {
                var size = source.Length;
                float* p = ptr, end = ptr + size;
                var vectorSum = Vector256<float>.Zero;
                for(int i = 0, numVectors = size / VectorSize; i < numVectors; i++) {
                    var current = Avx.LoadVector256(p);
                    p += VectorSize;
                    vectorSum = Avx.Add(current, vectorSum);
                }

                // sum the vector
                var temp = stackalloc float[VectorSize];
                Avx.Store(temp, vectorSum);
                var ret = 0f;
                for (var i = 0; i < VectorSize; i++)
                    ret += temp[i];

                // sum any remaining values
                while (p < end)
                    ret += *p++;

                return ret;
            }
        }

        const ulong AlignmentMask = 31UL;
        public static unsafe float VectorAligned(Span<float> source)
        {
            const int VectorSize = 256 / sizeof(float) / 8;
            fixed (float* ptr = source) {
                var ret = 0f;
                var size = source.Length;
                float* p = ptr, end = ptr + size;

                // sum the pre aligned values
                var alignedStart = (float*)(((ulong)ptr + AlignmentMask) & ~AlignmentMask);
                var startPos = (int)(alignedStart - ptr);
                for (var i = 0; i < startPos && p < end; i++)
                    ret += *p++;

                // vector sum the aligned values
                var vectorSum = Vector256<float>.Zero;
                var numVectors = (size - startPos) / VectorSize;
                for(var i = 0; i < numVectors; i++) {
                    var current = Avx.LoadAlignedVector256(p);
                    p += VectorSize;
                    vectorSum = Avx.Add(current, vectorSum);
                }

                // sum the output vector
                var temp = stackalloc float[VectorSize];
                Avx.Store(temp, vectorSum);
                for (var i = 0; i < VectorSize; i++)
                    ret += temp[i];

                // sum any remaining values
                while (p < end)
                    ret += *p++;

                return ret;
            }
        }

        public static unsafe float VectorBulkAligned(Span<float> source)
        {
            const int VectorSize = 256 / sizeof(float) / 8;
            fixed (float* ptr = source) {
                var ret = 0f;
                var size = source.Length;
                float* p = ptr, end = ptr + size;

                // sum the pre aligned values
                var alignedStart = (float*)(((ulong)ptr + AlignmentMask) & ~AlignmentMask);
                var startPos = (int)(alignedStart - ptr);
                for (var i = 0; i < startPos && p < end; i++)
                    ret += *p++;

                // bulk vector sum
                var vectorSum = Vector256<float>.Zero;
                var vectorIndex = 0;
                var numVectors = (size - startPos) / VectorSize;
                for (var numBulkVectors = numVectors / 4 * 4; vectorIndex < numBulkVectors; vectorIndex += 4) {
                    var block0 = Avx.LoadAlignedVector256(p);
                    var block1 = Avx.LoadAlignedVector256(p + VectorSize);
                    var block2 = Avx.LoadAlignedVector256(p + VectorSize * 2);
                    var block3 = Avx.LoadAlignedVector256(p + VectorSize * 3);
                    p += VectorSize * 4;
                    vectorSum = Avx.Add(block0, vectorSum);
                    vectorSum = Avx.Add(block1, vectorSum);
                    vectorSum = Avx.Add(block2, vectorSum);
                    vectorSum = Avx.Add(block3, vectorSum);
                }

                // vector sum the aligned values
                for(; vectorIndex < numVectors; vectorIndex++) {
                    var current = Avx.LoadAlignedVector256(p);
                    p += VectorSize;
                    vectorSum = Avx.Add(current, vectorSum);
                }

                // sum the output vector
                var temp = stackalloc float[VectorSize];
                Avx.Store(temp, vectorSum);
                for (var i = 0; i < VectorSize; i++)
                    ret += temp[i];

                // sum any remaining values
                while (p < end)
                    ret += *p++;

                return ret;
            }
        }

        public static float VectorSum2(Span<float> span)
        {
            var size = span.Length;
            var nextIndex = 0;
            var ret = 0f;

            if (size >= Consts.MinimumSizeForVectorised) {
                var vectorSize = Vector<float>.Count;
                var leftVec = MemoryMarshal.Cast<float, Vector<float>>(span);
                var numVectors = size / vectorSize;
                nextIndex = numVectors * vectorSize;
                for (var i = 0; i < numVectors; i++)
                    ret += Vector.Sum(leftVec[i]);
            }
            for (; nextIndex < size; nextIndex++)
                ret += span[nextIndex];
            return ret;
        }

        public static unsafe float VectorSum3(Span<float> span)
        {
            var size = span.Length;
            var ret = 0f;

            fixed (float* xp = span) {
                float* p = xp, end = xp + size;
                if (size >= Consts.MinimumSizeForVectorised) {
                    var vectorSize = Vector<float>.Count;
                    var vectors = (Vector<float>*)xp;
                    var numVectors = size / vectorSize;
                    for (var i = 0; i < numVectors; i++)
                        ret += Vector.Sum(*vectors++);
                    p += vectorSize * numVectors;
                }

                while (p < end)
                    ret += *p++;
            }

            return ret;
        }
    }
}
