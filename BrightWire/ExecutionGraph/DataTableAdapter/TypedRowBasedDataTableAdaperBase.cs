using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData;
using BrightData.DataTable;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    internal abstract class TypedRowBasedDataTableAdapterBase<T1, T2> : DataTableAdapterBase<TableRow<T1, T2>>
        where T1 : IHaveSize
        where T2 : IHaveSize
    {
        protected readonly uint[] _featureColumns;
        readonly IReadOnlyBuffer<TableRow<T1, T2>> _buffer;

        protected TypedRowBasedDataTableAdapterBase(IDataTable dataTable, uint[] featureColumns) : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            _buffer = dataTable.GetRowsBuffer<T1, T2>(_featureColumnIndices.Single(), _targetColumnIndex);
            var firstRow = _buffer.GetItem(0).Result;
            InputSize = firstRow.C1.Size;
            OutputSize = firstRow.C2.Size;
        }

        public override uint InputSize { get; }
        public override uint? OutputSize { get; }

        /// <inheritdoc />
        protected override async IAsyncEnumerable<TableRow<T1, T2>> GetRows(uint[] rows)
        {
            var ret = await _buffer.GetItems(rows);
            foreach (var item in ret)
                yield return item;
        }
    }
}
