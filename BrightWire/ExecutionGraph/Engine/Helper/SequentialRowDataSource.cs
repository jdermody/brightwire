using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.LinearAlgebra;
using BrightData.Buffer.Vectorisation;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single sequence
    /// </summary>
    internal class SequentialRowDataSource(float[][] data, LinearAlgebraProvider<float> lap) : IDataSource
    {
        public uint InputSize { get; } = (uint)data.First().Length;
        public uint? OutputSize { get; } = null;
        public uint RowCount => 1;
        public uint InputCount { get; } = (uint)data.Length;
        public VectorisationModel? InputVectoriser { get; } = null;
        public VectorisationModel? OutputVectoriser { get; } = null;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public Task<MiniBatch> Get(uint[] rows)
        {
            var ret = new MiniBatch(rows, this);
            int index = 0;
            foreach (var row in data) {
                var type = MiniBatchSequenceType.Standard;
                if (index == 0)
                    type = MiniBatchSequenceType.SequenceStart;
                else if (index == data.Length - 1)
                    type = MiniBatchSequenceType.SequenceEnd;
                using var temp = lap.CreateVector(row);
                ret.Add(type, temp.Reshape(1, null).AsGraphData(), null);
                ++index;
            }
            return Task.FromResult(ret);
        }

        public uint[][] GetSequentialBatches()
        {
            return [
                [
                    0
                ]
            ];
        }
    }
}
