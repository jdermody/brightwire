using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables that classify a sequence into a single classification
    /// </summary>
    class ManyToOneDataTableAdaptor : RowBasedDataTableAdaptorBase
    {
        readonly uint[] _rowDepth;
        private readonly uint _outputSize;

	    public ManyToOneDataTableAdaptor(ILinearAlgebraProvider lap, IRowOrientedDataTable dataTable) 
            : base(lap, dataTable)
        {
            if (_dataColumnIndex.Length > 1)
                throw new NotImplementedException("Sequential data sets not supported with more than one input data column");

            _rowDepth = new uint[dataTable.RowCount];
            Matrix<float> inputMatrix = null;
            Vector<float> outputVector = null;
            dataTable.ForEachRow((row, i) => {
                inputMatrix = (Matrix<float>)row[_dataColumnIndex[0]];
                outputVector = (Vector<float>)row[_dataTargetIndex];
                _rowDepth[i] = inputMatrix.RowCount;
                if (inputMatrix.ColumnCount != outputVector.Size)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            });

            InputSize = inputMatrix.ColumnCount;
            OutputSize = _outputSize = outputVector.Size;
        }

        public override IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            return new ManyToOneDataTableAdaptor(_lap, dataTable);
        }

        public override bool IsSequential => true;
        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

	    public override uint[][] GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => (uint)d.Item2).ToArray())
                .ToArray()
            ;
        }

        public override IMiniBatch Get(IExecutionContext executionContext, uint[] rows)
        {
            var data = _GetRows(rows)
                .Select(r => ((Matrix<float>)r[_dataColumnIndex[0]], (Vector<float>)r[_dataTargetIndex]))
                .ToList()
            ;
            var inputData = new Dictionary<uint, List<Vector<float>>>();
            foreach (var item in data) {
                var input = item.Item1;
                for (uint i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out List<Vector<float>> temp))
                        inputData.Add(i, temp = new List<Vector<float>>());
                    temp.Add(input.Row(i));
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            var outputVector = _lap.CreateMatrix((uint)data.Count, _outputSize, (x, y) => data[(int)x].Item2.Segment[y]);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = _lap.CreateMatrixFromRows(item.Value);
                var type = (item.Key == 0)
                    ? MiniBatchSequenceType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchSequenceType.SequenceEnd
                        : MiniBatchSequenceType.Standard
                ;
                var inputList = new IGraphData[] {
                    new MatrixGraphData(input)
                };
                miniBatch.Add(type, inputList, type == MiniBatchSequenceType.SequenceEnd ? new MatrixGraphData(outputVector) : null);
            }
            return miniBatch;
        }
    }
}
