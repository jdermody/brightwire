using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData
{
    /// <summary>
    /// Builds data tables
    /// </summary>
    public class DataTableBuilder : IHaveColumns
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
        readonly List<Column> _column = new List<Column>();
        readonly List<DataTableRow> _data = new List<DataTableRow>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataTableBuilder()
        {
        }

        /// <summary>
        /// Creates a data table builder with the initial list of columns
        /// </summary>
        /// <param name="columns">The initial columns</param>
        public DataTableBuilder(IEnumerable<IColumn> columns)
        {
            foreach (var column in columns) {
                var col = AddColumn(column.Type, column.Name, column.IsTarget);
                col.IsContinuous = column.IsContinuous;
            }
        }

        /// <summary>
        /// The current list of columns
        /// </summary>
        public IReadOnlyList<IColumn> Columns { get { return _column; } }
        internal IReadOnlyList<Column> Columns2 { get { return _column; } }
        
        /// <summary>
        /// The number of rows
        /// </summary>
        public int RowCount { get { return _data.Count; } }

        /// <summary>
        /// The number of columns
        /// </summary>
        public int ColumnCount { get { return _column.Count; } }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="column">The column type</param>
        /// <param name="name">The column name</param>
        /// <param name="isTarget">True if the column is a classification target</param>
        public IColumn AddColumn(ColumnType column, string name = "", bool isTarget = false)
        {
            var ret = new Column(column, name, isTarget);
            _column.Add(ret);
            return ret;
        }

        /// <summary>
        /// Adds a new row
        /// </summary>
        /// <param name="data">The data in the row</param>
        public IRow AddRow(IEnumerable<object> data)
        {
            var row = new DataTableRow(this, data);
            _data.Add(row);

            var data2 = row.Data;
            for(int i = 0, len = data2.Count; i < len && i < _column.Count; i++)
                _column[i].Add(data2[i]);
            return row;
        }

        /// <summary>
        /// Adds a new row
        /// </summary>
        /// <param name="data">The data in the new row</param>
        public IRow Add(params object[] data)
        {
            return AddRow(data);
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

        internal int WriteData(Stream stream)
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

        internal void Process(IRowProcessor rowProcessor)
        {
            foreach (var item in _data) {
                if (!rowProcessor.Process(item))
                    break;
            }
        }

        //public void Process(Func<IRow, int, bool> processor)
        //{
        //    int index = 0;
        //    foreach (var item in _data) {
        //        if (!processor(item, index++))
        //            break;
        //    }
        //}

        /// <summary>
        /// Creates a data table
        /// </summary>
        /// <param name="output">Optional stream to write the data table to</param>
        public IDataTable Build(Stream output = null)
        {
            var writer = new DataTableWriter(Columns, output);
            Process(writer);
            return writer.GetDataTable();
        }

        /// <summary>
        /// Clears the current list of rows
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }
    }
}
