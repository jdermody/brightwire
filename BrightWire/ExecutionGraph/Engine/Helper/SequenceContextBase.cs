using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightWire.ExecutionGraph.Helper;

namespace BrightWire.ExecutionGraph.Engine.Helper
{
    class SequenceContextBase
    {
        readonly Dictionary<string, List<(string Name, IGraphData Data)>> _namedData = new Dictionary<string, List<(string, IGraphData)>>();
        readonly Dictionary<int, IGraphData> _output = new Dictionary<int, IGraphData>();

        public SequenceContextBase(IMiniBatchSequence batchSequence)
        {
            Data = GraphData.Null;
            BatchSequence = batchSequence;
        }

        public IGraphData Data { get; set; }
        public IMiniBatchSequence BatchSequence { get; }

        public void SetOutput(IGraphData data, int channel = 0)
        {
            _output[channel] = data;
        }

        public IGraphData? GetOutput(int channel = 0)
        {
            if (_output.TryGetValue(channel, out var ret))
                return ret;
            return null;
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
            return Enumerable.Empty<(string, IGraphData)>();
        }
    }
}
