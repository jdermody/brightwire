using System;
using BrightData;
using BrightData.DataTable;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single row instead of the normal batch mode
    /// </summary>
    internal class SingleRowDataSource(float[] data, LinearAlgebraProvider lap, bool isSequential, MiniBatchSequenceType sequenceType, uint sequenceIndex)
        : IDataSource
    {
        class Sequence(IGraphData data, IMiniBatch miniBatch, MiniBatchSequenceType sequenceType, uint sequenceIndex)
            : IMiniBatchSequence
        {
            public IMiniBatch MiniBatch { get; } = miniBatch;
            public uint SequenceIndex { get; } = sequenceIndex;
            public MiniBatchSequenceType Type { get; } = sequenceType;
            public IGraphData Input { get; } = data;
            public IGraphData? Target => null;
            public IGraphContext? GraphContext { get; set; }
        }
        class SingleRowMiniBatch : IMiniBatch
        {
            int _index = 0;

            public SingleRowMiniBatch(IDataSource dataSource, IGraphData data, bool isSequential, MiniBatchSequenceType sequenceType, uint sequenceIndex)
            {
                CurrentSequence = new Sequence(data, this, sequenceType, sequenceIndex);
                DataSource = dataSource;
                IsSequential = isSequential;
            }

            public uint[] Rows { get; } = [0];
            public IDataSource DataSource { get; }
            public bool IsSequential { get; }
            public uint BatchSize => 1;
            public IMiniBatchSequence CurrentSequence { get; }
            public bool HasNextSequence => false;
            public uint SequenceCount => 0;
            public void Reset() => _index = 0;
            public IMiniBatch? NextMiniBatch { get; } = null;
            public IMiniBatch? PreviousMiniBatch { get; } = null;

            public IMiniBatchSequence? GetNextSequence()
            {
                if (_index++ == 0)
                    return CurrentSequence;
                return null;
            }
            public IMiniBatchSequence GetSequenceAtIndex(uint index)
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSequential { get; } = isSequential;
        public uint InputSize => (uint)data.Length;
        public uint? OutputSize => throw new NotImplementedException();
        public uint RowCount => 1;
        public uint InputCount => 1;
        public VectorisationModel? InputVectoriser { get; } = null;
        public VectorisationModel? OutputVectoriser { get; } = null;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(uint[] rows)
        {
            using var data1 = lap.CreateVector(data);
            return new SingleRowMiniBatch(this, data1.Reshape(1, null).AsGraphData(), IsSequential, sequenceType, sequenceIndex);
        }

        public uint[][] GetSequentialBatches()
        {
            return [
                [
                    1
                ]
            ];
        }
    }
}
