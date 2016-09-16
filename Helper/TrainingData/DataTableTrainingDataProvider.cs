using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Helper.TrainingData
{
    public class DataTableTrainingDataProvider : ITrainingDataProvider
    {
        readonly IIndexableDataTable _table;
        readonly int _inputSize, _outputSize;
        readonly ILinearAlgebraProvider _lap;

        public DataTableTrainingDataProvider(ILinearAlgebraProvider lap, IIndexableDataTable table)
        {
            _lap = lap;
            _table = table;
            _inputSize = table.ColumnCount - 1;
            _outputSize = _GetClassificationSize(table.Columns.Last());
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

        float _Get(IReadOnlyList<IRow> batchRow, int x, int y)
        {
            var row = batchRow[x];
            return row.GetField<float>(y);
        }

        int _GetClassificationSize(IColumn column)
        {
            return column.NumDistinct;
        }

        float _GetClassification(IReadOnlyList<IRow> batchRow, int x, int y)
        {
            // TODO: compute per row and cache
            return 0f;
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var batchRow = _table.GetRows(rows);
            var input = _lap.Create(rows.Count, _inputSize, (x, y) => _Get(batchRow, rows[x], y));
            var output = _lap.Create(rows.Count, _outputSize, (x, y) => _GetClassification(batchRow, rows[x], y));
            return new MiniBatch(input, output);
        }
    }
}
