using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single row instead of the normal batch mode
    /// </summary>
    class SingleRowDataSource : IDataSource
    {
        class Sequence : IMiniBatchSequence
        {
	        public Sequence(IReadOnlyList<IGraphData> data, IMiniBatch miniBatch, MiniBatchSequenceType sequenceType, int sequenceIndex)
            {
                Input = data;
                MiniBatch = miniBatch;
                Type = sequenceType;
                SequenceIndex = sequenceIndex;
            }
            public IMiniBatch MiniBatch { get; }
	        public int SequenceIndex { get; }
	        public MiniBatchSequenceType Type { get; }
	        public IReadOnlyList<IGraphData> Input { get; }
	        public IGraphData Target => null;
        }
        class SingleRowMiniBatch : IMiniBatch
        {
	        int _index = 0;

            public SingleRowMiniBatch(IDataSource dataSource, IGraphData data, bool isSequential, MiniBatchSequenceType sequenceType, int sequenceIndex)
            {
                CurrentSequence = new Sequence(new[] { data }, this, sequenceType, sequenceIndex);
                DataSource = dataSource;
                IsSequential = isSequential;
            }

            public IReadOnlyList<int> Rows => new[] { 0 };
            public IDataSource DataSource { get; }
	        public bool IsSequential { get; }
	        public int BatchSize => 1;
            public IMiniBatchSequence CurrentSequence { get; }
	        public bool HasNextSequence => false;
            public int SequenceCount => 0;
            public void Reset() => _index = 0;
            public IMiniBatchSequence GetNextSequence()
            {
                if(_index++ == 0)
                    return CurrentSequence;
                return null;
            }
            public IMiniBatchSequence GetSequenceAtIndex(int index)
            {
                throw new NotImplementedException();
            }
        }

        readonly float[] _data;
        readonly int _sequenceIndex;
	    readonly MiniBatchSequenceType _sequenceType;

        public SingleRowDataSource(float[] data, bool isSequential, MiniBatchSequenceType sequenceType, int sequenceIndex)
        {
            _data = data;
            _sequenceIndex = sequenceIndex;
            IsSequential = isSequential;
            _sequenceType = sequenceType;
        }

        public bool IsSequential { get; }
	    public int InputSize => _data.Length;
        public int OutputSize => throw new NotImplementedException();
        public uint RowCount => 1;
        public uint InputCount => 1;

        public IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = executionContext.LinearAlgebraProvider.CreateVector(_data);
            return new SingleRowMiniBatch(this, new MatrixGraphData(data.ReshapeAsRowMatrix()), IsSequential, _sequenceType, _sequenceIndex);
        }

        public IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return new[] {
                new [] {
                    1
                }
            };
        }

        public void OnBatchProcessed(IContext context)
        {
            // nop
        }
    }
}
