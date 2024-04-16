using System.Runtime.InteropServices;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSetProperty();
            BenchmarkSum();
            BenchmarkDot();
        }

        static void BenchmarkSetProperty()
        {
            BenchmarkRunner.Run<SetPropertyFromExpression>();
        }

        static void BenchmarkSum()
        {
            var data1 = Enumerable.Range(0, 256).Select(x => (float)x).ToArray();
            Console.WriteLine($"Sum Baseline:       {Sum.SimpleSum(data1):N0}");
            Console.WriteLine($"Vector Sum:         {Sum.VectorSum(data1):N0}");
            Console.WriteLine($"Vector Sum 2:       {Sum.VectorSum2(data1):N0}");
            Console.WriteLine($"Vector Sum 3:       {Sum.VectorSum3(data1):N0}");
            Console.WriteLine($"Vector Aligned:     {Sum.VectorAligned(data1):N0}");
            Console.WriteLine($"Vector Bulk Aligned:{Sum.VectorBulkAligned(data1):N0}");
            BenchmarkRunner.Run<Sum>();
        }

        static unsafe void BenchmarkDot()
        {
            var data1 = Enumerable.Range(0, 256).Select(x => (float)x).ToArray();
            var data2 = Enumerable.Range(0, 256).Select(x => (float)(x+1)).ToArray();
            var alignedPtr1 = Dot.GetAligned(data1);
            var alignedPtr2 = Dot.GetAligned(data2);
            try {
                Console.WriteLine($"Baseline:           {Dot.SimpleDot(data1, data2):N0}");
                Console.WriteLine($"Vector Dot:         {Dot.VectorDot(data1, data2):N0}");
                Console.WriteLine($"Vector Dot Aligned: {Dot.VectorAligned(data1, data2):N0}");
                Console.WriteLine($"Vector Dot Aligned2:{Dot.VectorAligned2(alignedPtr1, alignedPtr2, 256):N0}");
                Console.WriteLine($"Vector Dot Aligned3:{Dot.VectorAligned3(alignedPtr1, alignedPtr2, 256):N0}");
                BenchmarkRunner.Run<Dot>();
            }
            finally {
                NativeMemory.Free(alignedPtr1);
                NativeMemory.Free(alignedPtr2);
            }
        }
    }
}