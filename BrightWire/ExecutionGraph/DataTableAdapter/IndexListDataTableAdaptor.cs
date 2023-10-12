using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.Analysis;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables with a index list based feature columns (corresponding to an unweighted sparse vector)
    /// </summary>
    internal class IndexListDataTableAdapter : DataTableAdapterBase<(IndexList, float[])>, IIndexListEncoder
    {
        readonly uint[] _featureColumns;

        public IndexListDataTableAdapter(IDataTable dataTable, VectorisationModel? outputVectoriser, uint[] featureColumns)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            OutputVectoriser = outputVectoriser ?? dataTable.GetVectoriser(true, dataTable.GetTargetColumnOrThrow()).Result;
            OutputSize = OutputVectoriser.OutputSize;

            var analysis = dataTable.GetColumnAnalysis(_featureColumnIndices).Result.Select(m => m.GetIndexAnalysis());
            InputSize = analysis.Max(a => a.MaxIndex ?? throw new ArgumentException("Could not find the max index")) + 1;
        }

        protected override IEnumerable<(IndexList, float[])> GetRows(uint[] rows)
        {
            return _dataTable
                .GetRows(rows)
                .Result
                .Select(tableRow => (Combine(_featureColumnIndices.Select(i => (IndexList)tableRow[i])), OutputVectoriser!.Vectorise(tableRow)));
        }

        public override uint InputSize { get; }
        public override uint? OutputSize { get; }

        public IndexList Combine(IEnumerable<IndexList> lists) => IndexList.Merge(lists);

        public float[] Encode(IndexList indexList)
        {
            var ret = new float[InputSize];
            foreach (var group in indexList.Indices.GroupBy(d => d))
                ret[group.Key] = group.Count();
            return ret;
        }

        public override IMiniBatch Get(uint[] rows)
        {
            var data = GetRows(rows).Select(r => (Encode(r.Item1), r.Item2)).ToArray();
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new IndexListDataTableAdapter(dataTable, OutputVectoriser, _featureColumns);
        }
    }
}
