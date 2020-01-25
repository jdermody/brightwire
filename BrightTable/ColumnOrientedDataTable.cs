using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightTable.Buffers;
using BrightTable.Builders;
using BrightTable.Segments;
using BrightTable.Input;
using BrightTable.Transformations;
using BrightTable.Transformations.Conversions;

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
        readonly ColumnType[] _columnTypes;
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
            _columnTypes = new ColumnType[ColumnCount];
            for (uint i = 0; i < ColumnCount; i++) {
                var nextColumnPosition = reader.ReadInt64();
                _columns[i] = new Column(i, reader);
                _columnTypes[i] = _columns[i].ColumnType;
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
        public IReadOnlyList<ColumnType> ColumnTypes => _columnTypes;

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

        //public IDataTableSegment Row(uint rowIndex)
        //{
        //    Row ret = null;
        //    ForEachRow((row, index) => {
        //        if(rowIndex == index)
        //            ret = new Row(_columnTypes, row);
        //    });
        //    return ret;
        //}

        //public IReadOnlyList<IDataTableSegment> Rows(params uint[] rowIndices)
        //{
        //    var ret = new List<IDataTableSegment>();
        //    if (rowIndices.Any()) {
        //        var rowSet = new HashSet<uint>(rowIndices);
        //        ForEachRow((row, index) => {
        //            if(rowSet.Contains(index))
        //                ret.Add(new Row(_columnTypes, row));
        //        });
        //    }

        //    return ret;
        //}

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

        public IColumnOrientedDataTable Convert(params DataConversionParam[] conversionParams)
        {
            return Convert(null, conversionParams);
        }

        public IColumnOrientedDataTable Convert(string filePath, params DataConversionParam[] conversionParams)
        {
            using var tempStream = new TempStreamManager();
            var conversions = conversionParams
                .Zip(_columns, (c, columnInfo) => {
                    var column = _GetColumn(columnInfo.Index);
                    var converter = c.GetConverter(columnInfo.ColumnType, column, tempStream, Context);
                    var newColumnInfo = columnInfo.ChangeColumnType(converter.To.GetColumnType());
                    return (
                        Converter: converter,
                        Buffer: newColumnInfo.GetGrowableSegment(Context, tempStream),
                        Column: column
                    );
                })
                .Select((d, i) => {
                    var contextType = typeof(ConversionContext<,>).MakeGenericType(d.Converter.From, d.Converter.To);
                    var param = new object[] { d.Column, d.Converter, d.Buffer };
                    var converter = Activator.CreateInstance(contextType, param);
                    return (IColumnConversion)converter;
                })
                .ToList();

            var rowCount = conversions.Select(c => c.Convert()).Min();
            return conversions.Select(c => c.Buffer).ToList().BuildColumnOrientedTable(Context, rowCount, filePath);
            //var conversion = new ColumnConversionTransformation(this, conversionParams);
            //return conversion.Transform(this, filePath);
        }

        public IColumnOrientedDataTable SelectColumns(params uint[] columnIndices)
        {
            return SelectColumns(null, columnIndices);
        }

        public IColumnOrientedDataTable SelectColumns(string filePath, params uint[] columnIndices)
        {
            var transform = new ColumnSubsetTransformation(columnIndices);
            return transform.Transform(this, filePath);
        }

        public IColumnOrientedDataTable Normalise(NormalizationType type, string filePath = null)
        {
            var t = new NormalisationTransformation(type);
            return t.Transform(this, filePath);
        }

        public IColumnOrientedDataTable OneHotEncode(params uint[] columnIndices) => OneHotEncode(true, columnIndices);
        public IColumnOrientedDataTable OneHotEncode(bool writeToMetadata, params uint[] columnIndices) => OneHotEncode(null, writeToMetadata, columnIndices);
        public IColumnOrientedDataTable OneHotEncode(string filePath, bool writeToMetadata, params uint[] columnIndices)
        {
            var t = new OneHotEncodeCategoricalColumnsTransformation(writeToMetadata, columnIndices);
            return t.Transform(this, filePath);
        }

        public IMutateColumns CreateMutateContext() => new ProjectColumnsTransformation(this);

        public IColumnOrientedDataTable Concat(params IColumnOrientedDataTable[] others) => Concat(null, others);
        public IColumnOrientedDataTable Concat(string filePath, params IColumnOrientedDataTable[] others)
        {
            var t = new ZipTablesTransformation(new[] { this }.Concat(others).ToArray());
            return t.Transform(Context, filePath);
        }

        public IColumnOrientedDataTable FilterRows(Predicate<object[]> predicate, string filePath = null)
        {
            using var tempStream = new TempStreamManager();
            var buffers = ExtensionMethods.Range(0, ColumnCount)
                .Select(i => _columns[i].GetGrowableSegment(Context, tempStream))
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
