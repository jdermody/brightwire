using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.ConstraintValidation;
using BrightData.LinearAlgebra.ReadOnly;
using static BrightData.DataTable.TableBase;

namespace BrightData.DataTable
{
    internal class ColumnOrientedDataTableBuilder : IBuildDataTables
    {
        readonly int                   _blockSize;
        readonly uint?                 _maxInMemoryBlocks;
        readonly IProvideTempData?      _tempData;
        readonly List<ICompositeBuffer> _columns = new();

        public ColumnOrientedDataTableBuilder(
            BrightDataContext context, 
            IProvideTempData? tempData = null,
            int blockSize = Consts.DefaultBlockSize, 
            uint? maxInMemoryBlocks = Consts.DefaultMaxBlocksInMemory)
        {
            Context            = context;
            _tempData          = tempData;
            _blockSize         = blockSize;
            _maxInMemoryBlocks = maxInMemoryBlocks;
        }

        public MetaData TableMetaData { get; } = new();
        public BrightDataContext Context { get; }

        internal static string DefaultColumnName(string? name, int numColumns)
        {
            return String.IsNullOrWhiteSpace(name) ? $"Column {numColumns + 1}" : name;
        }

        public ICompositeBuffer AddColumn(BrightDataType type, string? name = null)
        {
            var columnMetaData = new MetaData();
            columnMetaData.Set(Consts.Name, DefaultColumnName(name, _columns.Count));
            columnMetaData.Set(Consts.Type, type);
            columnMetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            if (type.IsNumeric())
                columnMetaData.Set(Consts.IsNumeric, true);

            return AddColumn(type, columnMetaData);
        }

        public ICompositeBuffer AddColumn(BrightDataType type, MetaData metaData)
        {
            var buffer = type.CreateCompositeBuffer(_tempData, _blockSize, _maxInMemoryBlocks);
            buffer.MetaData.Set(Consts.ColumnIndex, (uint)_columns.Count);
            _columns.Add(buffer);
            return buffer;
        }

        public ICompositeBuffer[] AddColumnsFrom(IDataTable table, params uint[] columnIndices)
        {
            var columnSet = new HashSet<uint>(table.AllOrSpecifiedColumnIndices(false, columnIndices));
            var columnTypes = table.ColumnTypes.Zip(table.ColumnMetaData, (t, m) => (Type: t, MetaData: m))
                .Select((c, i) => (Column: c, Index: (uint) i));

            var wantedColumnTypes = columnTypes
                .Where(c => columnSet.Contains(c.Index))
                .Select(c => c.Column)
                .ToList()
            ;

            var index = 0;
            var ret = new ICompositeBuffer[wantedColumnTypes.Count];
            foreach (var column in wantedColumnTypes)
                ret[index++] = AddColumn(column.Type, column.MetaData);
            return ret;
        }

        public ICompositeBuffer<T> AddColumn<T>(string? name = null)
            where T : notnull
        {
            var type = typeof(T).GetBrightDataType();
            return (ICompositeBuffer<T>)AddColumn(type, name);
        }

        public void AddRow(params object[] items)
        {
            for(int i = 0, len = items.Length; i < len; i++)
                _columns[i].AddObject(items[i]);
        }

        public Task WriteTo(Stream stream)
        {
            var writer = new ColumnOrientedDataTableWriter(Context, _tempData, _blockSize, _maxInMemoryBlocks);
            return writer.Write(
                TableMetaData,
                _columns.Cast<IReadOnlyBufferWithMetaData>().ToArray(),
                stream
            );
        }

        public ICompositeBuffer<ReadOnlyVector> AddFixedSizeVectorColumn(uint size, string? name)
        {
            var ret = AddColumn<ReadOnlyVector>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyVector>((in ReadOnlyVector tensor) => tensor.Size == size);
            return ret;
        }

        
        public ICompositeBuffer<ReadOnlyMatrix> AddFixedSizeMatrixColumn(uint rows, uint columns, string? name)
        {
            var ret = AddColumn<ReadOnlyMatrix>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyMatrix>((in ReadOnlyMatrix tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
            );
            return ret;
        }

        public ICompositeBuffer<ReadOnlyTensor3D> AddFixedSize3DTensorColumn(uint depth, uint rows, uint columns, string? name)
        {
            var ret = AddColumn<ReadOnlyTensor3D>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyTensor3D>((in ReadOnlyTensor3D tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
                && tensor.Depth == depth
            );
            return ret;
        }

        public ICompositeBuffer<ReadOnlyTensor4D> AddFixedSize4DTensorColumn(uint count, uint depth, uint rows, uint columns, string? name)
        {
            var ret = AddColumn<ReadOnlyTensor4D>(name);
            ret.ConstraintValidator = new ThrowOnInvalidConstraint<ReadOnlyTensor4D>((in ReadOnlyTensor4D tensor) => 
                tensor.RowCount == rows 
                && tensor.ColumnCount == columns 
                && tensor.Depth == depth 
                && tensor.Count == count
            );
            return ret;
        }
    }
}
