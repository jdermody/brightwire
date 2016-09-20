using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData
{
    public class MutableDataTable : IDataTable
    {
        const int MAX_UNIQUE = 131072 * 4;

        public class Column : IColumn
        {
            public bool? _isContinuous;
            public ColumnType _type;
            readonly HashSet<string> _uniqueValues = new HashSet<string>();
            readonly string _name;

            public Column(ColumnType type, string name)
            {
                _type = type;
                _name = name;
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
                if (_uniqueValues.Count < MAX_UNIQUE)
                    _uniqueValues.Add(value.ToString());
            }

            public ColumnType Type { get { return _type; } }
            public string Name { get { return _name; } }

            public bool IsContinuous
            {
                get { return _isContinuous.HasValue && _isContinuous.Value; }
                set { _isContinuous = value; }
            }
        }
        readonly List<Column> _column = new List<Column>();
        readonly List<DataTableRow> _data = new List<DataTableRow>();

        public MutableDataTable()
        {
        }
        public MutableDataTable(IEnumerable<IColumn> columns)
        {
            foreach (var column in columns) {
                var col = Add(column.Type, column.Name);
                col.IsContinuous = column.IsContinuous;
            }
        }

        public IReadOnlyList<IColumn> Columns { get { return _column; } }
        public IReadOnlyList<Column> Columns2 { get { return _column; } }
        public int RowCount { get { return _data.Count; } }
        public int ColumnCount { get { return _column.Count; } }

        public Column Add(ColumnType column, string name = "")
        {
            var ret = new Column(column, name);
            _column.Add(ret);
            return ret;
        }

        public IRow AddRow(IEnumerable<object> data)
        {
            var row = new DataTableRow(this, data);
            _data.Add(row);

            var data2 = row.Data;
            for(int i = 0, len = data2.Count; i < len && i < _column.Count; i++)
                _column[i].Add(data2[i]);
            return row;
        }

        public void WriteMetadata(Stream stream)
        {
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(_column.Count);
            foreach (var column in _column) {
                writer.Write(column.Name);
                writer.Write((byte)column.Type);
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
            //else if (type == ColumnType.CategoryList) {
            //    var data = (int[])val;
            //    writer.Write(data.Length);
            //    foreach (var item in data) {
            //        writer.Write(item);
            //    }
            //}
            //else if (type == ColumnType.WeightedCategoryList) {
            //    var data = (Tuple<int, double>[])val;
            //    writer.Write(data.Length);
            //    foreach (var item in data) {
            //        writer.Write(item.Item1);
            //        writer.Write(item.Item2);
            //    }
            //}
        }

        public int WriteData(Stream stream)
        {
            int ret = 0;
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach (var row in _data) {
                var index = 0;
                var rowData = row.Data;
                foreach (var column in _column) {
                    var val = rowData[index++];
                    _WriteValueTo(writer, column, val);
                }
                ++ret;
            }
            return ret;
        }

        public void Process(IRowProcessor rowProcessor)
        {
            foreach (var item in _data) {
                if (!rowProcessor.Process(item))
                    break;
            }
        }

        public IIndexableDataTable Index(Stream output = null)
        {
            var destination = output ?? new MemoryStream();
            var writer = new DataTableWriter(Columns);
            Process(writer);
            return writer.GetIndexedTable(destination);
        }

        public void Clear()
        {
            _data.Clear();
        }
    }
}
