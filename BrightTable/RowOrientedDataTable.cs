using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Builders;
using BrightTable.Input;
using BrightTable.Segments;

namespace BrightTable
{
    class RowOrientedDataTable : DataTableBase, IRowOrientedDataTable
    {
        readonly ColumnInfo[] _columns;
        readonly ColumnType[] _columnTypes;
        readonly uint[] _rowOffset;
        readonly InputData _data;

        public RowOrientedDataTable(IBrightDataContext context, InputData data, bool readHeader) : base(context)
        {
            _data = data;
            var reader = data.Reader;

            if (readHeader) {
                var version = reader.ReadInt32();
                if (version > Consts.DataTableVersion)
                    throw new Exception($"Data table version {version} exceeds {Consts.DataTableVersion}");
                var orientation = (DataTableOrientation) reader.ReadInt32();
                if (orientation != DataTableOrientation.RowOriented)
                    throw new Exception("Invalid orientation");
            }

            var numColumns = reader.ReadUInt32();
            _columns = new ColumnInfo[numColumns];
            for (uint i = 0; i < numColumns; i++)
                _columns[i] = new ColumnInfo(reader, i);
            _columnTypes = _columns.Select(c => c.ColumnType).ToArray();

            uint rowCount = reader.ReadUInt32();
            _rowOffset = new uint[rowCount];
            for (uint i = 0; i < rowCount; i++)
                _rowOffset[i] = reader.ReadUInt32();

            RowCount = (uint)_rowOffset.Length;
            ColumnCount = (uint)_columns.Length;
        }

        public void Dispose()
        {
            _data.Dispose();
        }

        public DataTableOrientation Orientation => DataTableOrientation.RowOriented;
        public IReadOnlyList<ColumnType> ColumnTypes => _columnTypes;

        public IReadOnlyList<IDataTableSegment> Rows(params uint[] rowIndices)
        {
            var ret = new List<IDataTableSegment>();
            if (rowIndices.Any()) {
                ForEachRow(rowIndices, row => ret.Add(new Row(_columnTypes, row)));
            }

            return ret;
        }

        public IDataTableSegment Row(uint rowIndex) => Rows(rowIndex).Single();

        public void ForEachRow(IEnumerable<uint> rowIndices, Action<object[]> callback)
        {
            lock (_data) {
                foreach (var index in rowIndices) {
                    _data.MoveTo(_rowOffset[index]);
                    var row = new object[_columns.Length];
                    var ind = 0;
                    foreach (var columnType in _columnTypes)
                        row[ind++] = _Read(columnType, _data.Reader);

                    callback(row);
                }
            }
        }

        public ISingleTypeTableSegment Column(uint columnIndex)
        {
            return Columns(columnIndex).Single();
        }

        public IReadOnlyList<ISingleTypeTableSegment> Columns(params uint[] columnIndices)
        {
            // TODO: optionally compress the columns based on unique count statistics
            var columns = columnIndices.Select(i => (Index: i, Column:_GetColumn(_columnTypes[i], _columns[i].MetaData))).ToList();
            if (columns.Any()) {
                // set the column metadata
                columns.ForEach(item => {
                    var metadata = item.Column.Segment.MetaData;
                    var column = _columns[item.Index];
                    column.MetaData.CopyTo(metadata);
                });

                // copy the column data
                ForEachRow((row, index) => {
                    foreach (var column in columns)
                        column.Column.Buffer.Set(index, row[column.Index]);
                });
            }

            return columns.Select(c => c.Column.Segment).ToList();
        }

        public IReadOnlyList<IMetaData> ColumnMetaData(params uint[] columnIndices)
        {
            return columnIndices.Select(i => _columns[i].MetaData).ToList().AsReadOnly();
        }

        public IRowOrientedDataTable AsRowOriented(string filePath = null)
        {
            using (var builder = new RowOrientedTableBuilder(RowCount, filePath)) {
                foreach (var column in _columns)
                    builder.AddColumn(column.ColumnType, column.MetaData);

                // ReSharper disable once AccessToDisposedClosure
                ForEachRow((row, index) => builder.AddRow(row));

                return builder.Build(Context);
            }
        }

        public void ForEachRow(Action<object[], uint> callback)
        {
            lock (_data) {
                _data.MoveTo(_rowOffset[0]);
                var row = new object[_columns.Length];
                for(uint i = 0; i < RowCount; i++) {
                    var ind = 0;
                    foreach (var columnType in _columnTypes)
                        row[ind++] = _Read(columnType, _data.Reader);
                    callback(row, i);
                }
            }
        }

        public IColumnOrientedDataTable AsColumnOriented(string filePath = null)
        {
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            using var builder = new ColumnOrientedTableBuilder(filePath);

            builder.WriteHeader(ColumnCount, RowCount);
            var columns = Columns(Enumerable.Range(0, _columns.Length).Select(i => (uint)i).ToArray());
            foreach(var column in columns) {
                var position = builder.Write(column);
                columnOffsets.Add((position, builder.GetCurrentPosition()));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(Context);
        }

        public IReadOnlyList<IDataTableSegment> Head => Rows(ExtensionMethods.Range(0, Math.Min(PREVIEW_SIZE, RowCount)).ToArray());
        public IReadOnlyList<IDataTableSegment> Tail => Rows(ExtensionMethods.Range(Math.Max(0, RowCount - PREVIEW_SIZE), Math.Min(PREVIEW_SIZE, RowCount)).ToArray());

        (ISingleTypeTableSegment Segment, IEditableBuffer Buffer) _GetColumn(ColumnType columnType, IMetaData metadata)
        {
            var type = typeof(DataSegmentBuffer<>).MakeGenericType(columnType.GetColumnType());
            var ret = Activator.CreateInstance(type, Context, columnType, metadata, RowCount);
            return ((ISingleTypeTableSegment) ret, (IEditableBuffer) ret);
        }

        object _Read(ColumnType type, BinaryReader reader)
		{
			switch (type) {
				case ColumnType.String:
					return reader.ReadString();
				case ColumnType.Double:
					return reader.ReadDouble();
				case ColumnType.Decimal:
					return reader.ReadDecimal();
				case ColumnType.Int:
					return reader.ReadInt32();
				case ColumnType.Short:
					return reader.ReadInt16();
				case ColumnType.Float:
					return reader.ReadSingle();
				case ColumnType.Boolean:
					return reader.ReadBoolean();
				case ColumnType.Date:
					return new DateTime(reader.ReadInt64());
				case ColumnType.Long:
					return reader.ReadInt64();
				case ColumnType.Byte:
					return reader.ReadSByte();
				case ColumnType.IndexList:
					return IndexList.ReadFrom(Context, reader);
				case ColumnType.WeightedIndexList:
					return WeightedIndexList.ReadFrom(Context, reader);
				case ColumnType.Vector:
					return new Vector<float>(Context, reader);
				case ColumnType.Matrix:
					return new Matrix<float>(Context, reader);
				case ColumnType.Tensor3D:
					return new Tensor3D<float>(Context, reader);
				case ColumnType.Tensor4D:
					return new Tensor4D<float>(Context, reader);
                case ColumnType.BinaryData:
                    return new BinaryData(reader);
				default:
					return null;
			}
		}
    }
}
