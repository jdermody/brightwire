using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables with weighted index list based columns (corresponding to a sparse vector)
    /// </summary>
    internal class WeightedIndexListDataTableAdapter : DataTableAdapterBase<(WeightedIndexList IndexList, float[] Output)>, IWeightedIndexListEncoder
    {
        readonly uint[] _featureColumns;

        public WeightedIndexListDataTableAdapter(BrightDataTable dataTable, IDataTableVectoriser? outputVectoriser, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            OutputVectoriser = outputVectoriser ?? dataTable.GetVectoriser(true, dataTable.GetTargetColumnOrThrow());
            OutputSize = OutputVectoriser.OutputSize;

            var analysis = dataTable.GetColumnAnalysis(_featureColumnIndices).Select(m => m.MetaData.GetIndexAnalysis());
            InputSize = analysis.Max(a => a.MaxIndex ?? throw new ArgumentException("Could not find the max index")) + 1;
        }

        public override uint InputSize { get; }
        public override uint? OutputSize { get; }

        protected override IEnumerable<(WeightedIndexList IndexList, float[] Output)> GetRows(uint[] rows)
        {
            return _dataTable.GetRows(rows).Select(tableRow => (Combine(_featureColumnIndices.Select(i => (WeightedIndexList)tableRow[i])), OutputVectoriser!.Vectorise(tableRow)));
        }

        public WeightedIndexList Combine(IEnumerable<WeightedIndexList> lists) => WeightedIndexList.Merge(lists);

        public float[] Encode(WeightedIndexList indexList)
        {
            var ret = new float[InputSize];
            foreach (ref readonly var item in indexList.ReadOnlySpan)
                ret[item.Index] = item.Weight;
            return ret;
        }

        public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows)
                .Select(r => (Encode(r.IndexList), r.Output))
                .ToArray()
            ;
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(BrightDataTable dataTable)
        {
            return new WeightedIndexListDataTableAdapter(dataTable, OutputVectoriser, _featureColumns);
        }
    }
}
