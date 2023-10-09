using BrightWire.ExecutionGraph.Helper;
using System;
using System.Linq;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    /// <summary>
    /// Helper class when executing a single sequence
    /// </summary>
    internal class SequentialRowDataSource : IDataSource
    {
        readonly float[][] _data;
        readonly LinearAlgebraProvider _lap;

        public SequentialRowDataSource(float[][] data, LinearAlgebraProvider lap)
        {
            _data = data;
            _lap = lap;
            InputSize = (uint)data.First().Length;
            InputCount = (uint)data.Length;
        }

        public uint InputSize { get; }
        public uint? OutputSize { get; } = null;
        public uint RowCount => 1;
        public uint InputCount { get; }
        public IDataTableVectoriser? InputVectoriser { get; } = null;
        public IDataTableVectoriser? OutputVectoriser { get; } = null;

        public IDataSource CloneWith(IDataTable dataTable)
        {
            throw new NotImplementedException();
        }

        public IMiniBatch Get(uint[] rows)
        {
            var ret = new MiniBatch(rows, this);
            int index = 0;
            foreach (var row in _data) {
                var type = MiniBatchSequenceType.Standard;
                if (index == 0)
                    type = MiniBatchSequenceType.SequenceStart;
                else if (index == _data.Length - 1)
                    type = MiniBatchSequenceType.SequenceEnd;
                using var temp = _lap.CreateVector(row);
                ret.Add(type, temp.Reshape(1, null).AsGraphData(), null);
                ++index;
            }
            return ret;
        }

        public uint[][] GetSequentialBatches()
        {
            return new[] {
                new uint [] {
                    0
                }
            };
        }
    }
}
