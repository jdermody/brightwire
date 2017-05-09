using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    abstract class MultiGateBase : NodeBase
    {
        protected class IncomingChannel
        {
            public int Channel { get; private set; }
            public IMatrix Data { get; private set; }
            public INode Source { get; private set; }
            public int Size { get; private set; }

            public IncomingChannel(int size, int channel)
            {
                Channel = channel;
                Size = size;
            }
            public void SetData(IMatrix data, INode source)
            {
                Data = data;
                Source = source;
            }
            public void Clear()
            {
                Data?.Dispose();
                Data = null;
                Source = null;
            }
            public bool IsValid => Data != null;
        }
        readonly Dictionary<int, IncomingChannel> _data = new Dictionary<int, IncomingChannel>();

        public MultiGateBase(string name, params WireBuilder[] incoming) : base(name)
        {
            for(int i = 0, len = incoming.Length; i < len; i++)
                _data[i] = new IncomingChannel(incoming[i].CurrentSize, i);
        }

        public override void ExecuteForward(IContext context)
        {
            _ExecuteForward(context, 0);
        }

        public override void _ExecuteForward(IContext context, int channel)
        {
            IncomingChannel data;
            if (_data.TryGetValue(channel, out data)) {
                data.SetData(context.Data.GetAsMatrix(), context.Source);
                if(_data.All(kv => kv.Value.IsValid)) {
                    _Activate(context, _data.Select(kv => kv.Value));

                    // reset the inputs
                    foreach (var item in _data)
                        item.Value.Clear();
                }
            }
        }

        protected abstract void _Activate(IContext context, IEnumerable<IncomingChannel> data);

        protected void _AddHistory(IContext context, IMatrix output, Func<IBackpropagation> backpropagation)
        {
            var sources = _data.Select(kv => kv.Value.Source).ToList();
            context.Forward(new GraphAction(this, new MatrixGraphData(output), sources), backpropagation);
        }
    }
}
