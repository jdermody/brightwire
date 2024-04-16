using System.Collections.Generic;
using BrightData;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Base class for data tables that work with generic data table rows
    /// </summary>
    /// <inheritdoc />
    public abstract class GenericRowBasedDataTableAdapterBase(IDataTable dataTable, uint[] featureColumns) : DataTableAdapterBase<ICanRandomlyAccessData>(dataTable, featureColumns)
    {
        /// <summary>
        /// Returns a tensor segment from a segment provider
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="segmentProvider"></param>
        /// <returns></returns>
        protected IReadOnlyNumericSegment<float> GetSegment(uint columnIndex, ICanRandomlyAccessData segmentProvider)
        {
            return ((IHaveReadOnlyTensorSegment<float>)segmentProvider[columnIndex]).ReadOnlySegment;
        }

        /// <inheritdoc />
        protected override async IAsyncEnumerable<ICanRandomlyAccessData> GetRows(uint[] rows)
        {
            var data = await _dataTable.GetRows(rows);
            foreach(var item in data)
                yield return item;
        }
    }
}
