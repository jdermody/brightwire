using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightTable.Builders;
using BrightTable.Segments;
using BrightTable.Transformations;

namespace BrightTable
{
    class ColumnOrientedDataTable : DataTableBase, IColumnOrientedDataTable
    {
        interface IConsumerBinding
        {
            void Copy(uint maxRows);
        }
        class ConsumerBinding<T> : IConsumerBinding
        {
            private readonly IDataTableSegment<T> _segment;
            private readonly ITypedRowConsumer<T> _consumer;

            public ConsumerBinding(ISingleTypeTableSegment segment, ITypedRowConsumer consumer)
            {
                _segment = (IDataTableSegment<T>)segment;
                _consumer = (ITypedRowConsumer<T>)consumer;
            }

            public void Copy(uint maxRows)
            {
                uint index = 0;
                using var enumerator = _segment.EnumerateTyped().GetEnumerator();
                while (index < maxRows && enumerator.MoveNext())
                {
                    _consumer.Add(enumerator.Current);
                    ++index;
                }
            }
        }

        readonly (IColumnInfo Info, ISingleTypeTableSegment Segment)[] _columns;

        public ColumnOrientedDataTable(IBrightDataContext context, Stream stream, bool readHeader) : base(context, stream)
        {
            var reader = Reader;
            if (readHeader)
                _ReadHeader(reader, DataTableOrientation.ColumnOriented);
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();

            _columns = new (IColumnInfo Info, ISingleTypeTableSegment Segment)[ColumnCount];
            ColumnTypes = new ColumnType[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++) {
                var nextColumnPosition = reader.ReadInt64();
                _columns[i] = _Load(i, reader, 32768);
                ColumnTypes[i] = _columns[i].Info.ColumnType;
                _stream.Seek(nextColumnPosition, SeekOrigin.Begin);
            }
        }

        (IColumnInfo Info, ISingleTypeTableSegment Segment) _Load(uint index, BinaryReader reader, uint inMemorySize)
        {
            var columnType = (ColumnType) reader.ReadByte();
            var metadata = new MetaData(reader);

            // ensure the metadata has the index and type
            metadata.Set(Consts.Index, index);
            metadata.SetType(columnType);

            // create the column
            var stream = reader.BaseStream;
            var dataType = columnType.GetDataType();
            var ret = Activator.CreateInstance(typeof(Column<>).MakeGenericType(dataType),
                index,
                columnType,
                metadata,
                Context,
                stream,
                inMemorySize
            );

            return ((IColumnInfo)ret, (ISingleTypeTableSegment)ret);
        }

        public void Dispose()
        {
            foreach (var (_, segment) in _columns)
                segment.Dispose();
            _stream.Dispose();
        }

        public DataTableOrientation Orientation => DataTableOrientation.ColumnOriented;
        public ColumnType[] ColumnTypes { get; }
        public override void ForEachRow(Action<object[]> callback)
        {
            ForEachRow((row, index) => callback(row));
        }

        protected override IDataTable Table => this;

        public ISingleTypeTableSegment[] AllColumns()
        {
            var ret = new ISingleTypeTableSegment[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++)
                ret[i] = _columns[i].Segment;
            return ret;
        }

        ISingleTypeTableSegment IDataTable.Column(uint columnIndex) => _columns[columnIndex].Segment;

        public IEnumerable<ISingleTypeTableSegment> Columns(params uint[] columnIndices)
        {
            var table = new Dictionary<uint, ISingleTypeTableSegment>();
            foreach (var index in AllOrSpecifiedColumns(columnIndices).OrderBy(i => i).Distinct())
                table.Add(index, _columns[index].Segment);
            return columnIndices.Select(i => table[i]);
        }

        public void ReadTyped(ITypedRowConsumer[] consumers, uint maxRows = UInt32.MaxValue)
        {
            var bindings = consumers.Select(consumer => (IConsumerBinding)Activator.CreateInstance(
                typeof(ConsumerBinding<>).MakeGenericType(consumer.ColumnType.GetDataType()),
                _columns[consumer.ColumnIndex].Segment,
                consumer
            ));
            foreach(var binding in bindings)
                binding?.Copy(maxRows);
        }

        public void ForEachRow(Action<object[], uint> callback, uint maxRows = uint.MaxValue)
        {
            var row = new object[ColumnCount];
            var columns = AllColumns().Select(c => c.Enumerate().GetEnumerator()).ToArray();
            var rowCount = Math.Min(maxRows, RowCount);

            for (uint i = 0; i < rowCount; i++) {
                for (uint j = 0; j < ColumnCount; j++) {
                    var column = columns[j];
                    column.MoveNext();
                    row[j] = column.Current;
                }
                callback(row, i);
            }
        }

        public IEnumerable<IMetaData> ColumnMetaData(params uint[] columnIndices) => AllOrSpecifiedColumns(columnIndices).Select(i => _columns[i].Info.MetaData);

        public IRowOrientedDataTable AsRowOriented(string filePath = null)
        {
            using var builder = new RowOrientedTableBuilder(RowCount, filePath);
            foreach (var (info, _) in _columns)
                builder.AddColumn(info.ColumnType, info.MetaData);

            // ReSharper disable once AccessToDisposedClosure
            ForEachRow((row, index) => builder.AddRow(row));

            return builder.Build(Context);
        }

        public IColumnOrientedDataTable Convert(params ColumnConversion[] conversionParams)
        {
            return _Transform(conversionParams, null);
        }

        IColumnOrientedDataTable _Transform(IEnumerable<IColumnTransformationParam> input, string filePath)
        {
            using var tempStream = new TempStreamManager();
            var columnConversionTable = new Dictionary<uint, IColumnTransformationParam>();

            uint nextIndex = 0;
            foreach (var item in input) {
                if (item.ColumnIndex.HasValue && item.ColumnIndex.Value < ColumnCount) {
                    columnConversionTable[item.ColumnIndex.Value] = item;
                    nextIndex = item.ColumnIndex.Value + 1;
                } else if (nextIndex < ColumnCount)
                    columnConversionTable[nextIndex++] = item;
            }

            // TODO: return normalization parameters (from INormalize)
            var columnConversions = new Dictionary<ISingleTypeTableSegment, IColumnTransformation>();
            foreach (var (info, segment) in _columns) {
                if (columnConversionTable.TryGetValue(info.Index, out var conversion)) {
                    var column = _columns[info.Index].Segment;
                    var converter = conversion.GetConverter(info.ColumnType, column, tempStream);
                    if (converter != null) {
                        var newColumnInfo = info.ChangeColumnType(converter.To.GetColumnType());
                        var buffer = newColumnInfo.MetaData.GetGrowableSegment(newColumnInfo.ColumnType, Context, tempStream);
                        var contextType = typeof(TransformationContext<,>).MakeGenericType(converter.From, converter.To);
                        var param = new object[] { column, converter, buffer };
                        var conversionContext = (IColumnTransformation)Activator.CreateInstance(contextType, param);
                        columnConversions.Add(segment, conversionContext);
                    }
                }
            }

            var convertedColumns = new List<ISingleTypeTableSegment>();
            for (uint i = 0; i < ColumnCount; i++) {
                var wasConverted = false;
                var column = _columns[i].Segment;
                if (columnConversions.TryGetValue(column, out var converter)) {
                    if (converter.Transform() == RowCount) {
                        convertedColumns.Add((ISingleTypeTableSegment)converter.Buffer);
                        wasConverted = true;
                    }
                }
                if (!wasConverted)
                    convertedColumns.Add(column);
            }

            return convertedColumns.BuildColumnOrientedTable(Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable Convert(string filePath, params ColumnConversion[] conversionParams)
        {
            return _Transform(conversionParams, filePath);
        }

        public IColumnOrientedDataTable SelectColumns(params uint[] columnIndices) => SelectColumns(null, columnIndices);
        public IColumnOrientedDataTable SelectColumns(string filePath, params uint[] columnIndices)
        {
            return Columns(columnIndices).ToList().BuildColumnOrientedTable(Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable Normalize(NormalizationType type, string filePath = null)
        {
            if (type == NormalizationType.None)
                return this;
            var param = _columns
                .Select(c => c.Info)
                .Where(c => c.ColumnType.IsDecimal())
                .Select(c => new ColumnNormalization(c.Index, type));
            return _Transform(param, filePath);
        }

        public IColumnOrientedDataTable Normalize(params ColumnNormalization[] param)
        {
            return _Transform(param, null);
        }

        public IColumnOrientedDataTable Normalize(string filePath, params ColumnNormalization[] param)
        {
            return _Transform(param, filePath);
        }

        public IColumnOrientedDataTable ConcatColumns(params IColumnOrientedDataTable[] others) => ConcatColumns(null, others);
        public IColumnOrientedDataTable ConcatColumns(string filePath, params IColumnOrientedDataTable[] others)
        {
            if (others.Any(t => t.RowCount != RowCount))
                throw new ArgumentException("Row count across tables must agree");

            var columns = ColumnCount.AsRange().Select(i => _columns[i].Segment);
            foreach (var other in others)
                columns = columns.Concat(other.ColumnCount.AsRange().Select(i => other.Column(i)));

            return columns.ToList().BuildColumnOrientedTable(Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable FilterRows(Predicate<object[]> predicate, string filePath = null)
        {
            using var tempStream = new TempStreamManager();
            var buffers = ColumnCount.AsRange()
                .Select(i => _columns[i].Info.MetaData.GetGrowableSegment(_columns[i].Info.ColumnType, Context, tempStream))
                .ToList();

            uint rowCount = 0;
            ForEachRow((row, index) => {
                if (predicate(row)) {
                    ++rowCount;
                    for (int i = 0; i < ColumnCount; i++)
                        buffers[i].Add(row[i]);
                }
            });

            return buffers.BuildColumnOrientedTable(Context, rowCount, filePath);
        }

        public IColumnOrientedDataTable ReinterpretColumns(params ReinterpretColumns[] columns) => ReinterpretColumns(null, columns);
        public IColumnOrientedDataTable ReinterpretColumns(string filePath, params ReinterpretColumns[] columns)
        {
            using var tempStream = new TempStreamManager();
            var newColumns = new List<ISingleTypeTableSegment>();
            var reinterpreted = columns.SelectMany(c => c.ColumnIndices.Select(i => (Column: c, Index: i)))
                .ToDictionary(d => d.Index, d => d.Column);

            foreach (var column in _columns)
            {
                if (reinterpreted.TryGetValue(column.Info.Index, out var rc))
                {
                    if (column.Info.Index == rc.ColumnIndices[0])
                        newColumns.AddRange(rc.GetNewColumns(Context, tempStream, (uint)newColumns.Count, rc.ColumnIndices.Select(i => _columns[i]).ToArray()));
                }else
                    newColumns.Add(column.Segment);
            }

            return newColumns.BuildColumnOrientedTable(Context, RowCount, filePath);
        }

        public override string ToString() => String.Join(", ", _columns.Select(c => c.ToString()));
    }
}
