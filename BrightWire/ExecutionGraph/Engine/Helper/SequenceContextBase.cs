using System.Collections.Generic;
using System.Linq;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class SequenceContextBase(MiniBatch.Sequence batchSequence)
    {
        readonly Dictionary<string, List<(string Name, IGraphData Data)>> _namedData = [];
        readonly Dictionary<int, IGraphData> _output = [];

        public IGraphData Data { get; set; } = GraphData.Null;
        public MiniBatch.Sequence BatchSequence { get; } = batchSequence;

        public void SetOutput(IGraphData data, int channel = 0)
        {
            _output[channel] = data;
        }

        public IGraphData? GetOutput(int channel = 0)
        {
            return _output.GetValueOrDefault(channel);
        }

        public IGraphData[] Output => _output
            .OrderBy(kv => kv.Key)
            .Select(kv => kv.Value)
            .ToArray()
        ;

        public void SetData(string name, string type, IGraphData data)
        {
            if (!_namedData.TryGetValue(type, out var list))
                _namedData.Add(type, list = new List<(string, IGraphData)>());
            list.Add((name, data));
        }

        public IEnumerable<(string Name, IGraphData Data)> GetData(string type)
        {
            if (_namedData.TryGetValue(type, out var ret))
                return ret;
            return [];
        }
    }
}
