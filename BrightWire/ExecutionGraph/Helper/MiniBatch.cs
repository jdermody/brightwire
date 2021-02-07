using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Helper
{
	/// <inheritdoc />
	public class MiniBatch : IMiniBatch
    {
	    /// <inheritdoc />
	    internal class Sequence : IMiniBatchSequence
        {
	        /// <inheritdoc />
	        public IMiniBatch MiniBatch { get; }
	        /// <inheritdoc />
            public uint SequenceIndex { get; }
	        /// <inheritdoc />
            public MiniBatchSequenceType Type { get; }
	        /// <inheritdoc />
            public IGraphData? Input { get; }
	        /// <inheritdoc />
            public IGraphData? Target { get; }

            public Sequence(IGraphData? input, IGraphData? target, IMiniBatch miniBatch, uint sequenceIndex = 0, MiniBatchSequenceType type = MiniBatchSequenceType.Standard)
            {
                Input = input;
                Target = target;
                MiniBatch = miniBatch;
                SequenceIndex = sequenceIndex;
                Type = type;
            }

            public override string ToString() => $"{SequenceIndex} - {Type}";
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
        public MiniBatch(uint[] rows, IDataSource dataSource, IGraphData input, IGraphData? output) : this(rows, dataSource)
        {
            IsSequential = false;
            _sequence.Add(new Sequence(input, output, this));
        }

		/// <summary>
		/// Creates a sequential mini batch
		/// </summary>
		/// <param name="rows">The indices of the rows in this mini batch</param>
		/// <param name="dataSource">Associated data source</param>
        public MiniBatch(uint[] rows, IDataSource dataSource)
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
        public void Add(MiniBatchSequenceType type, IGraphData? input, IGraphData? output)
        {
            _sequence.Add(new Sequence(input, output, this, (uint) _sequence.Count, type));
        }

	    /// <inheritdoc />
	    public uint[] Rows { get; }
	    /// <inheritdoc />
	    public IDataSource DataSource { get; }
	    /// <inheritdoc />
	    public bool IsSequential { get; }
	    /// <inheritdoc />
	    public uint BatchSize => (uint)Rows.Length;
	    /// <inheritdoc />
        public IMiniBatchSequence CurrentSequence => _sequence[_index];
	    /// <inheritdoc />
        public bool HasNextSequence => _index < _sequence.Count;
	    /// <inheritdoc />
        public uint SequenceCount => (uint)_sequence.Count;
	    /// <inheritdoc />
        public void Reset() => _index = 0;

	    /// <inheritdoc />
        public IMiniBatchSequence? GetNextSequence()
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

        public IMiniBatch? NextMiniBatch { get; set; } = null;
    }
}
