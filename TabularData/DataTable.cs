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
    internal class DataTable : IDataTable
    {
        class Column : IColumn
        {
            readonly ColumnType _type;
            readonly string _name;
            bool _isTarget;
            int _numDistinct;
            bool? _isContinuous;

            public Column(string name, ColumnType type, int numDistinct, bool? isContinuous, bool isTarget)
            {
                _name = name;
                _type = type;
                _numDistinct = numDistinct;
                _isContinuous = isContinuous;
                _isTarget = isTarget;
            }
            public ColumnType Type { get { return _type; } }
            public string Name { get { return _name; } }
            public bool IsTarget {
                get { return _isTarget; }
                set { _isTarget = value; }
            }
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
        readonly long _dataOffset;
        readonly protected Stream _stream;
        readonly object _mutex = new object();
        readonly IReadOnlyList<long> _index = new List<long>();
        readonly int _rowCount;

        IDataTableAnalysis _analysis = null;

        public const int BLOCK_SIZE = 1024;

        public DataTable(Stream stream, IReadOnlyList<long> dataIndex, int rowCount) 
        {
            _index = dataIndex;
            _rowCount = rowCount;
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            var columnCount = reader.ReadInt32();
            for (var i = 0; i < columnCount; i++) {
                var name = reader.ReadString();
                var type = (ColumnType)reader.ReadByte();
                var isTarget = reader.ReadBoolean();
                var numDistinct = reader.ReadInt32();
                var continuousSpecified = reader.ReadBoolean();
                bool? isContinuous = null;
                if (continuousSpecified)
                    isContinuous = reader.ReadBoolean();
                _column.Add(new Column(name, type, numDistinct, isContinuous, isTarget));
            }

            _stream = stream;
            _dataOffset = stream.Position;
        }

        public static DataTable Create(Stream dataStream, Stream indexStream)
        {
            var reader = new BinaryReader(indexStream);
            var rowCount = reader.ReadInt32();
            var numIndex = reader.ReadInt32();
            var index = new long[numIndex];
            for (var i = 0; i < numIndex; i++)
                index[i] = reader.ReadInt64();
            return new DataTable(dataStream, index, rowCount);
        }

        public static DataTable Create(Stream dataStream)
        {
            var temp = new DataTable(dataStream, new long[0], 0);
            var index = new List<long>();
            var rowCount = 0;

            index.Add(dataStream.Position);
            var reader = new BinaryReader(dataStream);
            while(dataStream.Position < dataStream.Length) {
                temp._SkipRow(reader);
                if ((++rowCount % BLOCK_SIZE) == 0)
                    index.Add(dataStream.Position);
            }
            dataStream.Seek(0, SeekOrigin.Begin);
            return new DataTable(dataStream, index, rowCount);
        }

        public IReadOnlyList<IColumn> Columns { get { return _column; } }

        public int RowCount
        {
            get
            {
                return _rowCount;
            }
        }
        public int ColumnCount { get { return _column.Count; } }

        public void WriteIndexTo(Stream stream)
        {
            using(var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
                writer.Write(_rowCount);
                writer.Write(_index.Count);
                foreach (var item in _index)
                    writer.Write(item);
            }
        }

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

        //public void Process(Func<IRow, int, bool> processor)
        //{
        //    int index = 0;
        //    _Iterate(row => processor(row, index++));
        //}

        protected void _Iterate(Func<DataTableRow, bool> callback)
        {
            lock (_mutex) {
                _stream.Seek(_dataOffset, SeekOrigin.Begin);
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                while (_stream.Position < _stream.Length) {
                    var row = new DataTableRow(this, _ReadRow(reader));
                    if (!callback(row))
                        break;
                }
            }
        }

        protected void _IterateRaw(Action<object[]> callback)
        {
            lock (_mutex) {
                _stream.Seek(_dataOffset, SeekOrigin.Begin);
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                while (_stream.Position < _stream.Length) {
                    callback(_ReadRow(reader));
                }
            }
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

        public IReadOnlyList<IRow> GetSlice(int offset, int count)
        {
            var ret = new List<IRow>();
            lock (_mutex) {
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                _stream.Seek(_index[offset / BLOCK_SIZE], SeekOrigin.Begin);

                // seek to offset
                for (var i = 0; i < offset % BLOCK_SIZE; i++)
                    _SkipRow(reader);

                // read the data
                for (var i = 0; i < count && _stream.Position < _stream.Length; i++)
                    ret.Add(new DataTableRow(this, _ReadRow(reader)));
            }
            return ret;
        }

        public IRow GetRow(int rowIndex)
        {
            var block = rowIndex / BLOCK_SIZE;
            var offset = rowIndex % BLOCK_SIZE;

            lock (_mutex) {
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                _stream.Seek(_index[block], SeekOrigin.Begin);
                for (var i = 0; i < offset; i++)
                    _SkipRow(reader);
                return new DataTableRow(this, _ReadRow(reader));
            }
        }

        public IReadOnlyList<IRow> GetRows(IEnumerable<int> rowIndex)
        {
            // split the queries into blocks
            int temp;
            Dictionary<int, int> temp2;
            var blockMatch = new Dictionary<int, Dictionary<int, int>>();
            foreach (var row in rowIndex.OrderBy(r => r)) {
                var block = row / BLOCK_SIZE;
                if (!blockMatch.TryGetValue(block, out temp2))
                    blockMatch.Add(block, temp2 = new Dictionary<int, int>());
                if (temp2.TryGetValue(row, out temp))
                    temp2[row] = temp + 1;
                else
                    temp2.Add(row, 1);
            }

            var ret = new List<IRow>();
            lock (_mutex) {
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                foreach (var block in blockMatch.OrderBy(b => b.Key)) {
                    _stream.Seek(_index[block.Key], SeekOrigin.Begin);
                    var match = block.Value;

                    for (int i = block.Key * BLOCK_SIZE, len = i + BLOCK_SIZE; i < len && _stream.Position < _stream.Length; i++) {
                        if (match.TryGetValue(i, out temp)) {
                            var row = new DataTableRow(this, _ReadRow(reader));
                            for (var j = 0; j < temp; j++)
                                ret.Add(row);
                        }
                        else
                            _SkipRow(reader);
                    }
                }
            }
            return ret;
        }

        public Tuple<IDataTable, IDataTable> Split(int? randomSeed = null, double trainPercentage = 0.8, bool shuffle = true, Stream output1 = null, Stream output2 = null)
        {
            var input = Enumerable.Range(0, RowCount);
            if (shuffle)
                input = input.Shuffle(randomSeed);
            var final = input.ToList();
            int trainingCount = Convert.ToInt32(RowCount * trainPercentage);

            var writer1 = new DataTableWriter(Columns, output1);
            foreach (var row in GetRows(final.Take(trainingCount)))
                writer1.Process(row);

            var writer2 = new DataTableWriter(Columns, output2);
            foreach (var row in GetRows(final.Skip(trainingCount)))
                writer2.Process(row);

            return Tuple.Create<IDataTable, IDataTable>(
                writer1.GetDataTable(),
                writer2.GetDataTable()
            );
        }

        public IDataTable Bag(int? count = null, Stream output = null, int? randomSeed = null)
        {
            var input = Enumerable.Range(0, RowCount).ToList().Bag(count ?? RowCount, randomSeed);
            var writer = new DataTableWriter(Columns, output);
            foreach (var row in GetRows(input))
                writer.Process(row);
            return writer.GetDataTable();
        }

        public IEnumerable<Tuple<IDataTable, IDataTable>> Fold(int k, int? randomSeed = null, bool shuffle = true)
        {
            var input = Enumerable.Range(0, RowCount);
            if (shuffle)
                input = input.Shuffle(randomSeed);
            var final = input.ToList();
            var foldSize = final.Count / k;

            for (var i = 0; i < k; i++) {
                var trainingRows = final.Take(i * foldSize).Concat(final.Skip((i + 1) * foldSize));
                var validationRows = final.Skip(i * foldSize).Take(foldSize);

                var writer1 = new DataTableWriter(Columns, null);
                foreach (var row in GetRows(trainingRows))
                    writer1.Process(row);

                var writer2 = new DataTableWriter(Columns, null);
                foreach (var row in GetRows(validationRows))
                    writer2.Process(row);

                yield return Tuple.Create<IDataTable, IDataTable>(
                    writer1.GetDataTable(),
                    writer2.GetDataTable()
                );
            }
        }

        public IReadOnlyList<T> GetColumn<T>(int columnIndex)
        {
            var ret = new T[RowCount];
            int index = 0;

            _Iterate(row => {
                ret[index++] = row.GetField<T>(columnIndex);
                return true;
            });

            return ret;
        }

        //public string[] GetDiscreteColumn(int columnIndex)
        //{
        //    var ret = new string[RowCount];

        //    int index = 0;
        //    _Iterate(row => {
        //        ret[index++] = row.GetField<string>(columnIndex);
        //        return true;
        //    });

        //    return ret;
        //}

        //public float[] GetNumericColumn(int columnIndex)
        //{
        //    var ret = new float[RowCount];

        //    int index = 0;
        //    _Iterate(row => {
        //        ret[index++] = row.GetField<float>(columnIndex);
        //        return true;
        //    });

        //    return ret;
        //}

        public IReadOnlyList<float[]> GetNumericColumns(IEnumerable<int> columns = null)
        {
            var columnTable = (columns ?? Enumerable.Range(0, ColumnCount)).ToDictionary(i => i, i => new float[RowCount]);

            int index = 0;
            _Iterate(row => {
                foreach (var item in columnTable)
                    item.Value[index] = row.GetField<float>(item.Key);
                ++index;
                return true;
            });

            return columnTable.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
        }

        public IReadOnlyList<IVector> GetNumericColumns(ILinearAlgebraProvider lap, IEnumerable<int> columns = null)
        {
            var columnTable = (columns ?? Enumerable.Range(0, ColumnCount)).ToDictionary(i => i, i => new float[RowCount]);

            int index = 0;
            _Iterate(row => {
                foreach (var item in columnTable)
                    item.Value[index] = row.GetField<float>(item.Key);
                ++index;
                return true;
            });

            return columnTable.OrderBy(kv => kv.Key).Select(kv => lap.Create(kv.Value)).ToList();
        }

        public IReadOnlyList<float[]> GetNumericRows(IEnumerable<int> columns = null)
        {
            var columnList = new List<int>(columns ?? Enumerable.Range(0, ColumnCount));

            var ret = new List<float[]>();
            _Iterate(row => {
                int index = 0;
                var buffer = new float[columnList.Count];
                foreach (var item in columnList)
                    buffer[index++] = row.GetField<float>(item);
                ret.Add(buffer);
                return true;
            });

            return ret;
        }

        public IReadOnlyList<IVector> GetNumericRows(ILinearAlgebraProvider lap, IEnumerable<int> columns = null)
        {
            var columnList = new List<int>(columns ?? Enumerable.Range(0, ColumnCount));

            var ret = new List<IVector>();
            _Iterate(row => {
                int index = 0;
                var buffer = new float[columnList.Count];
                foreach (var item in columnList)
                    buffer[index++] = row.GetField<float>(item);
                ret.Add(lap.Create(buffer));
                return true;
            });

            return ret;
        }

        public IDataTable Normalise(NormalisationType normalisationType, Stream output = null)
        {
            var normaliser = new DataTableNormaliser(this, normalisationType, output);
            Process(normaliser);
            return normaliser.GetDataTable();
        }

        public IRow this[int index]
        {
            get
            {
                return GetRows(new[] { index }).FirstOrDefault();
            }
        }

        public int TargetColumnIndex
        {
            get
            {
                for (int i = 0, len = _column.Count; i < len; i++) {
                    if (_column[i].IsTarget)
                        return i;
                }
                // default to the last column
                return _column.Count-1;
            }
            set
            {
                for (int i = 0, len = _column.Count; i < len; i++)
                    _column[i].IsTarget = (i == value);
            }
        }

        public IReadOnlyList<Tuple<IRow, string>> Classify(IRowClassifier classifier)
        {
            var ret = new List<Tuple<IRow, string>>();
            _Iterate(row => {
                ret.Add(Tuple.Create<IRow, string>(row, classifier.Classify(row).First()));
                return true;
            });
            return ret;
        }

        public IDataTable SelectColumns(IEnumerable<int> columns, Stream output = null)
        {
            return DataTableProjector.Project(this, columns, output);
        }

        public IDataTable Project(Func<IReadOnlyList<object>, IReadOnlyList<object>> mutator, Stream output = null)
        {
            var isFirst = true;
            DataTableWriter writer = new DataTableWriter(output);
            _IterateRaw(row => {
                var row2 = mutator(row);
                if (row2 != null) {
                    if (isFirst) {
                        int index = 0;
                        foreach(var item in row2) {
                            var column = Columns[index];
                            if (item == null)
                                writer.AddColumn(column.Name, ColumnType.Null, column.IsTarget);
                            else {
                                var type = item.GetType();
                                ColumnType columnType;
                                if (type == typeof(string))
                                    columnType = ColumnType.String;
                                else if (type == typeof(double))
                                    columnType = ColumnType.Double;
                                else if (type == typeof(float))
                                    columnType = ColumnType.Float;
                                else if (type == typeof(long))
                                    columnType = ColumnType.Long;
                                else if (type == typeof(int))
                                    columnType = ColumnType.Int;
                                else if (type == typeof(byte))
                                    columnType = ColumnType.Byte;
                                else if (type == typeof(DateTime))
                                    columnType = ColumnType.Date;
                                else if (type == typeof(bool))
                                    columnType = ColumnType.Boolean;
                                else
                                    throw new FormatException();
                                writer.AddColumn(column.Name, columnType, column.IsTarget);
                            }
                            ++index;
                        }
                        isFirst = false;
                    }
                    writer.AddRow(row2);
                }
            });
            return writer.GetDataTable();
        }
    }
}
