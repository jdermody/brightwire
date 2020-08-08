using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightData.Transformation;
using BrightTable.Buffers;
using BrightTable.Builders;
using BrightTable.Segments;
using BrightTable.Input;
using BrightTable.Transformations;

namespace BrightTable
{
    class ColumnOrientedDataTable : DataTableBase, IColumnOrientedDataTable
    {
        internal enum ColumnFamily
        {
            Normal,
            Encoded,
            Struct
        }

        class Column : IColumnInfo
        {
            readonly MetaData _metadata;

            public Column(uint index, BinaryReader reader)
            {
                Index = index;
                ColumnType = (ColumnType)reader.ReadByte();
                _metadata = new MetaData(reader);
                IsEncoded = reader.ReadBoolean();
            }

            public uint Index { get; }
            public ColumnType ColumnType { get; }
            public IMetaData MetaData => _metadata;
            public bool IsEncoded { get; }

            public ISingleTypeTableSegment Load(IBrightDataContext context, InputData data, long columnOffset, uint rowCount)
            {
                var dataType = ColumnType.GetColumnType();
                var buffer = new InputBufferReader(data, columnOffset, rowCount);

                if (IsEncoded) {
                    return (ISingleTypeTableSegment)Activator.CreateInstance(typeof(EncodedColumn<>).MakeGenericType(dataType),
                        context,
                        buffer,
                        ColumnType,
                        MetaData
                    );
                } else if (ColumnType.IsStructable()) {
                    return (ISingleTypeTableSegment)Activator.CreateInstance(typeof(StructColumn<>).MakeGenericType(dataType),
                        context,
                        buffer,
                        ColumnType,
                        MetaData
                    );
                }

                // default column type is non-struct
                return (ISingleTypeTableSegment)Activator.CreateInstance(typeof(Column<>).MakeGenericType(dataType),
                    context,
                    buffer,
                    ColumnType,
                    MetaData
                );
            }

            public override string ToString()
            {
                return $"Encoded: {IsEncoded}, {MetaData}";
            }
        }

        readonly object _lock = new object();
        readonly InputData _data;
        readonly long[] _columnOffset;
        readonly Column[] _columns;
        readonly Dictionary<uint, ISingleTypeTableSegment> _loadedColumns = new Dictionary<uint, ISingleTypeTableSegment>();

        public ColumnOrientedDataTable(IBrightDataContext context, InputData data, bool readHeader) : base(context)
        {
            _data = data;
            var reader = data.Reader;
            if (readHeader) {
                var version = reader.ReadInt32();
                if (version > Consts.DataTableVersion)
                    throw new Exception($"Data table version {version} exceeds {Consts.DataTableVersion}");
                var orientation = (DataTableOrientation)reader.ReadByte();
                if (orientation != DataTableOrientation.ColumnOriented)
                    throw new Exception("Invalid orientation");
            }
            ColumnCount = reader.ReadUInt32();
            RowCount = reader.ReadUInt32();

            _columnOffset = new long[ColumnCount];
            _columns = new Column[ColumnCount];
            ColumnTypes = new ColumnType[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++) {
                var nextColumnPosition = reader.ReadInt64();
                _columns[i] = new Column(i, reader);
                ColumnTypes[i] = _columns[i].ColumnType;
                _columnOffset[i] = _data.Position;
                _data.MoveTo(nextColumnPosition);
            }
        }

        public void Dispose()
        {
            foreach (var item in _loadedColumns)
                item.Value.Dispose();
            _data.Dispose();
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
                ret[i] = _GetColumn(i);
            return ret;
        }

        ISingleTypeTableSegment IColumnOrientedDataTable.Column(uint columnIndex)
        {
            return _GetColumn(columnIndex);
        }

        public IReadOnlyList<ISingleTypeTableSegment> Columns(params uint[] columnIndices)
        {
            var table = new Dictionary<uint, ISingleTypeTableSegment>();
            foreach (var index in columnIndices.OrderBy(i => i).Distinct())
                table.Add(index, _GetColumn(index));
            return columnIndices.Select(i => table[i]).ToList();
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

        public IReadOnlyList<IMetaData> ColumnMetaData(params uint[] columnIndices) => columnIndices.Select(i => _columns[i].MetaData).ToList();

        public IRowOrientedDataTable AsRowOriented(string filePath = null)
        {
            using var builder = new RowOrientedTableBuilder(RowCount, filePath);
            foreach (var column in _columns)
                builder.AddColumn(column.ColumnType, column.MetaData);

            // ReSharper disable once AccessToDisposedClosure
            ForEachRow((row, index) => builder.AddRow(row));

            return builder.Build(Context);
        }

        ISingleTypeTableSegment _GetColumn(uint index)
        {
            if (_loadedColumns.TryGetValue(index, out var ret))
                return ret;

            lock (_lock) {
                if (_loadedColumns.TryGetValue(index, out ret))
                    return ret;

                var column = _columns[index];
                var data = _data.Clone();
                var offset = _columnOffset[index];
                data.MoveTo(_columnOffset[index]);
                _loadedColumns.Add(index, ret = column.Load(Context, data, offset, RowCount));
                return ret;
            }
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
                if (item.Index.HasValue && item.Index.Value < ColumnCount) {
                    columnConversionTable[item.Index.Value] = item;
                    nextIndex = item.Index.Value + 1;
                } else if (nextIndex < ColumnCount)
                    columnConversionTable[nextIndex++] = item;
            }

            var columnConversions = new Dictionary<uint, IColumnTransformation>();
            foreach (var columnInfo in _columns) {
                if (columnConversionTable.TryGetValue(columnInfo.Index, out var conversion)) {
                    var column = _GetColumn(columnInfo.Index);
                    var converter = conversion.GetConverter(columnInfo.ColumnType, column, tempStream, Context);
                    if (converter != null) {
                        var newColumnInfo = columnInfo.ChangeColumnType(converter.To.GetColumnType());
                        var buffer = newColumnInfo.GetGrowableSegment(Context, tempStream, false);
                        var contextType = typeof(TransformationContext<,>).MakeGenericType(converter.From, converter.To);
                        var param = new object[] { column, converter, buffer };
                        var conversionContext = (IColumnTransformation)Activator.CreateInstance(contextType, param);
                        columnConversions.Add(columnInfo.Index, conversionContext);
                    }
                }
            }

            var convertedColumns = new List<ISingleTypeTableSegment>();
            for (uint i = 0; i < ColumnCount; i++) {
                var wasConverted = false;
                var column = _GetColumn(i);
                if (columnConversions.TryGetValue(i, out var converter)) {
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
            var columns = Columns(columnIndices);
            return columns.BuildColumnOrientedTable(Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable Normalize(NormalizationType type, string filePath = null)
        {
            var param = _columns.Where(c => c.ColumnType.IsDecimal()).Select(c => new ColumnNormalization(c.Index, type));
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

            var columns = ExtensionMethods.Range(0, ColumnCount).Select(_GetColumn);
            foreach (var other in others)
                columns = columns.Concat(ExtensionMethods.Range(0, other.ColumnCount).Select(i => other.Column(i)));

            return columns.ToList().BuildColumnOrientedTable(Context, RowCount, filePath);
        }

        public IColumnOrientedDataTable FilterRows(Predicate<object[]> predicate, string filePath = null)
        {
            using var tempStream = new TempStreamManager();
            var buffers = ExtensionMethods.Range(0, ColumnCount)
                .Select(i => _columns[i].GetGrowableSegment(Context, tempStream, false))
                .ToArray();

            uint rowCount = 0;
            ForEachRow((row, index) => {
                if (predicate(row)) {
                    ++rowCount;
                    for (uint i = 0; i < ColumnCount; i++)
                        buffers[i].Add(row[i]);
                }
            });

            return buffers.BuildColumnOrientedTable(Context, rowCount, filePath);
        }
    }
}
