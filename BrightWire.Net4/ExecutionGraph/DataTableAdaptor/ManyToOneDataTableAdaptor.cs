using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    class ManyToOneDataTableAdaptor : DataTableAdaptorBase
    {
        readonly int[] _rowDepth;
        readonly int _inputSize, _outputSize;

        public ManyToOneDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable) 
            : base(lap, dataTable)
        {
            _rowDepth = new int[dataTable.RowCount];
            FloatMatrix inputMatrix = null;
            FloatVector outputVector = null;
            dataTable.ForEach((row, i) => {
                inputMatrix = row.GetField<FloatMatrix>(0);
                outputVector = row.GetField<FloatVector>(1);
                _rowDepth[i] = inputMatrix.RowCount;
                if (inputMatrix.ColumnCount != outputVector.Size)
                    throw new ArgumentException("Rows between input and output data tables do not match");
            });

            _inputSize = inputMatrix.ColumnCount;
            _outputSize = outputVector.Size;
        }

        public override IDataSource GetFor(IDataTable dataTable)
        {
            return new ManyToOneDataTableAdaptor(_lap, dataTable);
        }

        public override bool IsSequential => true;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;

        public override IReadOnlyList<IReadOnlyList<int>> GetBuckets()
        {
            return _rowDepth
                .Select((r, i) => (r, i))
                .GroupBy(t => t.Item1)
                .Select(g => g.Select(d => d.Item2).ToList())
                .ToList()
            ;
        }

        public override IMiniBatch Get(IReadOnlyList<int> rows)
        {
            var data = _dataTable
                .GetRows(rows)
                .Select(r => (r.GetField<FloatMatrix>(0), r.GetField<FloatVector>(1)))
                .ToList()
            ;
            List<FloatVector> temp;
            var inputData = new Dictionary<int, List<FloatVector>>();
            foreach (var item in data) {
                var input = item.Item1;
                for (int i = 0, len = input.RowCount; i < len; i++) {
                    if (!inputData.TryGetValue(i, out temp))
                        inputData.Add(i, temp = new List<FloatVector>());
                    temp.Add(input.Row[i]);
                }
            }

            var miniBatch = new MiniBatch(rows, this);
            var outputVector = _lap.CreateMatrix(data.Count, OutputSize, (x, y) => data[x].Item2.Data[y]);
            foreach (var item in inputData.OrderBy(kv => kv.Key)) {
                var input = _lap.CreateMatrix(item.Value);
                var type = (item.Key == 0)
                    ? MiniBatchType.SequenceStart
                    : item.Key == (inputData.Count - 1)
                        ? MiniBatchType.SequenceEnd
                        : MiniBatchType.Standard;

                miniBatch.Add(type, input, type == MiniBatchType.SequenceEnd ? outputVector : null);
            }
            return miniBatch;
        }
    }
}
