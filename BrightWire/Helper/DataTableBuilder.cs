using BrightWire.Models;
using BrightWire.TabularData;
using BrightWire.TabularData.Helper;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BrightWire.Helper
{
    /// <summary>
    /// Builds data tables
    /// </summary>
    internal class DataTableBuilder : IHaveColumns, IDataTableBuilder
    {
        const int MAX_UNIQUE = 131072 * 4;

        internal class Column : IColumn
        {
            public bool? _isContinuous;
            public ColumnType _type;
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

            public int NumDistinct
            {
                get
                {
                    return _uniqueValues.Count == MAX_UNIQUE ? 0 : _uniqueValues.Count;
                }
            }

            public void Add(object value)
            {
                if (value != null) {
                    if (_uniqueValues.Count < MAX_UNIQUE)
                        _uniqueValues.Add(value.ToString());
                }
            }

            public ColumnType Type { get { return _type; } }
            public string Name { get { return _name; } }
            public bool IsTarget { get { return _isTarget; } }

            public bool IsContinuous
            {
                get { return _isContinuous.HasValue && _isContinuous.Value; }
                set { _isContinuous = value; }
            }
        }
        class DeepDataTableBuilder : IDeepDataTableRowBuilder
        {
            readonly DataTableBuilder _builder;
            readonly List<ShallowDataTableRow> _subItem = new List<ShallowDataTableRow>();
            readonly DeepDataTableRow _row;

            public DeepDataTableBuilder(DataTableBuilder builder)
            {
                _builder = builder;
                _row = new DeepDataTableRow(_subItem);
            }

            public IRow AddSubItem(params object[] data)
            {
                var ret = _builder.CreateShallow(data, true);
                _subItem.Add(ret);
                return ret;
            }

            public IRow Row { get { return _row; } }
        }
        readonly List<Column> _column = new List<Column>();
        readonly List<IRow> _data = new List<IRow>();
        readonly RowConverter _rowConverter = new RowConverter();
        readonly Dictionary<int, int> _deepRowDepth = new Dictionary<int, int>();

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
        public IReadOnlyList<IColumn> Columns { get { return _column; } }
        internal IReadOnlyList<Column> Columns2 { get { return _column; } }
        public int RowCount { get { return _data.Count; } }
        public int ColumnCount { get { return _column.Count; } }

        public IColumn AddColumn(ColumnType column, string name = "", bool isTarget = false)
        {
            var ret = new Column(column, name, isTarget);
            _column.Add(ret);
            return ret;
        }

        internal IRow AddRow(IRow row)
        {
            _data.Add(row);

            for (var i = 0; i < row.Depth; i++) {
                var data2 = row.GetData(i);
                for (int j = 0, len = data2.Count; j < len && j < _column.Count; j++)
                    _column[j].Add(data2[j]);
            }
            return row;
        }

        public IRow Add(params object[] data)
        {
            return AddRow(CreateShallow(data, false));
        }

        public ShallowDataTableRow CreateShallow(IReadOnlyList<object> data, bool isSubItem)
        {
            return new ShallowDataTableRow(this, data, _rowConverter, isSubItem);
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
                writer.Write((byte)val);
            else if (type == ColumnType.CategoryList) {
                var data = (CategoryList)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.WeightedCategoryList) {
                var data = (WeightedCategoryList)val;
                data.WriteTo(writer);
            } else if (type == ColumnType.Vector) {
                var data = (FloatArray)val;
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
                int index = 0, depth = row.Depth;

                // write the depth indicator
                if (depth > 1) {
                    writer.Write(true);
                    writer.Write((uint)row.Depth);
                } else
                    writer.Write(false);

                for (var i = 0; i < depth; i++) {
                    var rowData = row.GetData(i);
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

        public IDeepDataTableRowBuilder AddDeepRow()
        {
            var ret = new DeepDataTableBuilder(this);
            AddRow(ret.Row);
            return ret;
        }
    }
}
