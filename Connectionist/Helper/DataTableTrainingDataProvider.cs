using BrightWire.TabularData.Helper;
using BrightWire.Helper;
using BrightWire.TabularData.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Connectionist.Helper
{
    internal class DataTableTrainingDataProvider : ITrainingDataProvider
    {
        readonly IDataTableVectoriser _vectoriser;
        readonly ILinearAlgebraProvider _lap;
        readonly IDataTable _table;
        readonly int _inputSize, _outputSize;

        public DataTableTrainingDataProvider(ILinearAlgebraProvider lap, IDataTable table, IDataTableVectoriser vectoriser)
        {
            _lap = lap;
            _table = table;
            _vectoriser = vectoriser;
            _inputSize = _vectoriser.InputSize;
            _outputSize = _vectoriser.OutputSize;
        }

        public int Count
        {
            get
            {
                return _table.RowCount;
            }
        }

        public int InputSize
        {
            get
            {
                return _inputSize;
            }
        }

        public int OutputSize
        {
            get
            {
                return _outputSize;
            }
        }

        Tuple<float[], float[]> _Convert(IRow row)
        {
            return Tuple.Create(_vectoriser.GetInput(row), _vectoriser.GetOutput(row));
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var batchRow = _table.GetRows(rows).Select(r => _Convert(r)).ToList();
            var input = _lap.Create(batchRow.Count, _inputSize, (x, y) => batchRow[x].Item1[y]);
            var output = _lap.Create(batchRow.Count, _outputSize, (x, y) => batchRow[x].Item2[y]);
            return new MiniBatch(input, output);
        }

        public string GetOutputLabel(int columnIndex, int vectorIndex)
        {
            return _vectoriser.GetOutputLabel(columnIndex, vectorIndex);
        }

        public void StartEpoch()
        {
            // nop
        }
    }
}
