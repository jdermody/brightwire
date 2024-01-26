using System.Collections.Generic;

namespace BrightWire.ExecutionGraph.Helper
{
    /// <summary>
    /// Information about the current mini batch
    /// </summary>
    /// <remarks>
    /// Creates a sequential mini batch
    /// </remarks>
    /// <param name="rows">The indices of the rows in this mini batch</param>
    /// <param name="dataSource">Associated data source</param>
    public class MiniBatch(uint[] rows, IDataSource dataSource)
    {
        /// <summary>
        /// A sequence within a mini batch
        /// </summary>
	    public class Sequence(IGraphData? input, IGraphData? target, MiniBatch miniBatch, uint sequenceIndex = 0, MiniBatchSequenceType type = MiniBatchSequenceType.Standard)
        {
            /// <summary>
            /// Mini batch
            /// </summary>
            public MiniBatch MiniBatch { get; } = miniBatch;

            /// <summary>
            /// Index of the sequence
            /// </summary>
            public uint SequenceIndex { get; } = sequenceIndex;

            /// <summary>
            /// Sequence type
            /// </summary>
            public MiniBatchSequenceType Type { get; } = type;

            /// <summary>
            /// Input data
            /// </summary>
            public IGraphData? Input { get; } = input;

            /// <summary>
            /// Training target data
            /// </summary>
            public IGraphData? Target { get; } = target;

            /// <summary>
            /// Graph sequence context that has been executed for this sequence
            /// </summary>
            public IGraphContext? GraphContext { get; set; }

            /// <inheritdoc />
            public override string ToString() => $"{SequenceIndex} - {Type}";
        }
        readonly List<Sequence> _sequence = [];
	    int _index = 0;

        /// <summary>
        /// Creates a non sequential mini batch
        /// </summary>
        /// <param name="rows">The indices of the rows in this mini batch</param>
        /// <param name="dataSource">Associated data source</param>
        /// <param name="input">Mini batch input data</param>
        /// <param name="output">Expected output data (when training, otherwise null)</param>
        /// <param name="isSequential"></param>
        /// <param name="type"></param>
        /// <param name="sequenceIndex"></param>
        public MiniBatch(uint[] rows, IDataSource dataSource, IGraphData input, IGraphData? output, bool isSequential = false, MiniBatchSequenceType type = MiniBatchSequenceType.Standard, uint sequenceIndex = 0) : this(rows, dataSource)
        {
            IsSequential = isSequential;
            _sequence.Add(new Sequence(input, output, this, sequenceIndex, type));
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
	    public uint[] Rows { get; } = rows;


        /// <summary>
        /// Data source
        /// </summary>
	    public IDataSource DataSource { get; } = dataSource;

        /// <summary>
        /// True if the data is sequential
        /// </summary>
	    public bool IsSequential { get; } = true;

        /// <summary>
        /// Number of items in the batch
        /// </summary>
	    public uint BatchSize => (uint)Rows.Length;

        /// <summary>
        /// Current sequence (non sequential batches have a single sequence)
        /// </summary>
        public Sequence CurrentSequence => _sequence[_index];

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
        public Sequence? GetNextSequence()
        {
            if(HasNextSequence)
                return _sequence[_index++];
            return null;
        }

        /// <summary>
        /// Gets a sequence item
        /// </summary>
        /// <param name="index">The index to retrieve</param>
        public Sequence GetSequenceAtIndex(uint index)
        {
            return _sequence[(int)index];
        }

        /// <summary>
        /// Subsequent mini batch
        /// </summary>
        public MiniBatch? NextMiniBatch { get; set; } = null;

        /// <summary>
        /// Previous mini batch
        /// </summary>
        public MiniBatch? PreviousMiniBatch { get; set; } = null;
    }
}
