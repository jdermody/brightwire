using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.DataTable.Builders;
using BrightData.Helper;
using BrightData.Segment;
using BrightTable.Buffer;

namespace BrightData.DataTable
{
    internal class RowOrientedDataTable : DataTableBase, IRowOrientedDataTable
    {
        interface IColumnReader
        {
            object Read(BinaryReader reader);
        }
        interface IColumnReader<out T> where T: notnull
        {
            T ReadTyped(BinaryReader reader);
        }
        class ColumnReader<T> : IColumnReader, IColumnReader<T> where T : notnull
        {
            readonly Func<BinaryReader, T> _reader;
            public ColumnReader(Func<BinaryReader, T> reader) => _reader = reader;
            public object Read(BinaryReader reader) => ReadTyped(reader);
            public T ReadTyped(BinaryReader reader) => _reader(reader);
        }

        interface IConsumerBinding
        {
            void Read(BinaryReader reader);
        }
        class ConsumerBinding<T> : IConsumerBinding where T : notnull
        {
            readonly ColumnReader<T> _reader;
            readonly IConsumeColumnData<T> _consumer;

            public ConsumerBinding(IColumnReader reader, IConsumeColumnData consumer)
            {
                _reader = (ColumnReader<T>)reader;
                _consumer = (IConsumeColumnData<T>)consumer;
            }

            public void Read(BinaryReader reader) => _consumer.Add(_reader.ReadTyped(reader));
        }
        readonly ColumnInfo[] _columns;
        readonly uint[] _rowOffset;
        readonly IColumnReader[] _columnReaders;

        public RowOrientedDataTable(IBrightDataContext context, Stream stream, bool readHeader) : base(context, stream)
        {
            var reader = Reader;

            if (readHeader)
                _ReadHeader(reader, DataTableOrientation.RowOriented);

            var numColumns = reader.ReadUInt32();
            _columns = new ColumnInfo[numColumns];
            for (uint i = 0; i < numColumns; i++)
                _columns[i] = new ColumnInfo(reader, i);
            ColumnTypes = _columns.Select(c => c.ColumnType).ToArray();

            uint rowCount = reader.ReadUInt32();
            _rowOffset = new uint[rowCount];
            for (uint i = 0; i < rowCount; i++)
                _rowOffset[i] = reader.ReadUInt32();

            RowCount = (uint)_rowOffset.Length;
            ColumnCount = (uint)_columns.Length;

            _columnReaders = ColumnTypes.Select(_GetReader).ToArray();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public DataTableOrientation Orientation => DataTableOrientation.RowOriented;
        public ColumnType[] ColumnTypes { get; }

        public IEnumerable<IDataTableSegment> Rows(params uint[] rowIndices)
        {
            foreach (var rowIndex in AllOrSpecifiedRows(rowIndices))
                yield return new Row(ColumnTypes, _ReadRow(rowIndex));
        }

        public IDataTableSegment Row(uint rowIndex) => new Row(ColumnTypes, _ReadRow(rowIndex));

        object[] _ReadRow(uint rowIndex)
        {
            lock (_stream)
            {
                var reader = Reader;
                _stream.Seek(_rowOffset[rowIndex], SeekOrigin.Begin);
                var row = new object[_columns.Length];
                for (int i = 0, len = _columnReaders.Length; i < len; i++)
                    row[i] = _columnReaders[i].Read(reader);
                return row;
            }
        }

        public void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback)
        {
            lock (_stream) {
                var reader = Reader;
                foreach (var index in rowIndices) {
                    if (index < _rowOffset.Length) {
                        _stream.Seek(_rowOffset[index], SeekOrigin.Begin);
                        var row = new object[_columns.Length];
                        for (int i = 0, len = _columnReaders.Length; i < len; i++)
                            row[i] = _columnReaders[i].Read(reader);
                        callback(row);
                    }
                }
            }
        }

        public IEnumerable<ISingleTypeTableSegment> Columns(params uint[] columnIndices)
        {
            // TODO: optionally compress the columns based on unique count statistics
            var columns = AllOrSpecifiedColumns(columnIndices).Select(i => (Index: i, Column: _GetColumn(ColumnTypes[i], i, _columns[i].MetaData))).ToList();
            if (columns.Any()) {
                // set the column metadata
                columns.ForEach(item => {
                    var metadata = item.Column.Segment.MetaData;
                    var column = _columns[item.Index];
                    column.MetaData.CopyTo(metadata);
                });
                var consumers = columns.Select(c => c.Column.Consumer).ToArray();
                ReadTyped(consumers);
            }

            return columns.Select(c => c.Column.Segment);
        }

        public ISingleTypeTableSegment Column(uint columnIndex) => Columns(columnIndex).Single();

        public IEnumerable<IMetaData> ColumnMetaData(params uint[] columnIndices)
        {
            return AllOrSpecifiedColumns(columnIndices).Select(i => _columns[i].MetaData);
        }

        public IRowOrientedDataTable AsRowOriented(string? filePath = null)
        {
            using var builder = GetBuilderForSelf(RowCount, filePath);

            // ReSharper disable once AccessToDisposedClosure
            ForEachRow((row, index) => builder.AddRow(row));

            return builder.Build(Context);
        }

        public override void ForEachRow(Action<object[]> callback, uint maxRows = uint.MaxValue) => ForEachRow((row, index) => callback(row), maxRows);
        protected override IDataTable Table => this;

        public void ForEachRow(Action<object[], uint> callback, uint maxRows = uint.MaxValue)
        {
            var rowCount = Math.Min(maxRows, RowCount);

            lock (_stream) {
                _stream.Seek(_rowOffset[0], SeekOrigin.Begin);
                var reader = Reader;

                for (uint i = 0; i < rowCount; i++) {
                    var row = new object[ColumnCount];
                    for (int j = 0, len = _columnReaders.Length; j < len; j++)
                        row[j] = _columnReaders[j].Read(reader);
                    callback(row, i);
                }
            }
        }

        public void ReadTyped(IConsumeColumnData[] consumers, uint maxRows = uint.MaxValue)
        {
            // create bindings for each column
            var bindings = new Dictionary<uint, IConsumerBinding>();
            foreach (var consumer in consumers) {
                var column = _columns[consumer.ColumnIndex];
                // ReSharper disable once InconsistentlySynchronizedField
                var columnReader = _columnReaders[column.Index];
                var type = typeof(ConsumerBinding<>).MakeGenericType(consumer.ColumnType.GetDataType());
                var binding = GenericActivator.Create<IConsumerBinding>(type, columnReader, consumer);
                bindings.Add(column.Index, binding);
            }

            var rowCount = Math.Min(maxRows, RowCount);
            lock (_stream) {
                _stream.Seek(_rowOffset[0], SeekOrigin.Begin);
                var reader = Reader;

                for (uint i = 0; i < rowCount; i++) {
                    for (uint j = 0, len = ColumnCount; j < len; j++) {
                        if (bindings.TryGetValue(j, out var binding))
                            binding.Read(reader);
                        else {
                            // read and discard value
                            _columnReaders[j].Read(reader);
                        }
                    }
                }
            }
        }

        public IColumnOrientedDataTable AsColumnOriented(string? filePath = null)
        {
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            using var builder = new ColumnOrientedTableBuilder(filePath);

            builder.WriteHeader(ColumnCount, RowCount);
            var columns = Columns(_columns.Length.AsRange().ToArray());
            foreach (var column in columns) {
                var position = builder.Write(column);
                columnOffsets.Add((position, builder.GetCurrentPosition()));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(Context);
        }

        (ISingleTypeTableSegment Segment, IConsumeColumnData Consumer) _GetColumn(ColumnType columnType, uint index, IMetaData metaData)
        {
            var type = typeof(InMemoryBuffer<>).MakeGenericType(columnType.GetDataType());
            var columnInfo = new ColumnInfo(index, columnType, metaData);
            return GenericActivator.Create<ISingleTypeTableSegment, IConsumeColumnData>(type, Context, columnInfo, RowCount, Context.TempStreamProvider);
        }

        static string _ReadString(BinaryReader reader) => reader.ReadString();
        static double _ReadDouble(BinaryReader reader) => reader.ReadDouble();
        static decimal _ReadDecimal(BinaryReader reader) => reader.ReadDecimal();
        static int _ReadInt32(BinaryReader reader) => reader.ReadInt32();
        static short _ReadInt16(BinaryReader reader) => reader.ReadInt16();
        static float _ReadSingle(BinaryReader reader) => reader.ReadSingle();
        static bool _ReadBoolean(BinaryReader reader) => reader.ReadBoolean();
        static DateTime _ReadDate(BinaryReader reader) => new DateTime(reader.ReadInt64());
        static long _ReadInt64(BinaryReader reader) => reader.ReadInt64();
        static sbyte _ReadByte(BinaryReader reader) => reader.ReadSByte();
        IndexList _ReadIndexList(BinaryReader reader) => Context.CreateIndexList(reader);
        WeightedIndexList _ReadWeightedIndexList(BinaryReader reader) => Context.CreateWeightedIndexList(reader);
        Vector<float> _ReadVector(BinaryReader reader) => Context.CreateVector<float>(reader);
        Matrix<float> _ReadMatrix(BinaryReader reader) => Context.CreateMatrix<float>(reader);
        Tensor3D<float> _ReadTensor3D(BinaryReader reader) => Context.CreateTensor3D<float>(reader);
        Tensor4D<float> _ReadTensor4D(BinaryReader reader) => Context.CreateTensor4D<float>(reader);
        static BinaryData _ReadBinaryData(BinaryReader reader) => new BinaryData(reader);

        IColumnReader _GetReader(ColumnType type)
        {
            return type switch {
                ColumnType.String => new ColumnReader<string>(_ReadString),
                ColumnType.Double => new ColumnReader<double>(_ReadDouble),
                ColumnType.Decimal => new ColumnReader<decimal>(_ReadDecimal),
                ColumnType.Int => new ColumnReader<int>(_ReadInt32),
                ColumnType.Short => new ColumnReader<short>(_ReadInt16),
                ColumnType.Float => new ColumnReader<Single>(_ReadSingle),
                ColumnType.Boolean => new ColumnReader<bool>(_ReadBoolean),
                ColumnType.Date => new ColumnReader<DateTime>(_ReadDate),
                ColumnType.Long => new ColumnReader<long>(_ReadInt64),
                ColumnType.Byte => new ColumnReader<sbyte>(_ReadByte),
                ColumnType.IndexList => new ColumnReader<IndexList>(_ReadIndexList),
                ColumnType.WeightedIndexList => new ColumnReader<WeightedIndexList>(_ReadWeightedIndexList),
                ColumnType.Vector => new ColumnReader<Vector<float>>(_ReadVector),
                ColumnType.Matrix => new ColumnReader<Matrix<float>>(_ReadMatrix),
                ColumnType.Tensor3D => new ColumnReader<Tensor3D<float>>(_ReadTensor3D),
                ColumnType.Tensor4D => new ColumnReader<Tensor4D<float>>(_ReadTensor4D),
                ColumnType.BinaryData => new ColumnReader<BinaryData>(_ReadBinaryData),
                _ => throw new ArgumentException($"Invalid column type: {type}")
            };
        }

        IRowOrientedDataTable _Copy(uint[] rowIndices, string? filePath)
        {
            using var builder = GetBuilderForSelf((uint)rowIndices.Length, filePath);
            ForEachRow(rowIndices, builder.AddRow);
            return builder.Build(Context);
        }

        RowOrientedTableBuilder GetBuilderForSelf(uint rowCount, string? filePath)
        {
            var ret = new RowOrientedTableBuilder(rowCount, filePath);
            ret.AddColumnsFrom(this);
            return ret;
        }

        public IRowOrientedDataTable Bag(uint sampleCount, string? filePath = null)
        {
            var rowIndices = this.RowIndices().ToArray().Bag(sampleCount, Context.Random);
            return _Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Concat(params IRowOrientedDataTable[] others) => Concat(null, others);
        public IRowOrientedDataTable Concat(string? filePath, params IRowOrientedDataTable[] others)
        {
            var rowCount = RowCount;
            foreach (var other in others) {
                if (other.ColumnCount != ColumnCount)
                    throw new ArgumentException("Columns must agree - column count was different");
                if (ColumnTypes.Zip(other.ColumnTypes, (t1, t2) => t1 == t2).Any(v => v == false))
                    throw new ArgumentException("Columns must agree - types were different");

                rowCount += other.RowCount;
            }
            using var builder = GetBuilderForSelf(rowCount, filePath);
            ForEachRow(builder.AddRow);

            foreach (var other in others)
                other.ForEachRow(builder.AddRow);
            return builder.Build(Context);
        }

        public IRowOrientedDataTable CopyRows(params uint[] rowIndices) => CopyRows(null, rowIndices);
        public IRowOrientedDataTable CopyRows(string? filePath, params uint[] rowIndices)
        {
            return _Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Shuffle(string? filePath = null)
        {
            var rowIndices = this.RowIndices().Shuffle(Context.Random).ToArray();
            return _Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Sort(uint columnIndex, bool @ascending, string? filePath = null)
        {
            var sortData = new List<(object Item, uint RowIndex)>();
            ForEachRow((row, rowIndex) => sortData.Add((row[columnIndex], rowIndex)));
            var sorted = ascending
                ? sortData.OrderBy(d => d.Item)
                : sortData.OrderByDescending(d => d.Item);
            var rowIndices = sorted.Select(d => d.RowIndex).ToArray();
            return _Copy(rowIndices, filePath);
        }

        public IEnumerable<(string Label, IRowOrientedDataTable Table)> GroupBy(uint columnIndex)
        {
            var groupedData = new Dictionary<string, List<object[]>>();
            ForEachRow(row => {
                var label = row[columnIndex].ToString() ?? throw new Exception("Cannot group by string when value is null");
                if (!groupedData.TryGetValue(label, out var data))
                    groupedData.Add(label, data = new List<object[]>());
                data.Add(row);
            });

            return groupedData.Select(g => {
                using var builder = GetBuilderForSelf((uint) g.Value.Count, null);
                g.Value.ForEach(builder.AddRow);
                return (g.Key, builder.Build(Context));
            });
        }

        public override string ToString() => String.Join(", ", _columns.Select(c => c.ToString()));

        public string FirstRow => _ToString(Row(0));
        public string SecondRow => _ToString(Row(1));
        public string ThirdRow => _ToString(Row(2));
        public string LastRow => _ToString(Row(RowCount - 1));

        string _ToString(IDataTableSegment segment)
        {
            var sb = new StringBuilder();
            for (uint i = 0; i < segment.Size; i++)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(segment[i]);
            }

            return sb.ToString();
        }
    }
}
