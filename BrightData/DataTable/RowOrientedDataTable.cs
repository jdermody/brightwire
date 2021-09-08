using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData.Buffer;
using BrightData.DataTable.Builders;
using BrightData.DataTable.Consumers;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.DataTable
{
    /// <summary>
    /// Data table in which the rows are stored contiguously
    /// </summary>
    internal class RowOrientedDataTable : DataTableBase, IRowOrientedDataTable
    {
        interface IColumnReader
        {
            object Read(BinaryReader reader);
        }
        class ColumnReader<T> : IColumnReader where T : notnull
        {
            readonly Func<BinaryReader, T> _reader;
            public ColumnReader(Func<BinaryReader, T> reader) => _reader = reader;
            public object Read(BinaryReader reader) => ReadTyped(reader);
            public T ReadTyped(BinaryReader reader) => _reader(reader);
        }

        interface IConsumerBinding
        {
            void Read(BinaryReader reader, uint index);
        }
        class ConsumerBinding<T> : IConsumerBinding where T : notnull
        {
            readonly ColumnReader<T> _reader;
            readonly IConsumeColumnData<T> _consumer;

            public ConsumerBinding(ColumnReader<T> reader, IConsumeColumnData<T> consumer)
            {
                _reader = reader;
                _consumer = consumer;
            }

            public void Read(BinaryReader reader, uint index) => _consumer.Add(_reader.ReadTyped(reader), index);
        }

        readonly ColumnInfo[] _columns;
        readonly uint[] _rowOffset;
        readonly IColumnReader[] _columnReaders;
        Stream _stream;

        public RowOrientedDataTable(IBrightDataContext context, Stream stream, bool readHeader) : base(context)
        {
            _stream = stream;
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            if (readHeader)
                ReadHeader(reader, DataTableOrientation.RowOriented);

            var numColumns = reader.ReadUInt32();
            MetaData.ReadFrom(reader);
            _columns = new ColumnInfo[numColumns];
            for (uint i = 0; i < numColumns; i++)
                _columns[i] = new ColumnInfo(reader, i);
            ColumnTypes = _columns.Select(c => c.ColumnType).ToArray();

            uint rowCount = reader.ReadUInt32();
            _rowOffset = new uint[rowCount];
            for (uint i = 0; i < rowCount; i++)
                _rowOffset[i] = reader.ReadUInt32();

            RowCount = (uint) _rowOffset.Length;
            ColumnCount = (uint) _columns.Length;

            _columnReaders = ColumnTypes.Select(GetReader).ToArray();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public override DataTableOrientation Orientation => DataTableOrientation.RowOriented;
        public BrightDataType[] ColumnTypes { get; }

        public IEnumerable<IDataTableSegment> Rows(params uint[] rowIndices)
        {
            foreach (var rowIndex in AllOrSpecifiedRows(rowIndices))
                yield return new Row(ColumnTypes, ReadRow(rowIndex));
        }

        object[] ReadRow(uint rowIndex)
        {
            lock (this)
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                _stream.Seek(_rowOffset[rowIndex], SeekOrigin.Begin);
                var row = new object[_columns.Length];
                for (int i = 0, len = _columnReaders.Length; i < len; i++)
                    row[i] = _columnReaders[i].Read(reader);
                return row;
            }
        }

        public void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback)
        {
            lock (this) {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                var ct = Context.CancellationToken;
                foreach (var index in rowIndices) {
                    if (ct.IsCancellationRequested)
                        break;

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
            // TODO: compress the columns based on frequency statistics
            var columns = this.AllOrSelectedColumnIndices(columnIndices).Select(i => (Index: i, Column: GetColumn(ColumnTypes[i], i, _columns[i].MetaData))).ToList();
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

        public IDataTableSegment Row(uint rowIndex) => new Row(ColumnTypes, ReadRow(rowIndex));
        public ISingleTypeTableSegment Column(uint columnIndex) => Columns(columnIndex).Single();
        public IEnumerable<(uint ColumnIndex, IMetaData MetaData)> ColumnAnalysis(IEnumerable<uint> columnIndices)
        {
            // see which columns need analysis
            var columnsToAnalyse = new List<(IConsumeColumnData, ICanComplete)>();
            var ret = new List<(uint ColumnIndex, IMetaData MetaData)>();
            foreach (var index in columnIndices) {
                var column = _columns[index];
                if (!column.MetaData.Get(Consts.HasBeenAnalysed, false))
                    columnsToAnalyse.Add(column.GetAnalysisConsumer());
                ret.Add((index, column.MetaData));
            }

            // analyse columns
            if (columnsToAnalyse.Any()) {
                ReadTyped(columnsToAnalyse.Select(d => d.Item1));
                foreach(var item in columnsToAnalyse)
                    item.Item2.Complete();
            }

            return ret;
        }

        public IMetaData ColumnMetaData(uint columnIndex) => _columns[columnIndex].MetaData;

        public IRowOrientedDataTable Clone(string? filePath = null)
        {
            if (filePath is not null && _stream is FileStream fileStream && fileStream.Name == filePath) {
                throw new Exception("Cannot clone a currently open row oriented table and write to the same path");
            }
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

            lock (this) {
                _stream.Seek(_rowOffset[0], SeekOrigin.Begin);
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                var ct = Context.CancellationToken;

                for (uint i = 0; i < rowCount && !ct.IsCancellationRequested; i++) {
                    var row = new object[ColumnCount];
                    for (int j = 0, len = _columnReaders.Length; j < len; j++)
                        row[j] = _columnReaders[j].Read(reader);
                    callback(row, i);
                }
            }
        }

        public void ReadTyped(IEnumerable<IConsumeColumnData> consumers, uint maxRows = 4294967295U)
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
            lock (this) {
                _stream.Seek(_rowOffset[0], SeekOrigin.Begin);
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                var ct = Context.CancellationToken;

                for (uint i = 0; i < rowCount && !ct.IsCancellationRequested; i++) {
                    for (uint j = 0, len = ColumnCount; j < len; j++) {
                        if (bindings.TryGetValue(j, out var binding))
                            binding.Read(reader, i);
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

            builder.WriteHeader(ColumnCount, RowCount, MetaData);
            var columns = Columns(_columns.Length.AsRange().ToArray());
            foreach (var column in columns) {
                var position = builder.Write(column);
                columnOffsets.Add((position, builder.GetCurrentPosition()));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(Context);
        }

        (ISingleTypeTableSegment Segment, IConsumeColumnData Consumer) GetColumn(BrightDataType columnType, uint index, IMetaData metaData)
        {
            var type = typeof(GrowableDataTableSegment<>).MakeGenericType(columnType.GetDataType());
            var columnInfo = new ColumnInfo(index, columnType, metaData);
            return GenericActivator.Create<ISingleTypeTableSegment, IConsumeColumnData>(type, Context, columnInfo, Context.TempStreamProvider);
        }

        static string ReadString(BinaryReader reader) => reader.ReadString();
        static double ReadDouble(BinaryReader reader) => reader.ReadDouble();
        static decimal ReadDecimal(BinaryReader reader) => reader.ReadDecimal();
        static int ReadInt32(BinaryReader reader) => reader.ReadInt32();
        static short ReadInt16(BinaryReader reader) => reader.ReadInt16();
        static float ReadSingle(BinaryReader reader) => reader.ReadSingle();
        static bool ReadBoolean(BinaryReader reader) => reader.ReadBoolean();
        static DateTime ReadDate(BinaryReader reader) => new(reader.ReadInt64());
        static long ReadInt64(BinaryReader reader) => reader.ReadInt64();
        static sbyte ReadByte(BinaryReader reader) => reader.ReadSByte();
        IndexList ReadIndexList(BinaryReader reader) => Context.CreateIndexList(reader);
        WeightedIndexList ReadWeightedIndexList(BinaryReader reader) => Context.CreateWeightedIndexList(reader);
        Vector<float> ReadVector(BinaryReader reader) => Context.CreateVector<float>(reader);
        Matrix<float> ReadMatrix(BinaryReader reader) => Context.CreateMatrix<float>(reader);
        Tensor3D<float> ReadTensor3D(BinaryReader reader) => Context.CreateTensor3D<float>(reader);
        Tensor4D<float> ReadTensor4D(BinaryReader reader) => Context.CreateTensor4D<float>(reader);
        static BinaryData ReadBinaryData(BinaryReader reader) => new(reader);

        IColumnReader GetReader(BrightDataType type)
        {
            return type switch {
                BrightDataType.String => new ColumnReader<string>(ReadString),
                BrightDataType.Double => new ColumnReader<double>(ReadDouble),
                BrightDataType.Decimal => new ColumnReader<decimal>(ReadDecimal),
                BrightDataType.Int => new ColumnReader<int>(ReadInt32),
                BrightDataType.Short => new ColumnReader<short>(ReadInt16),
                BrightDataType.Float => new ColumnReader<Single>(ReadSingle),
                BrightDataType.Boolean => new ColumnReader<bool>(ReadBoolean),
                BrightDataType.Date => new ColumnReader<DateTime>(ReadDate),
                BrightDataType.Long => new ColumnReader<long>(ReadInt64),
                BrightDataType.SByte => new ColumnReader<sbyte>(ReadByte),
                BrightDataType.IndexList => new ColumnReader<IndexList>(ReadIndexList),
                BrightDataType.WeightedIndexList => new ColumnReader<WeightedIndexList>(ReadWeightedIndexList),
                BrightDataType.Vector => new ColumnReader<Vector<float>>(ReadVector),
                BrightDataType.Matrix => new ColumnReader<Matrix<float>>(ReadMatrix),
                BrightDataType.Tensor3D => new ColumnReader<Tensor3D<float>>(ReadTensor3D),
                BrightDataType.Tensor4D => new ColumnReader<Tensor4D<float>>(ReadTensor4D),
                BrightDataType.BinaryData => new ColumnReader<BinaryData>(ReadBinaryData),
                _ => throw new ArgumentException($"Invalid column type: {type}")
            };
        }

        IRowOrientedDataTable Copy(uint[] rowIndices, string? filePath)
        {
            using var builder = GetBuilderForSelf((uint)rowIndices.Length, filePath);
            ForEachRow(rowIndices, builder.AddRow);
            return builder.Build(Context);
        }

        RowOrientedTableBuilder GetBuilderForSelf(uint rowCount, string? filePath)
        {
            var ret = new RowOrientedTableBuilder(MetaData.Clone(), rowCount, filePath);
            ret.AddColumnsFrom(this);
            return ret;
        }

        public IRowOrientedDataTable Bag(uint sampleCount, string? filePath = null)
        {
            var rowIndices = this.RowIndices().ToArray().Bag(sampleCount, Context.Random);
            return Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable ConcatRows(string? filePath, params IRowOrientedDataTable[] others)
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

        public IRowOrientedDataTable CopyRows(string? filePath, params uint[] rowIndices)
        {
            if (rowIndices.Length == 0)
                rowIndices = AllOrSpecifiedRows(rowIndices).ToArray();

            return Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Shuffle(string? filePath = null)
        {
            var rowIndices = this.RowIndices().Shuffle(Context.Random).ToArray();
            return Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Sort(uint columnIndex, bool ascending, string? filePath = null)
        {
            var sortData = new List<(object Item, uint RowIndex)>();
            ForEachRow((row, rowIndex) => sortData.Add((row[columnIndex], rowIndex)));
            var sorted = ascending
                ? sortData.OrderBy(d => d.Item)
                : sortData.OrderByDescending(d => d.Item);
            var rowIndices = sorted.Select(d => d.RowIndex).ToArray();
            return Copy(rowIndices, filePath);
        }

        public IEnumerable<(string Label, IHybridBuffer[] ColumnData)> GroupBy(params uint[] columnIndices)
        {
            var groupedData = new Dictionary<string, IHybridBuffer[]>();
            ForEachRow((row, rowIndex) => {
                var label = GetGroupLabel(columnIndices, row);
                if (!groupedData.TryGetValue(label, out var data))
                    groupedData.Add(label, data = _columns.Select(c => c.MetaData.GetGrowableSegment(c.ColumnType, Context, Context.TempStreamProvider)).ToArray());
                foreach(var (obj, buffer) in row.Zip(data))
                    buffer.Add(obj, rowIndex);
            });

            return groupedData.OrderBy(g => g.Key).Select(kv => (kv.Key, kv.Value));
        }

        public override string ToString() => string.Join(", ", _columns.Select(c => c.ToString()));

        public string FirstRow => ToString(Row(0));
        public string SecondRow => ToString(Row(1));
        public string ThirdRow => ToString(Row(2));
        public string LastRow => ToString(Row(RowCount - 1));

        static string ToString(IDataTableSegment segment)
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
