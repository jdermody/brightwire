using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.DataTable.Builders
{
    /// <summary>
    /// Builds tables dynamically in memory
    /// </summary>
    public class InMemoryTableBuilder : IHaveDataContext
    {
        readonly List<(BrightDataType Type, IMetaData MetaData)> _columns = new();
        readonly List<object[]> _rows = new();

        /// <inheritdoc />
        public IBrightDataContext Context { get; }

        /// <summary>
        /// Table meta data
        /// </summary>
        public IMetaData MetaData { get; } = new MetaData();

        internal InMemoryTableBuilder(IBrightDataContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Copies column definitions from an existing table
        /// </summary>
        /// <param name="table">Table to copy from</param>
        /// <param name="columnIndices">Column indices to copy</param>
        public void CopyColumnsFrom(IDataTable table, params uint[] columnIndices)
        {
            var columnSet = new HashSet<uint>(table.AllOrSelectedColumnIndices(columnIndices));
            var columnTypes = table.ColumnTypes.Zip(table.AllColumnsMetaData(), (t, m) => (Type: t, MetaData: m))
                .Select((c, i) => (Column: c, Index: (uint) i));

            var wantedColumnTypes = columnTypes
                .Where(c => columnSet.Contains(c.Index))
                .Select(c => c.Column);

            foreach (var column in wantedColumnTypes)
                _columns.Add(column);
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="type">Type of the column</param>
        /// <param name="name">Name of the column</param>
        /// <returns></returns>
        public IMetaData AddColumn(BrightDataType type, string? name = null)
        {
            var ret = new MetaData();
            ret.Set(Consts.Name, DataTableBase.DefaultColumnName(name, _columns.Count));
            _columns.Add((type, ret));
            return ret;
        }

        /// <summary>
        /// Adds a new fixed size vector column
        /// </summary>
        /// <param name="size">Size of the vector</param>
        /// <param name="name">Name of the column (optional)</param>
        /// <returns></returns>
        public IMetaData AddFixedSizeVectorColumn(uint size, string? name = null)
        {
            var metaData = AddColumn(BrightDataType.Vector, name);
            metaData.Set(Consts.XDimension, size);
            return metaData;
        }

        /// <summary>
        /// Adds a new fixed size matrix column
        /// </summary>
        /// <param name="rows">Row count of each matrix</param>
        /// <param name="columns">Column count of each matrix</param>
        /// <param name="name">Name of the column (optional)</param>
        /// <returns></returns>
        public IMetaData AddFixedSizeMatrixColumn(uint rows, uint columns, string? name = null)
        {
            var metaData = AddColumn(BrightDataType.Matrix, name);
            metaData.Set(Consts.XDimension, columns);
            metaData.Set(Consts.YDimension, rows);
            return metaData;
        }

        /// <summary>
        /// Adds a new fixed size 3D tensor column
        /// </summary>
        /// <param name="depth">Depth of each 3D tensor</param>
        /// <param name="rows">Row count of each matrix</param>
        /// <param name="columns">Column count of each matrix</param>
        /// <param name="name">Name of the column (optional)</param>
        /// <returns></returns>
        public IMetaData AddFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string? name = null)
        {
            var metaData = AddColumn(BrightDataType.Tensor3D, name);
            metaData.Set(Consts.XDimension, columns);
            metaData.Set(Consts.YDimension, rows);
            metaData.Set(Consts.ZDimension, depth);
            return metaData;
        }

        /// <summary>
        /// Adds a new row
        /// </summary>
        /// <param name="data"></param>
        public void AddRow(params object[] data)
        {
            if (data.Length != _columns.Count)
                throw new ArgumentException($"{data.Length} columns but needed {_columns.Count}");
            for (int i = 0, len = data.Length; i < len; i++)
                data[i] = EnsureType(i, data[i], _columns[i].Type);
            _rows.Add(data);
        }

        object EnsureType(int columnIndex, object val, BrightDataType type)
        {
            return type switch {
                BrightDataType.Matrix => EnsureMatrix(columnIndex, (Matrix<float>) val),
                BrightDataType.BinaryData => (BinaryData) val,
                BrightDataType.Boolean => Convert.ToBoolean(val),
                BrightDataType.SByte => Convert.ToSByte(val),
                BrightDataType.Date => Convert.ToDateTime(val),
                BrightDataType.Decimal => Convert.ToDecimal(val),
                BrightDataType.Short => Convert.ToInt16(val),
                BrightDataType.Int => Convert.ToInt32(val),
                BrightDataType.Long => Convert.ToInt64(val),
                BrightDataType.Float => Convert.ToSingle(val),
                BrightDataType.Double => Convert.ToDouble(val),
                BrightDataType.String => val.ToString(),
                BrightDataType.IndexList => (IndexList) val,
                BrightDataType.WeightedIndexList => (WeightedIndexList) val,
                BrightDataType.Vector => EnsureVector(columnIndex, (Vector<float>)val),
                BrightDataType.Tensor3D => EnsureTensor(columnIndex, (Tensor3D<float>) val),
                BrightDataType.Tensor4D => (Tensor4D<float>) val,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            } ?? throw new ArgumentException("Value cannot be null");
        }

        object EnsureVector(int columnIndex, Vector<float> vector)
        {
            var metaData = _columns[columnIndex].MetaData;
            if (metaData.Has(Consts.XDimension)) {
                var expectedSize = metaData.Get<uint>(Consts.XDimension);
                if (expectedSize != vector.Size)
                    throw new ArgumentException($"For column {columnIndex}, expected vector of size {expectedSize} but found vector of size {vector.Size}");
            }
            return vector;
        }

        object EnsureMatrix(int columnIndex, Matrix<float> matrix)
        {
            var metaData = _columns[columnIndex].MetaData;
            if (metaData.Has(Consts.XDimension) && metaData.Has(Consts.YDimension)) {
                var expectedRows = metaData.Get<uint>(Consts.YDimension);
                var expectedColumns = metaData.Get<uint>(Consts.XDimension);
                if (expectedRows != matrix.RowCount || expectedColumns != matrix.ColumnCount)
                    throw new ArgumentException($"For column {columnIndex}, expected a matrix with {expectedRows} rows and {expectedColumns} columns but found a matrix with {matrix.RowCount} rows and {matrix.ColumnCount} columns");
            }
            return matrix;
        }

        object EnsureTensor(int columnIndex, Tensor3D<float> tensor)
        {
            var metaData = _columns[columnIndex].MetaData;
            if (metaData.Has(Consts.XDimension) && metaData.Has(Consts.YDimension) && metaData.Has(Consts.ZDimension)) {
                var expectedRows = metaData.Get<uint>(Consts.YDimension);
                var expectedColumns = metaData.Get<uint>(Consts.XDimension);
                var expectedDepth = metaData.Get<uint>(Consts.ZDimension);
                if (expectedRows != tensor.RowCount || expectedColumns != tensor.ColumnCount || expectedDepth != tensor.Depth)
                    throw new ArgumentException($"For column {columnIndex}, expected a 3D tensor with {expectedRows} rows, {expectedColumns} columns and {expectedDepth} depth but found a tensor with {tensor.RowCount} rows, {tensor.ColumnCount} columns and {tensor.Depth} depth");
            }
            return tensor;
        }

        /// <summary>
        /// Creates a row oriented table
        /// </summary>
        /// <returns></returns>
        public IRowOrientedDataTable BuildRowOriented()
        {
            using var builder = new RowOrientedTableBuilder(MetaData, (uint)_rows.Count);
            foreach (var column in _columns)
                builder.AddColumn(column.Type, column.MetaData);
            foreach(var row in _rows)
                builder.AddRow(row);
            return builder.Build(Context);
        }

        /// <summary>
        /// Creates a column oriented table
        /// </summary>
        /// <returns></returns>
        public IColumnOrientedDataTable BuildColumnOriented()
        {
            using var builder = new ColumnOrientedTableBuilder();
            builder.WriteHeader((uint)_columns.Count, (uint)_rows.Count, MetaData);

            var tempStream = new TempStreamManager();
            var columns = _columns.Select(c => c.MetaData.GetGrowableSegment(c.Type, Context, tempStream)).ToList();
            uint index = 0;
            foreach (var row in _rows) {
                for (var i = 0; i < _columns.Count; i++) {
                    var val = i < row.Length 
                        ? row[i] 
                        : _columns[i].Type.GetDefaultValue();
                    if (val == null)
                        throw new Exception("Values cannot be null");
                    columns[i].Add(val, index);
                }

                ++index;
            }

            var segments = columns.Cast<ISingleTypeTableSegment>().ToArray();
            var columnOffsets = new List<(long Position, long EndOfColumnOffset)>();
            foreach (var segment in segments)
            {
                var position = builder.Write(segment);
                columnOffsets.Add((position, builder.GetCurrentPosition()));
            }
            builder.WriteColumnOffsets(columnOffsets);
            return builder.Build(Context);
        }
    }
}
