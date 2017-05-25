using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.TabularData.Helper
{
    internal class DataTableVectoriser : IDataTableVectoriser
    {
        readonly IReadOnlyList<IColumn> _column;
        readonly int _inputSize, _outputSize, _classColumnIndex;
        readonly bool _isTargetContinuous, _hasTarget = false;
        readonly Dictionary<int, Dictionary<string, int>> _columnMap = new Dictionary<int, Dictionary<string, int>>();
        readonly Dictionary<int, Dictionary<int, string>> _reverseColumnMap = new Dictionary<int, Dictionary<int, string>>();
        readonly List<string> _columnName = new List<string>();
        readonly IDataTableAnalysis _analysis;

        public DataTableVectoriser(IDataTable table, bool useTargetColumnIndex)
        {
            _column = table.Columns;
            _classColumnIndex = useTargetColumnIndex ? table.TargetColumnIndex : -1;
            _analysis = table.GetAnalysis();

            foreach (var columnInfo in _analysis.ColumnInfo) {
                var column = table.Columns[columnInfo.ColumnIndex];
                var isTarget = columnInfo.ColumnIndex == _classColumnIndex;
                int size = 0;
                var isContinuous = false;

                var indexColumn = columnInfo as IIndexColumnInfo;
                if (indexColumn != null)
                    size = Convert.ToInt32(indexColumn.MaxIndex + 1);
                else {
                    isContinuous = column.IsContinuous || !columnInfo.NumDistinct.HasValue;
                    if (isContinuous) {
                        size = 1;
                        if (!isTarget)
                            _columnName.Add(column.Name);
                    } else {
                        size = columnInfo.NumDistinct.Value;
                        var categoryIndex = columnInfo.DistinctValues.Select(s => s.ToString()).OrderBy(s => s).Select((s, i) => Tuple.Create(s, i)).ToList();
                        var columnMap = categoryIndex.ToDictionary(d => d.Item1, d => d.Item2);
                        var reverseColumnMap = categoryIndex.ToDictionary(d => d.Item2, d => d.Item1);
                        _columnMap.Add(columnInfo.ColumnIndex, columnMap);
                        _reverseColumnMap.Add(columnInfo.ColumnIndex, reverseColumnMap);
                        if (!isTarget) {
                            for (var i = 0; i < size; i++)
                                _columnName.Add(column.Name + ":" + reverseColumnMap[i]);
                        }
                    }
                }
                if (isTarget) {
                    _outputSize = size;
                    _isTargetContinuous = isContinuous;
                    _hasTarget = true;
                } else
                    _inputSize += size;
            }
        }

        public IReadOnlyList<string> ColumnNames => _columnName;
        public IDataTableAnalysis Analysis => _analysis;

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
            if (_hasTarget) {
                Dictionary<int, string> map;
                if (_reverseColumnMap.TryGetValue(columnIndex, out map)) {
                    string ret;
                    if (map.TryGetValue(vectorIndex, out ret))
                        return ret;
                }
            }
            return null;
        }

        public FloatVector GetInput(IRow row)
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
            return new FloatVector {
                Data = ret
            };
        }

        public FloatVector GetOutput(IRow row)
        {
            if (_hasTarget) {
                var ret = new float[_outputSize];
                if (_isTargetContinuous)
                    ret[0] = row.GetField<float>(_classColumnIndex);
                else {
                    var str = row.GetField<string>(_classColumnIndex);
                    var offset = _columnMap[_classColumnIndex][str];
                    ret[offset] = 1f;
                }
                return new FloatVector {
                    Data = ret
                };
            }
            return null;
        }
    }
}
