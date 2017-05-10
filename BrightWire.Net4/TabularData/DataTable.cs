using BrightWire.Helper;
using BrightWire.Models;
using BrightWire.TabularData.Analysis;
using BrightWire.TabularData.Helper;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

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
            public bool IsTarget
            {
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
        readonly RowConverter _rowConverter = new RowConverter();
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
            while (dataStream.Position < dataStream.Length) {
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

        public bool HasCategoricalData
        {
            get
            {
                return _column.Any(c => !c.IsContinuous && !c.IsTarget);
            }
        }

        public void WriteIndexTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
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
                case ColumnType.IndexList:
                    return IndexList.ReadFrom(reader);
                case ColumnType.WeightedIndexList:
                    return WeightedIndexList.ReadFrom(reader);
                case ColumnType.Vector:
                    return FloatVector.ReadFrom(reader);
                case ColumnType.Matrix:
                    return FloatMatrix.ReadFrom(reader);
                case ColumnType.Tensor:
                    return FloatTensor.ReadFrom(reader);
                default:
                    return null;
            }
        }

        protected IRow _ReadDataTableRow(BinaryReader reader)
        {
            object[] Read()
            {
                var row = new object[_column.Count];
                for (var j = 0; j < _column.Count; j++)
                    row[j] = _ReadColumn(_column[j], reader);
                return row;
            }

            return new DataTableRow(this, Read(), _rowConverter);
        }

        protected void _SkipRow(BinaryReader reader)
        {
            for (var j = 0; j < _column.Count; j++)
                _ReadColumn(_column[j], reader);
        }

        public void Process(IRowProcessor rowProcessor)
        {
            _Iterate((row, i) => rowProcessor.Process(row));
        }

        protected void _Iterate(Func<IRow, int, bool> callback)
        {
            lock (_mutex) {
                _stream.Seek(_dataOffset, SeekOrigin.Begin);
                var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                int index = 0;
                while (_stream.Position < _stream.Length) {
                    var row = _ReadDataTableRow(reader);
                    if (!callback(row, index++))
                        break;
                }
            }
        }

        public IDataTableAnalysis GetAnalysis()
        {
            if (_analysis == null) {
                var analysis = new DataTableAnalysis(this);
                Process(analysis);
                _analysis = analysis;
            }
            return _analysis;
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
                    ret.Add(_ReadDataTableRow(reader));
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
                return _ReadDataTableRow(reader);
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
                            var row = _ReadDataTableRow(reader);
                            for (var j = 0; j < temp; j++)
                                ret.Add(row);
                        } else
                            _SkipRow(reader);
                    }
                }
            }
            return ret;
        }

        public (IDataTable Training, IDataTable Test) Split(int? randomSeed = null, double trainPercentage = 0.8, bool shuffle = true, Stream output1 = null, Stream output2 = null)
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

            return (writer1.GetDataTable(), writer2.GetDataTable());
        }

        public IDataTable Bag(int? count = null, Stream output = null, int? randomSeed = null)
        {
            var input = Enumerable.Range(0, RowCount).ToList().Bag(count ?? RowCount, randomSeed);
            var writer = new DataTableWriter(Columns, output);
            foreach (var row in GetRows(input))
                writer.Process(row);
            return writer.GetDataTable();
        }

        public IEnumerable<(IDataTable Training, IDataTable Validation)> Fold(int k, int? randomSeed = null, bool shuffle = true)
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

                yield return (writer1.GetDataTable(), writer2.GetDataTable());
            }
        }

        public IReadOnlyList<T> GetColumn<T>(int columnIndex)
        {
            var ret = new T[RowCount];

            _Iterate((row, i) => {
                ret[i] = row.GetField<T>(columnIndex);
                return true;
            });

            return ret;
        }

        public IReadOnlyList<float[]> GetNumericColumns(IEnumerable<int> columns = null)
        {
            var columnTable = (columns ?? Enumerable.Range(0, ColumnCount)).ToDictionary(i => i, i => new float[RowCount]);

            _Iterate((row, i) => {
                foreach (var item in columnTable)
                    item.Value[i] = row.GetField<float>(item.Key);
                return true;
            });

            return columnTable.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
        }

        public IReadOnlyList<IVector> GetNumericColumns(ILinearAlgebraProvider lap, IEnumerable<int> columns = null)
        {
            var columnTable = (columns ?? Enumerable.Range(0, ColumnCount)).ToDictionary(i => i, i => new float[RowCount]);

            _Iterate((row, i) => {
                foreach (var item in columnTable)
                    item.Value[i] = row.GetField<float>(item.Key);
                return true;
            });

            return columnTable.OrderBy(kv => kv.Key).Select(kv => lap.Create(kv.Value)).ToList();
        }

        public IReadOnlyList<float[]> GetNumericRows(IEnumerable<int> columns = null)
        {
            var columnList = new List<int>(columns ?? Enumerable.Range(0, ColumnCount));

            var ret = new List<float[]>();
            _Iterate((row, i) => {
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
            _Iterate((row, i) => {
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

        public IDataTable Normalise(DataTableNormalisation normalisationModel, Stream output = null)
        {
            var normaliser = new DataTableNormaliser(this, normalisationModel.Type, output, normalisationModel);
            Process(normaliser);
            return normaliser.GetDataTable();
        }

        public DataTableNormalisation GetNormalisationModel(NormalisationType normalisationType)
        {
            var normaliser = new DataTableNormaliser(this, normalisationType);
            return normaliser.GetNormalisationModel();
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
                return _column.Count - 1;
            }
            set
            {
                for (int i = 0, len = _column.Count; i < len; i++)
                    _column[i].IsTarget = (i == value);
            }
        }

        public IReadOnlyList<(IRow Row, string Classification)> Classify(IRowClassifier classifier)
        {
            var ret = new List<(IRow, string)>();
            _Iterate((row, i) => {
                var bestClassification = classifier.Classify(row).OrderByDescending(d => d.Weight).First();
                ret.Add((row, bestClassification.Classification));
                return true;
            });
            return ret;
        }

        public IDataTable SelectColumns(IEnumerable<int> columns, Stream output = null)
        {
            return DataTableProjector.Project(this, columns, output);
        }

        public IDataTable Project(Func<IRow, IReadOnlyList<object>> mutator, Stream output = null)
        {
            var isFirst = true;
            DataTableWriter writer = new DataTableWriter(output);
            _Iterate((row, i) => {
                var mutatedRow = mutator(row);
                if (mutatedRow != null) {
                    if (isFirst) {
                        int index = 0;
                        foreach (var item in mutatedRow) {
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
                                else if (type == typeof(FloatVector))
                                    columnType = ColumnType.Vector;
                                else if (type == typeof(FloatMatrix))
                                    columnType = ColumnType.Matrix;
                                else if (type == typeof(FloatTensor))
                                    columnType = ColumnType.Tensor;
                                else if (type == typeof(WeightedIndexList))
                                    columnType = ColumnType.WeightedIndexList;
                                else if (type == typeof(IndexList))
                                    columnType = ColumnType.IndexList;
                                else
                                    throw new FormatException();

                                writer.AddColumn(column.Name, columnType, column.IsTarget);
                            }
                            ++index;
                        }
                        isFirst = false;
                    }
                    writer.AddRow(new DataTableRow(this, mutatedRow, _rowConverter));
                }
                return true;
            });
            return writer.GetDataTable();
        }

        public IReadOnlyList<(IDataTable Table, string Classification)> ConvertToBinaryClassification()
        {
            return GetAnalysis()[TargetColumnIndex].DistinctValues
                .Cast<string>()
                .Select(cls => (Project(r => {
                    var row = new object[ColumnCount];
                    for (var i = 0; i < ColumnCount; i++) {
                        if (i == TargetColumnIndex)
                            row[i] = r.GetField<string>(i) == cls;
                        else
                            row[i] = r.Data[i];
                    }
                    return row;
                }), cls))
                .ToList()
            ;
        }

        public void ForEach(Action<IRow> callback)
        {
            _Iterate((row, i) => { callback(row); return true; });
        }

        public void ForEach(Action<IRow, int> callback)
        {
            _Iterate((row, i) => { callback(row, i); return true; });
        }

        public void ForEach(Func<IRow, bool> callback)
        {
            _Iterate((row, i) => callback(row));
        }

        public IReadOnlyList<T> Map<T>(Func<IRow, T> mutator)
        {
            var ret = new List<T>();
            _Iterate((row, i) => {
                ret.Add(mutator(row));
                return true;
            });
            return ret;
        }

        public IDataTableVectoriser GetVectoriser(bool useTargetColumnIndex = true)
        {
            return new DataTableVectoriser(this, useTargetColumnIndex);
        }

        public IDataTable CopyWithRows(IEnumerable<int> rowIndex, Stream output = null)
        {
            var writer = new DataTableWriter(_column, output);
            foreach (var row in GetRows(rowIndex))
                writer.AddRow(row);
            return writer.GetDataTable();
        }

        public IDataTable ConvertToNumeric(IDataTableVectoriser vectoriser = null, bool useTargetColumnIndex = true, Stream output = null)
        {
            var writer = new DataTableWriter(output);
            vectoriser = vectoriser ?? GetVectoriser(useTargetColumnIndex);

            // add the numeric columns
            foreach (var name in vectoriser.ColumnNames)
                writer.AddColumn(name, ColumnType.Float);

            // add the classification label column
            var classColumnIndex = TargetColumnIndex;
            if (useTargetColumnIndex) {
                var classColumn = _column[classColumnIndex];
                writer.AddColumn(classColumn.Name, ColumnType.String, true);
            }

            // vectorise each row
            _Iterate((row, i) => {
                var rowData = vectoriser.GetInput(row).AsEnumerable().Cast<object>();
                if (useTargetColumnIndex)
                    rowData = rowData.Concat(new object[] { row.GetField<string>(classColumnIndex) });

                writer.AddRow(new DataTableRow(this, rowData.ToArray(), _rowConverter));
                return true;
            });
            return writer.GetDataTable();
        }

        public string XmlPreview
        {
            get
            {
                var rows = GetRows(Enumerable.Range(0, Math.Min(20, RowCount)));
                var ret = new StringBuilder();
                using (var writer = XmlWriter.Create(new StringWriter(ret))) {
                    writer.WriteStartElement("table");
                    writer.WriteAttributeString("row-count", RowCount.ToString());
                    foreach (var column in Columns) {
                        writer.WriteStartElement("column");
                        writer.WriteAttributeString("type", column.Type.ToString());
                        writer.WriteAttributeString("name", column.Name);
                        writer.WriteAttributeString("num-distinct", column.NumDistinct.ToString());
                        writer.WriteAttributeString("is-continuous", column.IsContinuous ? "y" : "n");
                        if (column.IsTarget)
                            writer.WriteAttributeString("classification-target", "y");
                        writer.WriteEndElement();
                    }
                    foreach (var row in rows) {
                        writer.WriteStartElement("row");
                        foreach (var val in row.Data) {
                            writer.WriteStartElement("item");
                            if (val == null)
                                writer.WriteString("(null)");
                            else
                                writer.WriteString(val.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                return ret.ToString();
            }
        }
    }
}
