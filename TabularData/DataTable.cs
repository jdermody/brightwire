using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.TabularData
{
    public class DataTable
    {
        public class Column
        {
            public ColumnType Type { get; private set; }

            public Column(ColumnType type)
            {
                Type = type;
            }

            public string TypeName
            {
                get
                {
                    switch (Type) {
                        case ColumnType.Boolean:
                            return "boolean";
                        case ColumnType.String:
                            return "string";
                        case ColumnType.Double:
                            return "double";
                        case ColumnType.Float:
                            return "float";
                        case ColumnType.Int:
                            return "int";
                        case ColumnType.Date:
                        case ColumnType.Long:
                            return "long";
                        case ColumnType.CategoryList:
                            return "category-list";
                        case ColumnType.WeightedCategoryList:
                            return "weighted-category-list";
                        default:
                            return "null";
                    }
                }
            }
        }
        class Row : IRow
        {
            readonly DataTable _table;
            readonly List<object> _data;

            public Row(DataTable table, IEnumerable<object> data)
            {
                _table = table;
                _data = new List<object>(data);
            }

            public IReadOnlyList<object> Data { get { return _data; } }
            public T GetField<T>(int index)
            {
                var ret = _data[index];
                var targetType = typeof(T);
                if (ret.GetType() == targetType)
                    return (T)ret;
                return (T)Convert.ChangeType(ret, targetType);
            }
            public IEnumerable<float> GetNumericFields(IEnumerable<int> fields)
            {
                return fields.Select(i => Convert.ToSingle(_data[i]));
            }
            public ColumnType GetType(int index)
            {
                if (index >= 0 && index < _table.ColumnTypes.Count)
                    return _table.ColumnTypes[index].Type;
                return ColumnType.Null;
            }
        }
        readonly List<Column> _column = new List<Column>();
        readonly List<Row> _data = new List<Row>();
        readonly IReadOnlyList<long> _index = new List<long>();
        readonly string _name;

        public const int BLOCK_SIZE = 1024;

        public DataTable(string name)
        {
            _name = name;
        }

        public string Name { get { return _name; } }
        public IReadOnlyList<Column> ColumnTypes { get { return _column; } }
        public IEnumerable<IRow> Rows { get { return _data; } }
        public IEnumerable<object> GetColumn(int index)
        {
            foreach (var row in _data)
                yield return row.Data[index];
        }

        public void Add(Column column)
        {
            _column.Add(column);
        }

        public void AddRow(IEnumerable<object> data)
        {
            _data.Add(new Row(this, data));
        }

        public int Count { get { return _data.Count; } }

        public void WriteTo(Stream stream)
        {
            // write the meta data
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            writer.Write(_column.Count);
            foreach (var column in _column)
                writer.Write((byte)column.Type);

            WriteDataTo(stream);
        }

        public static void WriteDataTo(BinaryWriter writer, ColumnType type, object val)
        {
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
            else if (type == ColumnType.CategoryList) {
                var data = (int[])val;
                writer.Write(data.Length);
                foreach (var item in data) {
                    writer.Write(item);
                }
            }
            else if (type == ColumnType.WeightedCategoryList) {
                var data = (Tuple<int, double>[])val;
                writer.Write(data.Length);
                foreach (var item in data) {
                    writer.Write(item.Item1);
                    writer.Write(item.Item2);
                }
            }
        }

        public int WriteDataTo(Stream stream)
        {
            int ret = 0;
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            foreach (var row in _data) {
                var index = 0;
                var rowData = row.Data;
                foreach (var column in _column) {
                    var val = rowData[index++];
                    WriteDataTo(writer, column.Type, val);
                }
                ++ret;
            }
            return ret;
        }

        public DataTable(Stream stream, IReadOnlyList<long> dataIndex, int maxRowCount)
        {
            // read the metadata
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var columnCount = reader.ReadInt32();
            for (var i = 0; i < columnCount; i++) {
                var type = (ColumnType)reader.ReadByte();
                Add(new Column(type));
            }

            _index = dataIndex;

            // read the data
            int index = 0;
            while (stream.Position < stream.Length && index < maxRowCount) {
                AddRow(_ReadRow(reader));
                ++index;
            }
        }

        public IEnumerable<IRow> EnumerateRows(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            while (stream.Position < stream.Length) {
                yield return new Row(this, _ReadRow(reader));
            }
        }

        object _ReadColumn(ColumnType type, BinaryReader reader)
        {
            switch (type) {
                case ColumnType.String:
                    return reader.ReadString();
                case ColumnType.Double:
                    return reader.ReadDouble();
                case ColumnType.Int:
                    return reader.ReadInt32();
                case ColumnType.Float:
                    return reader.ReadSingle();
                case ColumnType.Boolean:
                    return reader.ReadBoolean();
                case ColumnType.Date:
                    return new DateTime(reader.ReadInt64());
                case ColumnType.Long:
                    return reader.ReadInt64();
                case ColumnType.CategoryList:
                    return Enumerable.Range(0, reader.ReadInt32()).Select(i => reader.ReadInt32()).ToArray();
                case ColumnType.WeightedCategoryList:
                    return Enumerable.Range(0, reader.ReadInt32()).Select(i => Tuple.Create(reader.ReadInt32(), reader.ReadDouble())).ToArray();
                default:
                    return null;
            }
        }

        object[] _ReadRow(BinaryReader reader)
        {
            var row = new object[_column.Count];
            for (var j = 0; j < _column.Count; j++)
                row[j] = _ReadColumn(_column[j].Type, reader);
            return row;
        }

        void _SkipRow(BinaryReader reader)
        {
            for (var j = 0; j < _column.Count; j++)
                _ReadColumn(_column[j].Type, reader);
        }

        public void LoadData(Stream stream, int offset, int count, int fileSize)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            stream.Seek(_index[offset / BLOCK_SIZE], SeekOrigin.Begin);

            // seek to offset
            for (var i = 0; i < offset % BLOCK_SIZE; i++)
                _SkipRow(reader);

            // read the data
            for (var i = 0; i < count && stream.Position < fileSize && stream.Position < stream.Length; i++)
                AddRow(_ReadRow(reader));
        }

        public void Clear()
        {
            _data.Clear();
        }

        public NumericTable GetNumericColumns(params int[] columns)
        {
            return new NumericTable(Rows.Select(r => r.GetNumericFields(columns).ToArray()).ToList());
        }
    }
}
