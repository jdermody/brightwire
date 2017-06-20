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
            readonly IReadOnlyList<IGraphData> _data;
            readonly IMiniBatch _miniBatch;
            readonly MiniBatchSequenceType _sequenceType;
            readonly int _sequenceIndex;

            public Sequence(IReadOnlyList<IGraphData> data, IMiniBatch miniBatch, MiniBatchSequenceType sequenceType, int sequenceIndex)
            {
                _data = data;
                _miniBatch = miniBatch;
                _sequenceType = sequenceType;
                _sequenceIndex = sequenceIndex;
            }
            public IMiniBatch MiniBatch => _miniBatch;
            public int SequenceIndex => _sequenceIndex;
            public MiniBatchSequenceType Type => _sequenceType;
            public IReadOnlyList<IGraphData> Input => _data;
            public IGraphData Target => null;
        }
        class MiniBatch : IMiniBatch
        {
            readonly IMiniBatchSequence _sequence;
            readonly IDataSource _dataSource;
            readonly bool _isSequential;
            int _index = 0;

            public MiniBatch(IDataSource dataSource, IGraphData data, bool isSequential, MiniBatchSequenceType sequenceType, int sequenceIndex)
            {
                _sequence = new Sequence(new[] { data }, this, sequenceType, sequenceIndex);
                _dataSource = dataSource;
                _isSequential = isSequential;
            }

            public IReadOnlyList<int> Rows => new[] { 0 };
            public IDataSource DataSource => _dataSource;
            public bool IsSequential => _isSequential;
            public int BatchSize => 1;
            public IMiniBatchSequence CurrentSequence => _sequence;
            public bool HasNextSequence => false;
            public int SequenceCount => 0;
            public IMiniBatchSequence GetNextSequence()
            {
                if(_index++ == 0)
                    return _sequence;
                return null;
            }
            public IMiniBatchSequence GetSequenceAtIndex(int index)
            {
                throw new NotImplementedException();
            }
        }

        readonly float[] _data;
        readonly int _sequenceIndex;
        readonly bool _isSequential;
        readonly MiniBatchSequenceType _sequenceType;

        public SingleRowDataSource(float[] data, bool isSequential, MiniBatchSequenceType sequenceType, int sequenceIndex)
        {
            _data = data;
            _sequenceIndex = sequenceIndex;
            _isSequential = isSequential;
            _sequenceType = sequenceType;
        }

        public bool IsSequential => _isSequential;
        public int InputSize => _data.Length;
        public int OutputSize => throw new NotImplementedException();
        public int RowCount => 1;
        public int InputCount => 1;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = executionContext.LinearAlgebraProvider.CreateVector(_data);
            return new MiniBatch(this, new MatrixGraphData(data.ToRowMatrix()), _isSequential, _sequenceType, _sequenceIndex);
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
