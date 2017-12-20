using BrightWire.Models;
using BrightWire.TabularData;
using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BrightWire.Helper
{
    /// <summary>
    /// Builds data tables
    /// </summary>
    internal class DataTableBuilder : IHaveColumns
    {
        const int MAX_UNIQUE = 131072 * 4;

        internal class Column : IColumn
        {
	        internal bool? _isContinuous;
            internal ColumnType _type;
            readonly HashSet<string> _uniqueValues = new HashSet<string>();
            readonly string _name;
            readonly bool _isTarget;

            public Column(ColumnType type, string name, bool isTarget)
            {
                _type = type;
                _name = name;
                _isTarget = isTarget;
                if (type == ColumnType.Double || type == ColumnType.Float)
                    _isContinuous = true;
            }

            public int NumDistinct => _uniqueValues.Count == MAX_UNIQUE ? 0 : _uniqueValues.Count;

	        public void Add(object value)
            {
                if (value != null) {
                    // TODO: validate the type against ColumnType?
                    if (_uniqueValues.Count < MAX_UNIQUE)
                        _uniqueValues.Add(value.ToString());
                }
            }

            public ColumnType Type => _type;
	        public string Name => _name;
	        public bool IsTarget => _isTarget;

	        public bool IsContinuous
            {
                get => _isContinuous.HasValue && _isContinuous.Value;
		        set { _isContinuous = value; }
            }
        }
        readonly List<Column> _column = new List<Column>();
        readonly List<IRow> _data = new List<IRow>();
        readonly RowConverter _rowConverter = new RowConverter();

        public DataTableBuilder()
        {
        }

        public DataTableBuilder(IEnumerable<IColumn> columns)
        {
            foreach (var column in columns) {
                var col = AddColumn(column.Type, column.Name, column.IsTarget);
                col.IsContinuous = column.IsContinuous;
            }
        }

        public static DataTableBuilder CreateTwoColumnMatrix()
        {
            var ret = new DataTableBuilder();
            ret.AddColumn(ColumnType.Matrix, "Input");
            ret.AddColumn(ColumnType.Matrix, "Output", true);
            return ret;
        }

        public IReadOnlyList<IColumn> Columns => _column;
	    internal IReadOnlyList<Column> Columns2 => _column;
	    public int RowCount => _data.Count;
	    public int ColumnCount => _column.Count;

	    public IColumn AddColumn(ColumnType column, string name = "", bool isTarget = false)
        {
            var ret = new Column(column, name, isTarget);
            _column.Add(ret);
            return ret;
        }

        internal IRow AddRow(IRow row)
        {
            _data.Add(row);

            var data = row.Data;
            for (int j = 0, len = data.Count; j < len && j < _column.Count; j++)
                _column[j].Add(data[j]);
            return row;
        }

        public IRow Add(params object[] data)
        {
            return AddRow(CreateRow(data));
        }

        public DataTableRow CreateRow(IReadOnlyList<object> data)
        {
            return new DataTableRow(this, data, _rowConverter);
        }

        internal void WriteMetadata(Stream stream)
        {
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(_column.Count);
            foreach (var column in _column) {
                writer.Write(column.Name);
                writer.Write((byte)column.Type);
                writer.Write(column.IsTarget);
                writer.Write(column.NumDistinct);
                writer.Write(column._isContinuous.HasValue);
                if (column._isContinuous.HasValue)
                    writer.Write(column._isContinuous.Value);
            }
        }

        void _WriteValueTo(BinaryWriter writer, Column column, object val)
        {
            var type = column.Type;

            if (type == ColumnType.Date)
                writer.Write(((DateTime)val).Ticks);
            else if (type == ColumnType.Boolean)
                writer.Write((bool)val);
            else if (type == ColumnType.Double)
                writer.Write((double)val);
            else if (type == ColumnType.Float)
                writer.Write((float)val);
            else if (type == ColumnType.Int)
                writer.Write((int)val);
            else if (type == ColumnType.Long)
                writer.Write((long)val);
            else if (type == ColumnType.String)
                writer.Write((string)val);
            else if (type == ColumnType.Byte)
                writer.Write((sbyte)val);
            else if (type == ColumnType.IndexList) {
                var data = (IndexList)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.WeightedIndexList) {
                var data = (WeightedIndexList)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.Vector) {
                var data = (FloatVector)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.Matrix) {
                var data = (FloatMatrix)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.Tensor) {
                var data = (FloatTensor)val;
                data.WriteTo(writer);
            }
        }

        internal int WriteData(Stream stream)
        {
            int ret = 0;
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach (var row in _data) {
                int index = 0;
                var rowData = row.Data;
                foreach (var column in _column) {
                    if (index < rowData.Count) {
                        var val = rowData[index++];
                        _WriteValueTo(writer, column, val);
                    } else {
                        // if the value is missing then write the column's default value instead
                        object val = null;
                        var ct = column.Type;
                        if (ct == ColumnType.String)
                            val = "";
                        else if (ct == ColumnType.Date)
                            val = DateTime.MinValue;
                        else if (ct != ColumnType.Null) {
                            var columnType = ct.GetColumnType();
                            if (columnType.GetTypeInfo().IsValueType)
                                val = Activator.CreateInstance(columnType);
                        }
                        _WriteValueTo(writer, column, val);
                    }
                }
                ++ret;
            }
            return ret;
        }

        internal void Process(IRowProcessor rowProcessor)
        {
            foreach (var item in _data) {
                if (!rowProcessor.Process(item))
                    break;
            }
        }

        public IDataTable Build(Stream output = null)
        {
            var writer = new DataTableWriter(Columns, output);
            Process(writer);
            return writer.GetDataTable();
        }

        public void ClearRows()
        {
            _data.Clear();
        }
    }
}
