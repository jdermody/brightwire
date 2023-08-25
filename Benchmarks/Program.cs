using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BrightData;
using CommunityToolkit.HighPerformance;

namespace Benchmarks
{
    public class Program
    {
        static unsafe void Main(string[] args)
        {
            var data1 = Enumerable.Range(0, 256).Select(x => (float)x).ToArray();
            var data2 = Enumerable.Range(0, 256).Select(x => (float)(x+1)).ToArray();
            var alignedPtr1 = DotBenchmark.GetAligned(data1);
            var alignedPtr2 = DotBenchmark.GetAligned(data2);
            try {
                //Console.WriteLine($"Sum Baseline:       {SumBenchmark.SimpleSum(data1):N0}");
                //Console.WriteLine($"Vector Sum:         {SumBenchmark.VectorSum(data1):N0}");
                //Console.WriteLine($"Vector Sum 2:       {SumBenchmark.VectorSum2(data1):N0}");
                //Console.WriteLine($"Vector Sum 3:       {SumBenchmark.VectorSum3(data1):N0}");
                //Console.WriteLine($"Vector Aligned:     {SumBenchmark.VectorAligned(data1):N0}");
                //Console.WriteLine($"Vector Bulk Aligned:{SumBenchmark.VectorBulkAligned(data1):N0}");
                //BenchmarkRunner.Run<SumBenchmark>();

                Console.WriteLine($"Baseline:           {DotBenchmark.SimpleDot(data1, data2):N0}");
                Console.WriteLine($"Vector Dot:         {DotBenchmark.VectorDot(data1, data2):N0}");
                Console.WriteLine($"Vector Dot Aligned: {DotBenchmark.VectorAligned(data1, data2):N0}");
                Console.WriteLine($"Vector Dot Aligned2:{DotBenchmark.VectorAligned2(alignedPtr1, alignedPtr2, 256):N0}");
                Console.WriteLine($"Vector Dot Aligned3:{DotBenchmark.VectorAligned3(alignedPtr1, alignedPtr2, 256):N0}");
                //BenchmarkRunner.Run<DotBenchmark>();
            }
            finally {
                NativeMemory.Free(alignedPtr1);
                NativeMemory.Free(alignedPtr2);
            }
        }
    }
}