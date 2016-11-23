using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.TabularData.Helper
{
    internal class DataTableVectoriser : IDataTableVectoriser
    {
        readonly IReadOnlyList<IColumn> _column;
        readonly int _inputSize, _outputSize, _classColumnIndex;
        readonly Dictionary<int, Dictionary<string, int>> _columnMap = new Dictionary<int, Dictionary<string, int>>();
        readonly Dictionary<int, Dictionary<int, string>> _reverseColumnMap = new Dictionary<int, Dictionary<int, string>>();

        public DataTableVectoriser(IDataTable table, bool useTargetColumnIndex)
        {
            _column = table.Columns;
            _classColumnIndex = useTargetColumnIndex ? table.TargetColumnIndex : -1;
            var analysis = table.GetAnalysis();

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

        public string GetOutputLabel(int columnIndex, int vectorIndex)
        {
            Dictionary<int, string> map;
            if (_reverseColumnMap.TryGetValue(columnIndex, out map)) {
                string ret;
                if (map.TryGetValue(vectorIndex, out ret))
                    return ret;
            }
            return null;
        }

        public float[] GetInput(IRow row)
        {
            var ret = new float[_inputSize];
            var index = 0;

            for (int i = 0, len = _column.Count; i < len; i++) {
                var column = _column[i];
                if (i == _classColumnIndex)
                    continue;

                if (column.IsContinuous)
                    ret[index++] = row.GetField<float>(i);
                else {
                    var str = row.GetField<string>(i);
                    var offset = index + _columnMap[i][str];
                    index += column.NumDistinct;
                    ret[offset] = 1f;
                }
            }
            return ret;
        }

        public float[] GetOutput(IRow row)
        {
            var ret = new float[_outputSize];
            var column = _column[_classColumnIndex];
            var str = row.GetField<string>(_classColumnIndex);
            var offset = _columnMap[_classColumnIndex][str];
            ret[offset] = 1f;
            return ret;
        }
    }
}
