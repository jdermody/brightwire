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
        readonly IDataTable _table;
        readonly int _inputSize, _outputSize, _classColumnIndex;
        readonly ILinearAlgebraProvider _lap;
        readonly Dictionary<int, Dictionary<string, int>> _columnMap = new Dictionary<int, Dictionary<string, int>>();
        readonly Dictionary<int, Dictionary<int, string>> _reverseColumnMap = new Dictionary<int, Dictionary<int, string>>();

        public DataTableTrainingDataProvider(ILinearAlgebraProvider lap, IDataTable table)
        {
            _lap = lap;
            _table = table;
            _classColumnIndex = _table.TargetColumnIndex;
            var analysis = table.Analysis;

            foreach (var columnInfo in analysis.ColumnInfo) {
                var column = table.Columns[columnInfo.ColumnIndex];
                int size = 0;
                if (column.IsContinuous || !columnInfo.NumDistinct.HasValue)
                    size = 1;
                else {
                    size = columnInfo.NumDistinct.Value;
                    var categoryIndex = columnInfo.DistinctValues.Select(s => s.ToString()).OrderBy(s => s).Select((s, i) => Tuple.Create(s, i)).ToList();
                    var columnMap = categoryIndex.ToDictionary(d => d.Item1, d => d.Item2);
                    var reverseColumnMap = categoryIndex.ToDictionary(d => d.Item2, d => d.Item1);
                    _columnMap.Add(columnInfo.ColumnIndex, columnMap);
                    _reverseColumnMap.Add(columnInfo.ColumnIndex, reverseColumnMap);
                }
                if (columnInfo.ColumnIndex == _classColumnIndex)
                    _outputSize = size;
                else
                    _inputSize += size;
            }
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
            var input = new float[_inputSize];
            var output = new float[_outputSize];
            var index = 0;

            for (int i = 0, len = _table.ColumnCount; i < len; i++) {
                var column = _table.Columns[i];
                var isClassColumn = (i == _classColumnIndex);
                float[] ptr = isClassColumn ? output : input;

                if (column.IsContinuous)
                    ptr[index++] = row.GetField<float>(i);
                else {
                    var str = row.GetField<string>(i);
                    var offset = _columnMap[i][str];
                    if (!isClassColumn) {
                        offset += index;
                        index += column.NumDistinct;
                    }
                    ptr[offset] = 1f;
                }
            }
            return Tuple.Create(input, output);
        }

        public IMiniBatch GetTrainingData(IReadOnlyList<int> rows)
        {
            var batchRow = _table.GetRows(rows).Select(r => _Convert(r)).ToList();
            var input = _lap.Create(batchRow.Count, _inputSize, (x, y) => batchRow[x].Item1[y]);
            var output = _lap.Create(batchRow.Count, _outputSize, (x, y) => batchRow[x].Item2[y]);
            return new MiniBatch(input, output);
        }
    }
}
