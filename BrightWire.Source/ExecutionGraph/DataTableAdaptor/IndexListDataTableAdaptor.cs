using System.Collections.Generic;
using System.Linq;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.DataTableAdaptor
{
    /// <summary>
    /// Adapts data tables with a index list based column (corresponding to an unweighted sparse vector)
    /// </summary>
    class IndexListDataTableAdaptor : DataTableAdaptorBase<(List<IndexList>, FloatVector)>, IIndexListEncoder
    {
        readonly int _inputSize, _outputSize;
        readonly IDataTableVectoriser _vectoriser;

        public IndexListDataTableAdaptor(ILinearAlgebraProvider lap, IDataTable dataTable, IDataTableVectoriser vectoriser)
            : base(lap, dataTable)
        {
            _vectoriser = vectoriser;
            _inputSize = vectoriser.InputSize;
            _outputSize = _vectoriser.OutputSize;

            // load the data
            dataTable.ForEach(row => _data.Add((_dataColumnIndex.Select(i => row.GetField<IndexList>(i)).ToList(), _vectoriser.GetOutput(row))));
        }

        public override bool IsSequential => false;
        public override int InputSize => _inputSize;
        public override int OutputSize => _outputSize;
        public override int RowCount => _data.Count;

        public float[] Encode(IndexList indexList)
        {
            var ret = new float[_inputSize];
            foreach (var group in indexList.Index.GroupBy(d => d))
                ret[group.Key] = group.Count();
            return ret;
        }

        public override IMiniBatch Get(IExecutionContext executionContext, IReadOnlyList<int> rows)
        {
            var data = _GetRows(rows)
                .Select(r => (r.Item1.Select(d => Encode(d)).ToArray(), r.Item2.Data))
                .ToList()
            ;
            return _GetMiniBatch(rows, data);
        }

        public override IDataSource CloneWith(IDataTable dataTable)
        {
            return new IndexListDataTableAdaptor(_lap, dataTable, _vectoriser);
        }
    }
}
