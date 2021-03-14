using BrightData;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.DataSource
{
    /// <summary>
    /// Feeds data to the execution graph
    /// </summary>
    internal class VectorDataSource : IDataSource
    {
	    readonly Vector<float>[] _data;
        readonly ILinearAlgebraProvider _lap;

        public VectorDataSource(ILinearAlgebraProvider lap, Vector<float>[] data)
        {
            _lap = lap;
            _data = data;

            var first = data.First();
            InputSize = first.Size;
            OutputSize = null;
        }

        public uint InputCount => 1;
        public bool IsSequential => false;
        public uint InputSize { get; }
	    public uint? OutputSize { get; }
	    public uint RowCount => (uint)_data.Length;
        public IDataTableVectoriser? InputVectoriser { get; } = null;
        public IDataTableVectoriser? OutputVectoriser { get; } = null;

        public IMiniBatch Get(uint[] rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();
            var input = _lap.CreateMatrix((uint)data.Count, InputSize, (x, y) => data[(int)x].Segment[y]);
            return new MiniBatch(rows, this, new MatrixGraphData(input), null);
        }

        public uint[][] GetSequentialBatches()
        {
            return new[] {
                _data.Length.AsRange().ToArray()
            };
        }

        public IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            throw new NotImplementedException();
        }
    }
}
