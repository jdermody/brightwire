using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData.Types;
using CommunityToolkit.HighPerformance;

namespace BrightData.DataTable
{
    /// <summary>
    /// A collection of vectorisers (such as from the columns of a data table)
    /// </summary>
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

        /// <summary>
        /// Initialise the model from a collection of vectorisers
        /// </summary>
        /// <param name="vectorisers"></param>
        public VectorisationModel(ICanVectorise[] vectorisers)
        {
            Vectorisers = vectorisers;
            OutputSize = (uint)vectorisers.Sum(x => x.OutputSize);
        }

        /// <summary>
        /// The combined output size of all vectorisers
        /// </summary>
        public uint OutputSize { get; }

        /// <summary>
        /// The source vectorisers
        /// </summary>
        public ICanVectorise[] Vectorisers { get; }

        /// <summary>
        /// The source column indices from the data table (if available)
        /// </summary>
        public uint[]? SourceColumnIndices { get; set; }

        /// <summary>
        /// Vectorise the buffers as an enumerable
        /// </summary>
        /// <param name="buffers">Buffers to vectoriser</param>
        /// <param name="metaData">Metadata in which to store the vectorisation parameters</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async IAsyncEnumerable<float[,]> Vectorise(IReadOnlyBuffer[] buffers, MetaData[]? metaData)
        {
            if (metaData is not null && metaData.Length != Vectorisers.Length)
                throw new ArgumentException("If meta data is provided then there should be one for each vectoriser", nameof(metaData));

            await foreach (var item in DoVectorise(buffers))
                yield return item;

            if (metaData is not null)
            {
                for (int i = 0, len = metaData.Length; i < len; i++)
                    Vectorisers[i].WriteTo(metaData[i]);
            }
        }

        /// <summary>
        /// Called for each vector
        /// </summary>
        public delegate void VectorCallback(ReadOnlySpan<float> vector);

        /// <summary>
        /// Vectorise the buffers and pass each vector to a callback
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task ForEachVector(IReadOnlyBufferWithMetaData[] buffers, VectorCallback callback)
        {
            await foreach (var block in Vectorise(buffers))
            {
                Process(block);
            }
            return;

            void Process(ReadOnlySpan2D<float> data)
            {
                for (var i = 0; i < data.Height; i++)
                    callback(data.GetRowSpan(i));
            }
        }

        /// <summary>
        /// Single vectorisation
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public float[] Vectorise(ICanRandomlyAccessData data)
        {
            var ret = new float[OutputSize];
            var span = ret.AsSpan();
            uint index = 0;

            var indices = SourceColumnIndices ?? stackalloc uint[(int)data.Size];
            if (SourceColumnIndices is null)
            {
                for (int i = 0; i < data.Size; i++)
                    indices[i] = (uint)i;
            }

            for (int i = 0, len = Vectorisers.Length; i < len; i++)
            {
                var vectoriser = Vectorisers[i];
                vectoriser.Vectorise(data[indices[i]], span.Slice((int)index, (int)vectoriser.OutputSize));
                index += vectoriser.OutputSize;
            }
            return ret;
        }

        /// <summary>
        /// Single vectorisation
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public float[] Vectorise(params object[] values) => Vectorise(new ArrayWrapper(values));

        /// <summary>
        /// Vectorise the buffers as an enumerable
        /// </summary>
        /// <param name="buffers"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<float[,]> Vectorise(IReadOnlyBufferWithMetaData[] buffers)
        {
            await foreach (var item in DoVectorise(buffers))
                yield return item;

            for (int i = 0, len = buffers.Length; i < len; i++)
                Vectorisers[i].WriteTo(buffers[i].MetaData);
        }

        /// <summary>
        /// Vectorise the data table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnIndices">List of column indices to vectorise (optional)</param>
        /// <returns></returns>
        public IAsyncEnumerable<float[,]> Vectorise(IDataTable table, params uint[] columnIndices) => Vectorise(table.GetColumns(table.AllOrSpecifiedColumnIndices(false, columnIndices)));

        async IAsyncEnumerable<float[,]> DoVectorise<T>(T[] buffers) where T : IReadOnlyBuffer
        {
            if (buffers.Length != Vectorisers.Length)
                throw new ArgumentException($"Expected to receive {Vectorisers.Length} buffers (not {buffers.Length})", nameof(buffers));
            var first = buffers[0];
            if (buffers.Skip(1).Any(x => x.Size != first.Size || x.BlockSize != first.BlockSize))
                throw new ArgumentException("Expected all buffers to have the same size and block size", nameof(buffers));

            var len = Vectorisers.Length;
            var tasks = new Task[len];

            for (uint i = 0; i < first.BlockCount; i++)
            {
                var blockSize = i + 1 == first.BlockCount
                    ? first.Size - first.BlockSize * i
                    : first.BlockSize;
                var buffer = new float[blockSize, OutputSize];
                uint offset = 0;
                var index = 0;
                for (var j = 0; j < len; j++)
                {
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
