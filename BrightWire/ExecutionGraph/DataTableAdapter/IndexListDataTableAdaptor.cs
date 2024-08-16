using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.Buffer.Vectorisation;
using BrightData.Types;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.DataTableAdapter
{
    /// <summary>
    /// Adapts data tables with an index list based feature columns (corresponding to an unweighted sparse vector)
    /// </summary>
    internal class IndexListDataTableAdapter : DataTableAdapterBase<(IndexList, float[]?)>, IIndexListEncoder
    {
        readonly uint[] _featureColumns;

        IndexListDataTableAdapter(IDataTable dataTable, VectorisationModel? outputVectoriser, uint[] featureColumns, uint inputSize)
            : base(dataTable, featureColumns)
        {
            _featureColumns = featureColumns;
            OutputVectoriser = outputVectoriser;
            OutputSize = OutputVectoriser?.OutputSize;
            InputSize = inputSize;
        }

        public static async Task<IndexListDataTableAdapter> Create(IDataTable dataTable, VectorisationModel? outputVectoriser, uint[] featureColumns)
        {
            var analysis = (await dataTable.GetColumnAnalysis(featureColumns)).Select(m => m.GetIndexAnalysis());
            var inputSize = analysis.Max(a => a.MaxIndex ?? throw new ArgumentException("Could not find the max index")) + 1;
            return new(dataTable, outputVectoriser ?? await dataTable.GetVectoriser(true, dataTable.GetTargetColumnOrThrow()), featureColumns, inputSize);
        }

        protected override async IAsyncEnumerable<(IndexList, float[]?)> GetRows(uint[] rows)
        {
            foreach (var tableRow in await _dataTable.GetRows(rows))
                yield return (Combine(_featureColumnIndices.Select(i => (IndexList)tableRow[i])), OutputVectoriser?.Vectorise(tableRow));
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

        public override async Task<MiniBatch> Get(uint[] rows)
        {
            var index = 0;
            var data = new (float[], float[]?)[rows.Length];
            await foreach (var row in GetRows(rows))
                data[index++] = (Encode(row.Item1), row.Item2);
            return GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new IndexListDataTableAdapter(dataTable, OutputVectoriser, _featureColumns, InputSize);
        }
    }
}
