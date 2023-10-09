using System;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single row instead of the normal batch mode
    /// </summary>
    internal class SingleRowDataSource : IDataSource
    {
        class Sequence : IMiniBatchSequence
        {
            public Sequence(IGraphData data, IMiniBatch miniBatch, MiniBatchSequenceType sequenceType, uint sequenceIndex)
            {
                Input = data;
                MiniBatch = miniBatch;
                Type = sequenceType;
                SequenceIndex = sequenceIndex;
            }
            public IMiniBatch MiniBatch { get; }
            public uint SequenceIndex { get; }
            public MiniBatchSequenceType Type { get; }
            public IGraphData Input { get; }
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

            public uint[] Rows { get; } = { 0 };
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

        readonly float[] _data;
        readonly LinearAlgebraProvider _lap;
        readonly uint _sequenceIndex;
        readonly MiniBatchSequenceType _sequenceType;

        public SingleRowDataSource(float[] data, LinearAlgebraProvider lap, bool isSequential, MiniBatchSequenceType sequenceType, uint sequenceIndex)
        {
            _data = data;
            _lap = lap;
            _sequenceIndex = sequenceIndex;
            IsSequential = isSequential;
            _sequenceType = sequenceType;
        }

        public bool IsSequential { get; }
        public uint InputSize => (uint)_data.Length;
        public uint? OutputSize => throw new NotImplementedException();
        public uint RowCount => 1;
        public uint InputCount => 1;
        public IDataTableVectoriser? InputVectoriser { get; } = null;
        public IDataTableVectoriser? OutputVectoriser { get; } = null;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(uint[] rows)
        {
            using var data = _lap.CreateVector(_data);
            return new SingleRowMiniBatch(this, data.Reshape(1, null).AsGraphData(), IsSequential, _sequenceType, _sequenceIndex);
        }

        public uint[][] GetSequentialBatches()
        {
            return new[] {
                new uint [] {
                    1
                }
            };
        }
    }
}
