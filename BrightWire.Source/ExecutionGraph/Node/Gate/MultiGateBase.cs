using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrightWire.ExecutionGraph.Node.Gate
{
    /// <summary>
    /// Base class for nodes that accept input on an arbitary number of channels and output a single signal
    /// </summary>
    public abstract class MultiGateBase : NodeBase
    {
        /// <summary>
        /// Information about an incoming signal
        /// </summary>
        protected class IncomingChannel
        {
            /// <summary>
            /// The channel on which the signal was received
            /// </summary>
            public int Channel { get; private set; }

            /// <summary>
            /// The signal
            /// </summary>
            public IMatrix Data { get; private set; }

            /// <summary>
            /// The node the signal came from
            /// </summary>
            public INode Source { get; private set; }

            /// <summary>
            /// The size of the input signal
            /// </summary>
            public int Size { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="size">The size of the input signal</param>
            /// <param name="channel">The channel</param>
            public IncomingChannel(int size, int channel)
            {
                Channel = channel;
                Size = size;
            }

            /// <summary>
            /// Sets data
            /// </summary>
            /// <param name="data">The node signal</param>
            /// <param name="source">The source node</param>
            public void SetData(IMatrix data, INode source)
            {
                Data = data;
                Source = source;
            }

            /// <summary>
            /// Clears the data
            /// </summary>
            public void Clear()
            {
                Data = null;
                Source = null;
            }

            /// <summary>
            /// True if a signal has been received
            /// </summary>
            public bool IsValid => Data != null;
        }
        Dictionary<int, IncomingChannel> _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the node (optional)</param>
        /// <param name="incoming">The list of incoming wires</param>
        public MultiGateBase(string name, params WireBuilder[] incoming) : base(name)
        {
            _data = new Dictionary<int, IncomingChannel>();
            for (int i = 0, len = incoming.Length; i < len; i++)
                _data[i] = new IncomingChannel(incoming[i].CurrentSize, i);
        }

        /// <summary>
        /// Executes on the primary channel
        /// </summary>
        /// <param name="context">The graph context</param>
        public override void ExecuteForward(IContext context)
        {
            _ExecuteForward(context, 0);
        }

        /// <summary>
        /// Executes on a secondary channel
        /// </summary>
        /// <param name="context">The graph context</param>
        /// <param name="channel">The channel</param>
        protected override void _ExecuteForward(IContext context, int channel)
        {
            if (_data.TryGetValue(channel, out IncomingChannel data)) {
                data.SetData(context.Data.GetMatrix(), context.Source);
                if(_data.All(kv => kv.Value.IsValid)) {
                    _Activate(context, _data.Select(kv => kv.Value).ToList());

                    // reset the inputs
                    foreach (var item in _data)
                        item.Value.Clear();
                }
            }
        }

        /// <summary>
        /// Called when all registered inputs have arrived
        /// </summary>
        /// <param name="context">The graph context</param>
        /// <param name="data">The list of incoming signals</param>
        protected abstract void _Activate(IContext context, IReadOnlyList<IncomingChannel> data);

        /// <summary>
        /// Records the network activity
        /// </summary>
        /// <param name="context">The graph context</param>
        /// <param name="data">The list of incoming signals</param>
        /// <param name="output">Output signal</param>
        /// <param name="backpropagation">Backpropagation creator (optional)</param>
        protected void _AddHistory(IContext context, IReadOnlyList<IncomingChannel> data, IMatrix output, Func<IBackpropagation> backpropagation)
        {
            var sources = data.Select(d => d.Source).ToList();
            context.AddForward(new TrainingAction(this, new MatrixGraphData(output), sources), backpropagation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("MG", _WriteData(WriteTo));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="reader"></param>
        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var lap = factory?.LinearAlgebraProvider;

            if (_data == null)
                _data = new Dictionary<int, IncomingChannel>();
            else
                _data.Clear();

            var len = reader.ReadInt32();
            for(var i = 0; i < len; i++) {
                var size = reader.ReadInt32();
                var channel = reader.ReadInt32();
                _data[i] = new IncomingChannel(size, channel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_data.Count);
            foreach (var item in _data) {
                writer.Write(item.Value.Size);
                writer.Write(item.Value.Channel);
            }
        }
    }
}
