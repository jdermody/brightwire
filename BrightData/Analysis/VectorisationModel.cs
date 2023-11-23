using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Operations.Vectorisation;
using BrightData.Types;
using CommunityToolkit.HighPerformance;

namespace BrightData.Analysis
{
    public class VectorisationModel
    {
        readonly record struct ArrayWrapper(object[] Values) : ICanRandomlyAccessData
        {
            public void Dispose()
            {
            }

            public uint Size => (uint)Values.Length;
            public object this[int index] => Values[index];
            public object this[uint index] => Values[index];
        }

        public VectorisationModel(ICanVectorise[] vectorisers)
        {
            Vectorisers = vectorisers;
            OutputSize = (uint)vectorisers.Sum(x => x.OutputSize);
        }

        public uint OutputSize { get; }
        public ICanVectorise[] Vectorisers { get; }
        public uint[]? SourceColumnIndices { get; set; }

        public async IAsyncEnumerable<float[,]> Vectorise(IReadOnlyBuffer[] buffers, MetaData[]? metaData)
        {
            if(metaData is not null && metaData.Length != Vectorisers.Length)
                throw new ArgumentException("If meta data is provided then there should be one for each vectoriser", nameof(metaData));
            
            await foreach(var item in DoVectorise(buffers))
                yield return item;

            if (metaData is not null) {
                for (int i = 0, len = metaData.Length; i < len; i++)
                    Vectorisers[i].WriteTo(metaData[i]);
            }
        }

        public delegate void VectorCallback(ReadOnlySpan<float> vector);

        public async Task ForEachVector(IReadOnlyBufferWithMetaData[] buffers, VectorCallback callback)
        {
            await foreach (var block in Vectorise(buffers)) {
                Process(block);
            }
            return;

            void Process(ReadOnlySpan2D<float> data)
            {
                for (var i = 0; i < data.Height; i++)
                    callback(data.GetRowSpan(i));
            }
        }

        public float[] Vectorise(ICanRandomlyAccessData data)
        {
            var ret = new float[OutputSize];
            var span = ret.AsSpan();
            uint index = 0;

            var indices = SourceColumnIndices ?? stackalloc uint[(int)data.Size];
            if (SourceColumnIndices is null) {
                for(int i = 0; i < data.Size; i++)
                    indices[i] = (uint)i;
            }

            for(int i = 0, len = Vectorisers.Length; i < len; i++) {
                var vectoriser = Vectorisers[i];
                vectoriser.Vectorise(data[indices[i]], span.Slice((int)index, (int)vectoriser.OutputSize));
                index += vectoriser.OutputSize;
            }
            return ret;
        }

        public float[] Vectorise(params object[] values) => Vectorise(new ArrayWrapper(values));

        public async IAsyncEnumerable<float[,]> Vectorise(IReadOnlyBufferWithMetaData[] buffers)
        {
            await foreach(var item in DoVectorise(buffers))
                yield return item;

            for (int i = 0, len = buffers.Length; i < len; i++)
                Vectorisers[i].WriteTo(buffers[i].MetaData);
        }

        public IAsyncEnumerable<float[,]> Vectorise(IDataTable table, params uint[] columnIndices) => Vectorise(table.GetColumns(columnIndices));

        async IAsyncEnumerable<float[,]> DoVectorise<T>(T[] buffers) where T: IReadOnlyBuffer
        {
            if (buffers.Length != Vectorisers.Length)
                throw new ArgumentException($"Expected to receive {Vectorisers.Length} buffers (not {buffers.Length})", nameof(buffers));
            var first = buffers[0];
            if (buffers.Skip(1).Any(x => x.Size != first.Size || x.BlockSize != first.BlockSize))
                throw new ArgumentException("Expected all buffers to have the same size and block size", nameof(buffers));

            var len = Vectorisers.Length;
            var tasks = new Task[len];

            for (uint i = 0; i < first.BlockCount; i++) {
                var blockSize = (i+1 == first.BlockCount)
                    ? first.Size - first.BlockSize * i
                    : first.BlockSize;
                var buffer = new float[blockSize, OutputSize];
                uint offset = 0;
                var index = 0;
                for(var j = 0; j < len; j++) {
                    var vectoriser = Vectorisers[j];
                    tasks[index++] = vectoriser.WriteBlock(buffers[j], i, offset, buffer);
                    offset += vectoriser.OutputSize;
                }
                await Task.WhenAll(tasks);
                yield return buffer;
            }
        }
    }
}
