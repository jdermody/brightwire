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
        readonly Dictionary<int, ColumnNormalisation> _columnInfo = new Dictionary<int, ColumnNormalisation>();
        readonly DataTableWriter _writer;
        readonly int _columnCount;
        readonly NormalisationType _normalisation;
        readonly DataTable _table;

        class ColumnNormalisation
        {
            readonly ColumnType _type;
            readonly double _subtract, _divide;

            public ColumnNormalisation(ColumnType type, double divide, double subtract = 0.0)
            {
                _type = type;
                _subtract = subtract;
                _divide = divide;
            }

            public ColumnType ColumnType { get { return _type; } }

            public double Normalise(double val)
            {
                var ret = (val - _subtract) / _divide;

                switch(_type) {
                    case ColumnType.Float:
                        return Convert.ToSingle(ret);
                    case ColumnType.Long:
                        return Convert.ToInt64(ret);
                    case ColumnType.Int:
                        return Convert.ToInt32(ret);
                    case ColumnType.Byte:
                        return Convert.ToByte(ret);
                    default:
                        return ret;
                }
            }

            public double Subtract { get { return _subtract; } }
            public double Divide { get { return _divide; } }
        }

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

                        ColumnNormalisation columnNorm;
                        if (_normalisation == NormalisationType.Standard)
                            columnNorm = new ColumnNormalisation(column.Type, numericInfo.StdDev.Value, numericInfo.Mean);
                        else if (_normalisation == NormalisationType.Euclidean)
                            columnNorm = new ColumnNormalisation(column.Type, numericInfo.L2Norm);
                        else if (_normalisation == NormalisationType.Manhattan)
                            columnNorm = new ColumnNormalisation(column.Type, numericInfo.L1Norm);
                        else if (_normalisation == NormalisationType.FeatureScale)
                            columnNorm = new ColumnNormalisation(column.Type, numericInfo.Max - numericInfo.Mean, numericInfo.Min);
                        else
                            throw new NotImplementedException();
                        _columnInfo.Add(columnInfo.ColumnIndex, columnNorm);
                    }
                }
            }
        }

        public IReadOnlyList<object> Normalise(IReadOnlyList<object> row)
        {
            ColumnNormalisation norm;
            var ret = new object[_columnCount];
            for (var i = 0; i < _columnCount; i++) {
                object obj = null;
                if (_columnInfo.TryGetValue(i, out norm)) {
                    double val;
                    switch(norm.ColumnType) {
                        case ColumnType.Byte:
                            val = (byte)row[i];
                            break;
                        case ColumnType.Double:
                            val = (double)row[i];
                            break;
                        case ColumnType.Float:
                            val = (float)row[i];
                            break;
                        case ColumnType.Int:
                            val = (int)row[i];
                            break;
                        case ColumnType.Long:
                            val = (long)row[i];
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    obj = norm.Normalise(val);
                }
                else
                    obj = row[i];
                ret[i] = obj;
            }
            return ret;
        }

        public bool Process(IRow row)
        {
            ColumnNormalisation norm;
            var data = new object[_columnCount];
            for (var i = 0; i < _columnCount; i++) {
                object obj = null;
                if (_columnInfo.TryGetValue(i, out norm))
                    obj = norm.Normalise(row.GetField<double>(i));
                else
                    obj = row.Data[i];
                data[i] = obj;
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
