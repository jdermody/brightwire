using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables that classify each step of a sequence
    /// </summary>
    internal class SequentialDataTableAdapter : DataTableAdapterBase<(Matrix<float> Input, Matrix<float>? Output)>
    {
        readonly uint[] _featureColumns;
        readonly uint[] _rowDepth;
        readonly bool _sequenceLengthsAreVaried = false;

	    public SequentialDataTableAdapter(IRowOrientedDataTable dataTable, uint[] featureColumns, bool sequenceLengthsAreVaried = false) 
            : base(dataTable, featureColumns)
        {
            if (_featureColumnIndices.Length > 1)
                throw new NotImplementedException("Sequential datasets not supported with more than one input data column");
            _featureColumns = featureColumns;

            _rowDepth = new uint[dataTable.RowCount];

            // find the number of sequences of each row
            Matrix<float>? inputMatrix = null, outputMatrix = null;
            dataTable.ForEachRow((row, i) => {
                inputMatrix = (Matrix<float>) row[_featureColumnIndices[0]];
                outputMatrix = (Matrix<float>) row[_targetColumnIndex];
                _rowDepth[i] = inputMatrix.RowCount;
                if (outputMatrix.RowCount != inputMatrix.RowCount)
                    sequenceLengthsAreVaried = true;
            });
            if (inputMatrix == null || outputMatrix == null)
                throw new Exception("No data found");

            _sequenceLengthsAreVaried = sequenceLengthsAreVaried;
            InputSize = inputMatrix.ColumnCount;
            OutputSize = outputMatrix.ColumnCount;
        }

        protected override IEnumerable<(Matrix<float> Input, Matrix<float>? Output)> GetRows(uint[] rows)
        {
            return _dataTable.Rows(rows).Select(row => ((Matrix<float>) row[_featureColumnIndices[0]], (Matrix<float>?) row[_targetColumnIndex]));
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new SequentialDataTableAdapter(dataTable, _featureColumns, _sequenceLengthsAreVaried);
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }
	    public override uint RowCount => (uint)_rowDepth.Length;

        public override IMiniBatch Get(uint[] rows)
        {
            var lap = _dataTable.Context.LinearAlgebraProvider;
            if (_sequenceLengthsAreVaried) {
                var inputData = new Dictionary<uint, List<Vector<float>>>();
                var outputData = new Dictionary<uint, List<Vector<float>>>();

                foreach (var (input, output) in GetRows(rows)) {
                    for (uint i = 0, len = input.RowCount; i < len; i++) {
                        if (!inputData.TryGetValue(i, out var temp))
                            inputData.Add(i, temp = new List<Vector<float>>());
                        temp.Add(input.Row(i));
                    }

                    if (output != null) {
                        for (uint i = 0, len = output.RowCount; i < len; i++) {
                            if (!outputData.TryGetValue(i, out var temp))
                                outputData.Add(i, temp = new List<Vector<float>>());
                            temp.Add(output.Row(i));
                        }
                    }
                }

                var encoderMiniBatch = new MiniBatch(rows, this);
                foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                    var input = lap.CreateMatrixFromRows(item.Value);
                    var type = item.Key == 0
                        ? MiniBatchSequenceType.SequenceStart
                        : item.Key == (inputData.Count - 1)
                            ? MiniBatchSequenceType.SequenceEnd
                            : MiniBatchSequenceType.Standard
                    ;
                    encoderMiniBatch.Add(type, new MatrixGraphData(input), null);
                }

                var decoderMiniBatch = new MiniBatch(rows, this);
                foreach (var item in outputData.OrderBy(kv => kv.Key)) {
                    var output = lap.CreateMatrixFromRows(item.Value);
                    var type = item.Key == 0
                        ? MiniBatchSequenceType.SequenceStart
                        : item.Key == (outputData.Count - 1)
                            ? MiniBatchSequenceType.SequenceEnd
                            : MiniBatchSequenceType.Standard
                    ;
                    decoderMiniBatch.Add(type, null, new MatrixGraphData(output));
                }

                encoderMiniBatch.NextMiniBatch = decoderMiniBatch;
                decoderMiniBatch.PreviousMiniBatch = encoderMiniBatch;
                return encoderMiniBatch;
            }
            return GetSequentialMiniBatch(rows, GetRows(rows).ToArray());
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
