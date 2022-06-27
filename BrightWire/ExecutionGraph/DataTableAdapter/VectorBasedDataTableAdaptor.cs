﻿using System.Linq;
using BrightData;
using BrightData.DataTable2;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Segment table adapter for tables with vector data
    /// </summary>
    internal class VectorBasedDataTableAdapter : RowBasedDataTableAdapterBase
    {
        readonly uint[] _featureColumns;

        public VectorBasedDataTableAdapter(BrightDataTable dataTable, uint[] featureColumns) 
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            using var firstRow = dataTable.GetRow(0);
            var input = (IVector)firstRow[_featureColumnIndices.First()];
            var output = (IVector)firstRow[_targetColumnIndex];

            InputSize = input.Size;
            OutputSize = output.Size;
        }

        public override uint InputSize { get; }
	    public override uint? OutputSize { get; }

        public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows)
                .Select(r => (_featureColumnIndices.Select(i => ((IVector)r[i]).Segment.ToNewArray()).ToArray(), ((IVector)r[_targetColumnIndex]).Segment.ToNewArray()))
                .ToArray()
            ;
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(BrightDataTable dataTable)
        {
            return new VectorBasedDataTableAdapter(dataTable, _featureColumns);
        }
    }
}
