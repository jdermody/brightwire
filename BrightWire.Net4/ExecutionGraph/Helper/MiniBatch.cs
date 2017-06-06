using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Information about the current mini batch
    /// </summary>
    class MiniBatch : IMiniBatch
    {
        public class Sequence : IMiniBatchSequence
        {
            public IMiniBatch MiniBatch { get; set; }
            public int SequenceIndex { get; set; }
            public MiniBatchSequenceType Type { get; set; }
            public IReadOnlyList<IGraphData> Input { get; set; }
            public IGraphData Target { get; set; }
        }
        readonly List<Sequence> _sequence = new List<Sequence>();
        readonly IReadOnlyList<int> _rows;
        readonly IDataSource _dataSource;
        readonly bool _isSequential;
        int _index = 0;

        public MiniBatch(IReadOnlyList<int> rows, IDataSource dataSource, IReadOnlyList<IGraphData> input, IGraphData output) : this(rows, dataSource)
        {
            _isSequential = false;
            _sequence.Add(new Sequence {
                Input = input,
                Target = output,
                SequenceIndex = 0,
                Type = MiniBatchSequenceType.Standard,
                MiniBatch = this
            });
        }

        public MiniBatch(IReadOnlyList<int> rows, IDataSource dataSource)
        {
            _rows = rows;
            _isSequential = true;
            _dataSource = dataSource;
        }

        public void Add(MiniBatchSequenceType type, IReadOnlyList<IGraphData> input, IGraphData output)
        {
            _sequence.Add(new Sequence {
                Input = input,
                Target = output,
                SequenceIndex = _sequence.Count,
                Type = type,
                MiniBatch = this
            });
        }

        public IReadOnlyList<int> Rows => _rows;
        public IDataSource DataSource => _dataSource;
        public bool IsSequential => _isSequential;
        public int BatchSize => _rows.Count;
        public IMiniBatchSequence CurrentSequence => _sequence[_index];
        public bool HasNextSequence => _index < _sequence.Count;
        public int SequenceCount => _sequence.Count;

        public IMiniBatchSequence GetNextSequence()
        {
            if(HasNextSequence)
                return _sequence[_index++];
            return null;
        }

        public IMiniBatchSequence GetSequenceAtIndex(int index)
        {
            return _sequence[index];
        }
    }
}
