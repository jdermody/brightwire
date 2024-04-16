using System;
using System.Threading.Tasks;
using BrightData;
using BrightData.DataTable.Helper;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single row instead of the normal batch mode
    /// </summary>
    internal class SingleRowDataSource(float[] data, LinearAlgebraProvider<float> lap, bool isSequential, MiniBatchSequenceType sequenceType, uint sequenceIndex)
        : IDataSource
    {
        public bool IsSequential { get; } = isSequential;
        public uint InputSize => (uint)data.Length;
        public uint? OutputSize => throw new NotImplementedException();
        public uint RowCount => 1;
        public VectorisationModel? InputVectoriser => null;
        public VectorisationModel? OutputVectoriser => null;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public Task<MiniBatch> Get(uint[] rows)
        {
            using var data1 = lap.CreateVector(data);
            var ret = new MiniBatch([0], this, data1.Reshape(1, null).AsGraphData(), null, IsSequential, sequenceType, sequenceIndex);
            return Task.FromResult(ret);
        }

        public uint[][] GetSequentialBatches()
        {
            return [
                [
                    1
                ]
            ];
        }
    }
}
