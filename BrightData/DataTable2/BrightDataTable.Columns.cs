using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightData.Helper;

namespace BrightData.DataTable2
{
    public partial class BrightDataTable
    {
        protected IEnumerable<uint> AllOrSpecifiedColumnIndices(uint[]? indices) => (indices is null || indices.Length == 0)
            ? _header.ColumnCount.AsRange()
            : indices;

        

        //public IEnumerable<ISingleTypeTableSegment> Columns(params uint[] columnIndices)
        //{
        //    // TODO: compress the columns based on frequency statistics
        //    var columns = AllOrSpecifiedColumnIndices(columnIndices).Select(i => (Index: i, Column: GetColumn(ColumnTypes[i], i, _columns[i].MetaData))).ToList();
        //    if (columns.Any()) {
        //        // set the column metadata
        //        columns.ForEach(item => {
        //            var metadata = item.Column.Segment.MetaData;
        //            var column = _columns[item.Index];
        //            column.MetaData.CopyTo(metadata);
        //        });
        //        var consumers = columns.Select(c => c.Column.Consumer).ToArray();
        //        ReadTyped(consumers);
        //    }

        //    return columns.Select(c => c.Column.Segment);
        //}
    }
}
