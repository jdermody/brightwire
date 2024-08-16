using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData;
using BrightData.Types;

namespace Benchmarks
{
    public class SortedArrayBenchmarks
    {
        [Params(8, 32, 128, 1024, 32768)]
        public int Size { get; set; }

        [Benchmark(Baseline = true)]
        public void SortedList() => SortedList(Enumerable.Range(0, Size).Select(x => (float)Size - x).ToArray());

        [Benchmark]
        public void IndexedSortedArray() => IndexedSortedArray(Enumerable.Range(0, Size).Select(x => (float)Size - x).ToArray());

        public static void SortedList(float[] data)
        {
            var list = new SortedList<uint, float>(data.Length);
            for(var i = 0U; i < data.Length; i++)
                list.Add(i, data[i]);
            var search = list.BinarySearch<uint, float>(0);
        }

        readonly struct IndexedValue<T>(uint index, T value) : IHaveSingleIndex
        {
            public uint Index => index;
            public T Value => value;
        }

        public static void IndexedSortedArray(float[] data)
        {
            var list = new IndexedSortedArray<IndexedValue<float>>(data.Length);
            for(var i = 0U; i < data.Length; i++)
                list.Add(new IndexedValue<float>(i, data[i]));
            var search = list.Find(0);
        }
    }
}
