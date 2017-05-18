using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.ExecutionGraph.Helper
{
    class MiniBatch : IMiniBatch
    {
        public class MiniBatchSequence : IMiniBatchSequence
        {
            public IMiniBatch MiniBatch { get; set; }
            public int SequenceIndex { get; set; }
            public MiniBatchType Type { get; set; }
            public IMatrix Input { get; set; }
            public IMatrix Target { get; set; }
        }
        readonly List<MiniBatchSequence> _sequence = new List<MiniBatchSequence>();
        readonly IReadOnlyList<int> _rows;
        readonly IDataSource _dataSource;
        readonly bool _isSequential;
        int _index = 0;

        public MiniBatch(IReadOnlyList<int> rows, IDataSource dataSource, IMatrix input, IMatrix output) : this(rows, dataSource)
        {
            _isSequential = false;
            _sequence.Add(new MiniBatchSequence {
                Input = input,
                Target = output,
                SequenceIndex = 0,
                Type = MiniBatchType.Standard,
                MiniBatch = this
            });
        }

        public MiniBatch(IReadOnlyList<int> rows, IDataSource dataSource)
        {
            _rows = rows;
            _isSequential = true;
            _dataSource = dataSource;
        }

        public void Add(MiniBatchType type, IMatrix input, IMatrix output)
        {
            _sequence.Add(new MiniBatchSequence {
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
