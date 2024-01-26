using System.Collections.Generic;
using System.Linq;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    internal abstract class TypedRowBasedDataTableAdapterBase<T1, T2> : DataTableAdapterBase<TableRow<T1, T2>>
        where T1 : IHaveSize
        where T2 : IHaveSize
    {
        protected readonly uint[] _featureColumns;
        protected readonly IReadOnlyBuffer<TableRow<T1, T2>> _buffer;

        protected TypedRowBasedDataTableAdapterBase(IDataTable dataTable, uint[] featureColumns) : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            _buffer = dataTable.GetRowsBuffer<T1, T2>(_featureColumnIndices.Single(), _targetColumnIndex);
        }

        /// <inheritdoc />
        protected override async IAsyncEnumerable<TableRow<T1, T2>> GetRows(uint[] rows)
        {
            var ret = await _buffer.GetItems(rows);
            foreach (var item in ret)
                yield return item;
        }
    }
}
