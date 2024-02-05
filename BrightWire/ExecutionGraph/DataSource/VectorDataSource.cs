using BrightData;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using System.Threading.Tasks;
using BrightData.LinearAlgebra;
using BrightData.DataTable.Helper;

namespace BrightWire.ExecutionGraph.DataSource
{
    /// <summary>
    /// Feeds data to the execution graph
    /// </summary>
    internal class VectorDataSource : IDataSource
    {
	    readonly IVector[] _data;
        readonly LinearAlgebraProvider _lap;

        public VectorDataSource(LinearAlgebraProvider lap, IVector[] data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            InputSize = first.Size;
            OutputSize = null;
        }

        public uint InputSize { get; }
	    public uint? OutputSize { get; }
	    public uint RowCount => (uint)_data.Length;
        public VectorisationModel? InputVectoriser => null;
        public VectorisationModel? OutputVectoriser => null;

        public Task<MiniBatch> Get(uint[] rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();
            var input = _lap.CreateMatrix((uint)data.Count, InputSize, (x, y) => data[(int)x].Segment[y]);
            return Task.FromResult(new MiniBatch(rows, this, input.AsGraphData(), null));
        }

        public uint[][] GetSequentialBatches()
        {
            return [
                _data.Length.AsRange().ToArray()
            ];
        }

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }
    }
}
