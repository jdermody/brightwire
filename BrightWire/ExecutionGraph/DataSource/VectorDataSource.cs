using BrightData;
using BrightTable;
using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;

namespace BrightWire.ExecutionGraph.DataSource
{
    /// <summary>
    /// Feeds data to the execution graph
    /// </summary>
    class VectorDataSource : IDataSource
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
        public IVectorise InputVectoriser { get; } = null;
        public IVectorise OutputVectoriser { get; } = null;

        public IMiniBatch Get(IExecutionContext executionContext, uint[] rows)
        {
            var data = rows.Select(i => _data[(int)i]).ToList();
            var input = _lap.CreateMatrix((uint)data.Count, InputSize, (x, y) => data[(int)x].Segment[y]);
            var inputList = new IGraphData[] {
                new MatrixGraphData(input)
            };
            return new MiniBatch(rows, this, inputList, null);
        }

        public uint[][] GetBuckets()
        {
            return new[] {
                _data.Length.AsRange().ToArray()
            };
        }

        public IDataSource CloneWith(IRowOrientedDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public void OnBatchProcessed(IGraphContext context)
        {
            // nop
        }
    }
}
