using BrightWire.TabularData.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData.Helper
{
    public class DataTableNormaliser : IRowProcessor
    {
        readonly Dictionary<int, INumericColumnInfo> _columnInfo = new Dictionary<int, INumericColumnInfo>();
        readonly DataTableWriter _writer;
        readonly int _columnCount;
        readonly NormalisationType _normalisation;
        readonly DataTable _table;

        public DataTableNormaliser(DataTable dataTable, NormalisationType type, Stream output = null)
        {
            _table = dataTable;
            var analysis = dataTable.Analysis;
            _columnCount = dataTable.ColumnCount;
            _writer = new DataTableWriter(dataTable.Columns, output);
            _normalisation = type;

            foreach (var columnInfo in analysis.ColumnInfo) {
                var column = dataTable.Columns[columnInfo.ColumnIndex];
                if(column.IsContinuous) {
                    var numericInfo = columnInfo as INumericColumnInfo;
                    if (numericInfo != null) {
                        if (_normalisation == NormalisationType.Standard && !numericInfo.StdDev.HasValue)
                            continue;
                        _columnInfo.Add(columnInfo.ColumnIndex, numericInfo);
                    }
                }
            }
        }

        public bool Process(IRow row)
        {
            INumericColumnInfo info;
            var data = new object[_columnCount];
            for (var i = 0; i < _columnCount; i++) {
                object obj = null;
                if (_columnInfo.TryGetValue(i, out info)) {
                    var val = row.GetField<double>(i);
                    if (_normalisation == NormalisationType.Standard)
                        val = (val - info.Mean) / info.StdDev.Value;
                    else if (_normalisation == NormalisationType.Euclidean)
                        val = val / info.L2Norm;
                    else if (_normalisation == NormalisationType.Manhattan)
                        val = val / info.L1Norm;
                    else if (_normalisation == NormalisationType.FeatureScale)
                        val = (val - info.Min) / (info.Max - info.Mean);

                    var type = _table.Columns[i].Type;
                    if (type == ColumnType.Double)
                        obj = val;
                    else if (type == ColumnType.Float)
                        obj = Convert.ToSingle(val);
                    else if (type == ColumnType.Long)
                        obj = Convert.ToInt64(val);
                    else if (type == ColumnType.Int)
                        obj = Convert.ToInt32(val);
                    else if (type == ColumnType.Byte)
                        obj = Convert.ToByte(val);
                }
                data[i] = obj ?? row.Data[i];
            }
            _writer.AddRow(data);
            return true;
        }

        public IIndexableDataTable GetIndexedTable()
        {
            return _writer.GetIndexedTable();
        }
    }
}
