using BrightWire.TabularData.Analysis;
using BrightWire.TabularData.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TabularData
{
    public class DataTable : IDataTable
    {
        class Column : IColumn
        {
            readonly ColumnType _type;
            readonly string _name;
            int _numDistinct;
            bool? _isContinuous;

            public Column(string name, ColumnType type, int numDistinct, bool? isContinuous)
            {
                _name = name;
                _type = type;
                _numDistinct = numDistinct;
                _isContinuous = isContinuous;
            }
            public ColumnType Type { get { return _type; } }
            public string Name { get { return _name; } }
            public int NumDistinct
            {
                get { return _numDistinct; }
                set { _numDistinct = value; }
            }
            public bool IsContinuous
            {
                get { return _isContinuous.HasValue ? _isContinuous.Value : ColumnTypeClassifier.IsContinuous(this); }
                set { _isContinuous = value; }
            }
        }

        readonly List<Column> _column = new List<Column>();
        IDataTableAnalysis _analysis = null;
        readonly long _dataOffset;
        readonly protected Stream _stream;

        public DataTable(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var columnCount = reader.ReadInt32();
            for (var i = 0; i < columnCount; i++) {
                var name = reader.ReadString();
                var type = (ColumnType)reader.ReadByte();
                var numDistinct = reader.ReadInt32();
                var continuousSpecified = reader.ReadBoolean();
                bool? isContinuous = null;
                if (continuousSpecified)
                    isContinuous = reader.ReadBoolean();
                _column.Add(new Column(name, type, numDistinct, isContinuous));
            }

            _stream = stream;
            _dataOffset = stream.Position;
        }

        public IReadOnlyList<IColumn> Columns { get { return _column; } }

        public virtual int RowCount
        {
            get
            {
                return -1;
            }
        }
        public int ColumnCount { get { return _column.Count; } }

        object _ReadColumn(Column column, BinaryReader reader)
        {
            var type = column.Type;
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
                case ColumnType.Byte:
                    return reader.ReadByte();
                //case ColumnType.CategoryList:
                //    return Enumerable.Range(0, reader.ReadInt32()).Select(i => reader.ReadInt32()).ToArray();
                //case ColumnType.WeightedCategoryList:
                //    return Enumerable.Range(0, reader.ReadInt32()).Select(i => Tuple.Create(reader.ReadInt32(), reader.ReadDouble())).ToArray();
                default:
                    return null;
            }
        }

        protected object[] _ReadRow(BinaryReader reader)
        {
            var row = new object[_column.Count];
            for (var j = 0; j < _column.Count; j++)
                row[j] = _ReadColumn(_column[j], reader);
            return row;
        }

        protected void _SkipRow(BinaryReader reader)
        {
            for (var j = 0; j < _column.Count; j++)
                _ReadColumn(_column[j], reader);
        }

        public void Process(IRowProcessor rowProcessor)
        {
            _Iterate(row => rowProcessor.Process(row));
        }

        protected void _Iterate(Func<DataTableRow, bool> callback)
        {
            lock (_stream) {
                _stream.Seek(_dataOffset, SeekOrigin.Begin);
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                while (_stream.Position < _stream.Length) {
                    var row = new DataTableRow(this, _ReadRow(reader));
                    if (!callback(row))
                        break;
                }
            }
        }

        public IIndexableDataTable Index(Stream output = null)
        {
            var writer = new DataTableWriter(Columns, output);
            _Iterate(row => {
                writer.AddRow(row.Data);
                return true;
            });
            return writer.GetIndexedTable();
        }

        public IDataTableAnalysis Analysis
        {
            get
            {
                if (_analysis == null) {
                    var analysis = new DataTableAnalysis(this);
                    Process(analysis);
                    _analysis = analysis;
                }
                return _analysis;
            }
        }
    }
}
