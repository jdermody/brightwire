using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra.ReadOnly;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify each step of a sequence
    /// </summary>
    internal class SequentialDataTableAdapter : TypedRowBasedDataTableAdapterBase<ReadOnlyMatrix<float>, ReadOnlyMatrix<float>>
    {
        readonly uint[] _rowDepth;
        readonly bool _sequenceLengthsAreVaried = false;

	    public SequentialDataTableAdapter(IDataTable dataTable, uint[] featureColumns, bool sequenceLengthsAreVaried = false) 
            : base(dataTable, featureColumns)
        {
            if (_featureColumnIndices.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");

            if (dataTable.MetaData.Get("Seq2Seq", false))
                sequenceLengthsAreVaried = true;
            _rowDepth = new uint[dataTable.RowCount];

            // find the number of sequences of each row
            var foundData = false;
            foreach(var row in _buffer.EnumerateAllTyped().ToBlockingEnumerable()) {
                var inputMatrix = row.C1;
                var outputMatrix = row.C2;
                _rowDepth[row.RowIndex] = inputMatrix.RowCount;
                if (outputMatrix.RowCount != inputMatrix.RowCount)
                    sequenceLengthsAreVaried = true;
                if (!foundData) {
                    InputSize = inputMatrix.ColumnCount;
                    OutputSize = outputMatrix.ColumnCount;
                    foundData = true;
                }
            }
            if (!foundData)
                throw new Exception("No data found");

            _sequenceLengthsAreVaried = sequenceLengthsAreVaried;
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new SequentialDataTableAdapter(dataTable, _featureColumns, _sequenceLengthsAreVaried);
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }
	    public override uint RowCount => (uint)_rowDepth.Length;

        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            var inputData = new Dictionary<uint, List<IReadOnlyNumericSegment<float>>>();
            var outputData = new Dictionary<uint, List<IReadOnlyNumericSegment<float>>>();

            if (_sequenceLengthsAreVaried) {
                await foreach (var row in GetRows(rows)) {
                    var input = row.C1;
                    var output = row.C2;
                    for (uint i = 0, len = input.RowCount; i < len; i++) {
                        if (!inputData.TryGetValue(i, out var temp))
                            inputData.Add(i, temp = []);
                        temp.Add(input.GetReadOnlyRow(i));
                    }

                    for (uint i = 0, len = output.RowCount; i < len; i++) {
                        if (!outputData.TryGetValue(i, out var temp))
                            outputData.Add(i, temp = []);
                        temp.Add(output.GetReadOnlyRow(i));
                    }
                }

                var encoderMiniBatch = new MiniBatch(rows, this);
                foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                    var input = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                    var type = item.Key == 0
                        ? MiniBatchSequenceType.SequenceStart
                        : item.Key == (inputData.Count - 1)
                            ? MiniBatchSequenceType.SequenceEnd
                            : MiniBatchSequenceType.Standard
                    ;
                    encoderMiniBatch.Add(type, input.AsGraphData(), null);
                }

                var decoderMiniBatch = new MiniBatch(rows, this);
                foreach (var item in outputData.OrderBy(kv => kv.Key)) {
                    var output = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                    var type = item.Key == 0
                        ? MiniBatchSequenceType.SequenceStart
                        : item.Key == (outputData.Count - 1)
                            ? MiniBatchSequenceType.SequenceEnd
                            : MiniBatchSequenceType.Standard
                    ;
                    decoderMiniBatch.Add(type, null, output.AsGraphData());
                }

                encoderMiniBatch.NextMiniBatch = decoderMiniBatch;
                decoderMiniBatch.PreviousMiniBatch = encoderMiniBatch;
                return encoderMiniBatch;
            }

            await foreach (var row in GetRows(rows)) {
                var input = row.C1;
                var output = row.C2;
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out var temp))
                        inputData.Add(i, temp = []);
                    temp.Add(input.GetReadOnlyRow(i));

                    if (output != null) {
                        if (!outputData.TryGetValue(i, out temp))
                            outputData.Add(i, temp = []);
                        temp.Add(output.GetReadOnlyRow(i));
                    }
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(item.Value));
                IGraphData? output = null;
                if (outputData.TryGetValue(item.Key, out var outputList))
                    output = lap.CreateMatrixFromRows(CollectionsMarshal.AsSpan(outputList)).AsGraphData();
                var type = (item.Key == 0)
                        ? MiniBatchSequenceType.SequenceStart
                        : item.Key == (inputData.Count - 1)
                            ? MiniBatchSequenceType.SequenceEnd
                            : MiniBatchSequenceType.Standard
                    ;
                miniBatch.Add(type, input.AsGraphData(), output);
            }
            return miniBatch;
        }

        public override uint[][] GetSequentialBatches()
        {
            return _rowDepth
                .Select((r, i) => (Row: r, Index: i))
                .GroupBy(t => t.Row)
                .Select(g => g.Select(d => (uint)d.Index).ToArray())
                .ToArray()
            ;
        }
    }
}
