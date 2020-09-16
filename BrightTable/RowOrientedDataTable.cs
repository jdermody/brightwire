using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Buffers;
using BrightTable.Builders;
using BrightTable.Input;
using BrightTable.Segments;
using BrightTable.Transformations;

namespace BrightTable
{
    class RowOrientedDataTable : DataTableBase, IRowOrientedDataTable
    {
        interface IColumnReader
        {
            object Read(BinaryReader reader);
        }
        interface IColumnReader<out T>
        {
            T ReadTyped(BinaryReader reader);
        }
        class ColumnReader<T> : IColumnReader, IColumnReader<T>
        {
            private readonly Func<BinaryReader, T> _reader;
            public ColumnReader(Func<BinaryReader, T> reader) => _reader = reader;
            public object Read(BinaryReader reader) => (object)ReadTyped(reader);
            public T ReadTyped(BinaryReader reader) => _reader(reader);
        }

        interface IConsumerBinding
        {
            void Read(uint rowIndex, BinaryReader reader);
        }
        class ConsumerBinding<T> : IConsumerBinding
        {
            private readonly ColumnReader<T> _reader;
            private readonly ITypedRowConsumer<T> _consumer;

            public ConsumerBinding(IColumnReader reader, ITypedRowConsumer consumer)
            {
                _reader = (ColumnReader<T>)reader;
                _consumer = (ITypedRowConsumer<T>)consumer;
            }

            public uint ColumnIndex => _consumer.ColumnIndex;
            public void Read(uint rowIndex, BinaryReader reader) => _consumer.Set(rowIndex, _reader.ReadTyped(reader));
        }
        readonly ColumnInfo[] _columns;
        readonly uint[] _rowOffset;
        readonly InputData _data;
        readonly IColumnReader[] _columnReaders;

        public RowOrientedDataTable(IBrightDataContext context, InputData data, bool readHeader) : base(context)
        {
            _data = data;
            var reader = data.Reader;

            if (readHeader) {
                var version = reader.ReadInt32();
                if (version > Consts.DataTableVersion)
                    throw new Exception($"Segment table version {version} exceeds {Consts.DataTableVersion}");
                var orientation = (DataTableOrientation)reader.ReadInt32();
                if (orientation != DataTableOrientation.RowOriented)
                    throw new Exception("Invalid orientation");
            }

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
            _data.Dispose();
        }

        public DataTableOrientation Orientation => DataTableOrientation.RowOriented;
        public ColumnType[] ColumnTypes { get; }

        public IEnumerable<IDataTableSegment> Rows(params uint[] rowIndices)
        {
            if (rowIndices.Any()) {
                var ret = new List<IDataTableSegment>();
                ForEachRow(rowIndices, row => ret.Add(new Row(ColumnTypes, row)));
                return ret;
            }

            return Enumerable.Empty<IDataTableSegment>();
        }

        public IDataTableSegment Row(uint rowIndex) => Rows(rowIndex).SingleOrDefault();

        public void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback)
        {
            lock (_data) {
                var reader = _data.Reader;
                foreach (var index in rowIndices) {
                    if (index < _rowOffset.Length) {
                        _data.MoveTo(_rowOffset[index]);
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

        public IRowOrientedDataTable AsRowOriented(string filePath = null)
        {
            using var builder = GetBuilderForSelf(RowCount, filePath);

            // ReSharper disable once AccessToDisposedClosure
            ForEachRow((row, index) => builder.AddRow(row));

            return builder.Build(Context);
        }

        public override void ForEachRow(Action<object[]> callback) => ForEachRow((row, index) => callback(row));
        protected override IDataTable Table => this;

        public void ForEachRow(Action<object[], uint> callback, uint maxRows = uint.MaxValue)
        {
            var rowCount = Math.Min(maxRows, RowCount);

            lock (_data) {
                _data.MoveTo(_rowOffset[0]);
                var reader = _data.Reader;

                for (uint i = 0; i < rowCount; i++) {
                    var row = new object[ColumnCount];
                    for (int j = 0, len = _columnReaders.Length; j < len; j++)
                        row[j] = _columnReaders[j].Read(reader);
                    callback(row, i);
                }
            }
        }

        public void ReadTyped(ITypedRowConsumer[] consumers, uint maxRows = uint.MaxValue)
        {
            // create bindings for each column
            var bindings = new Dictionary<uint, IConsumerBinding>();
            foreach (var consumer in consumers) {
                var column = _columns[consumer.ColumnIndex];
                var columnReader = _columnReaders[column.Index];
                var type = typeof(ConsumerBinding<>).MakeGenericType(ExtensionMethods.GetDataType(consumer.ColumnType));
                var binding = (IConsumerBinding)Activator.CreateInstance(type, columnReader, consumer);
                bindings.Add(column.Index, binding);
            }

            var rowCount = Math.Min(maxRows, RowCount);
            lock (_data) {
                _data.MoveTo(_rowOffset[0]);
                var reader = _data.Reader;

                for (uint i = 0; i < rowCount; i++) {
                    for (uint j = 0, len = ColumnCount; j < len; j++) {
                        if (bindings.TryGetValue(j, out var binding))
                            binding.Read(i, reader);
                        else {
                            // read and discard value
                            _columnReaders[j].Read(reader);
                        }
                    }
                }
            }
        }

        public IColumnOrientedDataTable AsColumnOriented(string filePath = null)
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

        (ISingleTypeTableSegment Segment, ITypedRowConsumer Consumer) _GetColumn(ColumnType columnType, uint columnIndex, IMetaData metadata)
        {
            var type = typeof(InMemoryBuffer<>).MakeGenericType(ExtensionMethods.GetDataType(columnType));
            var ret = Activator.CreateInstance(type, Context, columnType, columnIndex, metadata, RowCount);
            return ((ISingleTypeTableSegment)ret, (ITypedRowConsumer)ret);
        }

        private string _ReadString(BinaryReader reader) => reader.ReadString();
        private double _ReadDouble(BinaryReader reader) => reader.ReadDouble();
        private decimal _ReadDecimal(BinaryReader reader) => reader.ReadDecimal();
        private int _ReadInt32(BinaryReader reader) => reader.ReadInt32();
        private short _ReadInt16(BinaryReader reader) => reader.ReadInt16();
        private float _ReadSingle(BinaryReader reader) => reader.ReadSingle();
        private bool _ReadBoolean(BinaryReader reader) => reader.ReadBoolean();
        private DateTime _ReadDate(BinaryReader reader) => new DateTime(reader.ReadInt64());
        private long _ReadInt64(BinaryReader reader) => reader.ReadInt64();
        private sbyte _ReadByte(BinaryReader reader) => reader.ReadSByte();
        private IndexList _ReadIndexList(BinaryReader reader) => IndexList.ReadFrom(Context, reader);
        private WeightedIndexList _ReadWeightedIndexList(BinaryReader reader) => WeightedIndexList.ReadFrom(Context, reader);
        private Vector<float> _ReadVector(BinaryReader reader) => new Vector<float>(Context, reader);
        private Matrix<float> _ReadMatrix(BinaryReader reader) => new Matrix<float>(Context, reader);
        private Tensor3D<float> _ReadTensor3D(BinaryReader reader) => new Tensor3D<float>(Context, reader);
        private Tensor4D<float> _ReadTensor4D(BinaryReader reader) => new Tensor4D<float>(Context, reader);
        private BinaryData _ReadBinaryData(BinaryReader reader) => new BinaryData(reader);

        IColumnReader _GetReader(ColumnType type)
        {
            switch (type) {
                case ColumnType.String:
                    return new ColumnReader<string>(_ReadString);
                case ColumnType.Double:
                    return new ColumnReader<double>(_ReadDouble);
                case ColumnType.Decimal:
                    return new ColumnReader<decimal>(_ReadDecimal);
                case ColumnType.Int:
                    return new ColumnReader<int>(_ReadInt32);
                case ColumnType.Short:
                    return new ColumnReader<short>(_ReadInt16);
                case ColumnType.Float:
                    return new ColumnReader<Single>(_ReadSingle);
                case ColumnType.Boolean:
                    return new ColumnReader<bool>(_ReadBoolean);
                case ColumnType.Date:
                    return new ColumnReader<DateTime>(_ReadDate);
                case ColumnType.Long:
                    return new ColumnReader<long>(_ReadInt64);
                case ColumnType.Byte:
                    return new ColumnReader<sbyte>(_ReadByte);
                case ColumnType.IndexList:
                    return new ColumnReader<IndexList>(_ReadIndexList);
                case ColumnType.WeightedIndexList:
                    return new ColumnReader<WeightedIndexList>(_ReadWeightedIndexList);
                case ColumnType.Vector:
                    return new ColumnReader<Vector<float>>(_ReadVector);
                case ColumnType.Matrix:
                    return new ColumnReader<Matrix<float>>(_ReadMatrix);
                case ColumnType.Tensor3D:
                    return new ColumnReader<Tensor3D<float>>(_ReadTensor3D);
                case ColumnType.Tensor4D:
                    return new ColumnReader<Tensor4D<float>>(_ReadTensor4D);
                case ColumnType.BinaryData:
                    return new ColumnReader<BinaryData>(_ReadBinaryData);
                default:
                    return null;
            }
        }

        IRowOrientedDataTable _Copy(uint[] rowIndices, string filePath)
        {
            using var builder = GetBuilderForSelf((uint)rowIndices.Length, filePath);
            // ReSharper disable once AccessToDisposedClosure
            ForEachRow(rowIndices, row => builder.AddRow(row));
            return builder.Build(Context);
        }

        RowOrientedTableBuilder GetBuilderForSelf(uint rowCount, string filePath)
        {
            var ret = new RowOrientedTableBuilder(rowCount, filePath);
            ret.AddColumnsFrom(this);
            return ret;
        }

        public IRowOrientedDataTable Bag(uint sampleCount, int? randomSeed = null, string filePath = null)
        {
            var rowIndices = this.RowIndices().ToArray().Bag(sampleCount, randomSeed);
            return _Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Concat(params IRowOrientedDataTable[] others) => Concat(null, others);
        public IRowOrientedDataTable Concat(string filePath, params IRowOrientedDataTable[] others)
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

        public IRowOrientedDataTable Mutate(Func<object[], object[]> projector, string filePath = null)
        {
            var mutatedRows = new List<object[]>();
            var columnTypes = new Dictionary<uint, ColumnType>();

            ForEachRow(row => {
                var projected = projector(row);
                if (projected != null) {
                    if (projected.Length > columnTypes.Count) {
                        for (uint i = 0, len = (uint)projected.Length; i < len; i++) {
                            var type = projected.GetType().GetColumnType();
                            if (columnTypes.TryGetValue(i, out var existing) && existing != type)
                                throw new Exception($"Column {i} type changed between mutations");
                            columnTypes.Add(i, type);
                        }
                    }
                    mutatedRows.Add(projected);
                }
            });

            using var builder = new RowOrientedTableBuilder((uint)mutatedRows.Count, filePath);
            foreach (var column in columnTypes.OrderBy(c => c.Key))
                builder.AddColumn(column.Value, $"Column {column.Key}");
            foreach (var row in mutatedRows)
                builder.AddRow(row);
            return builder.Build(Context);
        }

        public IRowOrientedDataTable SelectRows(params uint[] rowIndices) => SelectRows(null, rowIndices);
        public IRowOrientedDataTable SelectRows(string filePath, params uint[] rowIndices)
        {
            return _Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Shuffle(int? randomSeed = null, string filePath = null)
        {
            var rowIndices = this.RowIndices().Shuffle(randomSeed).ToArray();
            return _Copy(rowIndices, filePath);
        }

        public IRowOrientedDataTable Sort(bool ascending, uint columnIndex, string filePath = null)
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
                var label = row[columnIndex].ToString();
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
            if (segment != null) {
                for (uint i = 0; i < segment.Size; i++) {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(segment[i].ToString());
                }
            }

            return sb.ToString();
        }
    }
}
