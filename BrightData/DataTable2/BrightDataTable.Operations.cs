using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.DataTable2.Operations;
using BrightData.Helper;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable
    {
        public IOperation<MetaData> CreateColumnAnalyser(uint columnIndex, uint writeCount, uint maxDistinctCount)
        {
            var metaData = GetColumnMetaData(columnIndex);
            var type = metaData.GetColumnType();
            var analyser = type.GetColumnAnalyser(metaData, writeCount, maxDistinctCount);

            return GenericActivator.Create<IOperation<MetaData>>(typeof(AnalyseColumnOperation<>).MakeGenericType(type.GetDataType()),
                metaData,
                GetColumnReader(columnIndex, _header.ColumnCount),
                analyser
            );
        }

        public IOperation<(string Label, IHybridBuffer[] ColumnData)[]> GroupBy(params uint[] groupByColumnIndices)
        {
            return new GroupByOperation(
                _context,
                _header.RowCount,
                groupByColumnIndices,
                ColumnMetaData.ToArray(),
                GetColumnReaders(ColumnIndices)
            );
        }
    }
}
