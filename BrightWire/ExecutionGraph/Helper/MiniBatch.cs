using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Helper
{
	/// <inheritdoc />
	public class MiniBatch : IMiniBatch
    {
	    /// <inheritdoc />
	    public class Sequence : IMiniBatchSequence
        {
	        /// <inheritdoc />
	        public IMiniBatch MiniBatch { get; set; }
	        /// <inheritdoc />
            public uint SequenceIndex { get; set; }
	        /// <inheritdoc />
            public MiniBatchSequenceType Type { get; set; }
	        /// <inheritdoc />
            public IReadOnlyList<IGraphData> Input { get; set; }
	        /// <inheritdoc />
            public IGraphData Target { get; set; }
        }
        readonly List<Sequence> _sequence = new List<Sequence>();
	    int _index = 0;

		/// <summary>
		/// Creates a non sequential mini batch
		/// </summary>
		/// <param name="rows">The indices of the rows in this mini batch</param>
		/// <param name="dataSource">Associated data source</param>
		/// <param name="input">Mini batch input data</param>
		/// <param name="output">Expected output data (when training, otherwise null)</param>
        public MiniBatch(IReadOnlyList<uint> rows, IDataSource dataSource, IReadOnlyList<IGraphData> input, IGraphData output) : this(rows, dataSource)
        {
            IsSequential = false;
            _sequence.Add(new Sequence {
                Input = input,
                Target = output,
                SequenceIndex = 0,
                Type = MiniBatchSequenceType.Standard,
                MiniBatch = this
            });
        }

		/// <summary>
		/// Creates a sequential mini batch
		/// </summary>
		/// <param name="rows">The indices of the rows in this mini batch</param>
		/// <param name="dataSource">Associated data source</param>
        public MiniBatch(IReadOnlyList<uint> rows, IDataSource dataSource)
        {
            Rows = rows;
            IsSequential = true;
            DataSource = dataSource;
        }

		/// <summary>
		/// Adds another item to the sequential mini batch
		/// </summary>
		/// <param name="type">Type of the sequential item</param>
		/// <param name="input">Mini batch input data</param>
		/// <param name="output">Expected output data (when training, otherwise null)</param>
        public void Add(MiniBatchSequenceType type, IReadOnlyList<IGraphData> input, IGraphData output)
        {
            _sequence.Add(new Sequence {
                Input = input,
                Target = output,
                SequenceIndex = (uint)_sequence.Count,
                Type = type,
                MiniBatch = this
            });
        }

	    /// <inheritdoc />
	    public IReadOnlyList<uint> Rows { get; }
	    /// <inheritdoc />
	    public IDataSource DataSource { get; }
	    /// <inheritdoc />
	    public bool IsSequential { get; }
	    /// <inheritdoc />
	    public uint BatchSize => (uint)Rows.Count;
	    /// <inheritdoc />
        public IMiniBatchSequence CurrentSequence => _sequence[_index];
	    /// <inheritdoc />
        public bool HasNextSequence => _index < _sequence.Count;
	    /// <inheritdoc />
        public uint SequenceCount => (uint)_sequence.Count;
	    /// <inheritdoc />
        public void Reset() => _index = 0;

	    /// <inheritdoc />
        public IMiniBatchSequence GetNextSequence()
        {
            if(HasNextSequence)
                return _sequence[_index++];
            return null;
        }

	    /// <inheritdoc />
        public IMiniBatchSequence GetSequenceAtIndex(uint index)
        {
            return _sequence[(int)index];
        }
    }
}
