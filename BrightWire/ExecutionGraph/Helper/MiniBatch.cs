using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Information about the current mini batch
    /// </summary>
	public class MiniBatch : IMiniBatch
    {
        /// <summary>
        /// A sequence within a mini batch
        /// </summary>
	    internal class Sequence : IMiniBatchSequence
        {
            public IMiniBatch MiniBatch { get; }
            public uint SequenceIndex { get; }
            public MiniBatchSequenceType Type { get; }
            public IGraphData? Input { get; }
            public IGraphData? Target { get; }
            public IGraphContext? GraphContext { get; set; }

            public Sequence(IGraphData? input, IGraphData? target, MiniBatch miniBatch, uint sequenceIndex = 0, MiniBatchSequenceType type = MiniBatchSequenceType.Standard)
            {
                Input = input;
                Target = target;
                MiniBatch = miniBatch;
                SequenceIndex = sequenceIndex;
                Type = type;
            }

            /// <inheritdoc />
            public override string ToString() => $"{SequenceIndex} - {Type}";
        }
        readonly List<Sequence> _sequence = new();
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

        /// <summary>
        /// Row indexes of the current batch
        /// </summary>
	    public uint[] Rows { get; }


        /// <summary>
        /// Data source
        /// </summary>
	    public IDataSource DataSource { get; }

        /// <summary>
        /// True if the data is sequential
        /// </summary>
	    public bool IsSequential { get; }

        /// <summary>
        /// Number of items in the batch
        /// </summary>
	    public uint BatchSize => (uint)Rows.Length;

        /// <summary>
        /// Current sequence (non sequential batches have a single sequence)
        /// </summary>
        public IMiniBatchSequence CurrentSequence => _sequence[_index];

        /// <summary>
        /// True if there is another item in the sequence after the current item
        /// </summary>
        public bool HasNextSequence => _index < _sequence.Count;

        /// <summary>
        /// Gets the length of the sequence
        /// </summary>
        public uint SequenceCount => (uint)_sequence.Count;

        /// <summary>
        /// Resets the sequence iterator
        /// </summary>
        public void Reset() => _index = 0;

        /// <summary>
        /// Gets the next item in the sequence (or null if none)
        /// </summary>
        public IMiniBatchSequence? GetNextSequence()
        {
            if(HasNextSequence)
                return _sequence[_index++];
            return null;
        }

        /// <summary>
        /// Gets a sequence item
        /// </summary>
        /// <param name="index">The index to retrieve</param>
        public IMiniBatchSequence GetSequenceAtIndex(uint index)
        {
            return _sequence[(int)index];
        }

        /// <summary>
        /// Subsequent mini batch
        /// </summary>
        public IMiniBatch? NextMiniBatch { get; set; } = null;

        /// <summary>
        /// Previous mini batch
        /// </summary>
        public IMiniBatch? PreviousMiniBatch { get; set; } = null;
    }
}
