using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;

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
            public uint Channel { get; }

            /// <summary>
            /// The signal
            /// </summary>
            public IFloatMatrix? Data { get; private set; }

            /// <summary>
            /// The node the signal came from
            /// </summary>
            public INode? Source { get; private set; }

            /// <summary>
            /// The size of the input signal
            /// </summary>
            public uint Size { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="size">The size of the input signal</param>
            /// <param name="channel">The channel</param>
            public IncomingChannel(uint size, uint channel)
            {
                Channel = channel;
                Size = size;
            }

            /// <summary>
            /// Sets data
            /// </summary>
            /// <param name="data">The node signal</param>
            /// <param name="source">The source node</param>
            public void SetData(IFloatMatrix? data, INode? source)
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

        readonly Dictionary<uint, IncomingChannel> _data;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the node (optional)</param>
        /// <param name="incoming">The list of incoming wires</param>
        protected MultiGateBase(string? name, params WireBuilder[] incoming) : base(name)
        {
            _data = new Dictionary<uint, IncomingChannel>();
            for (uint i = 0, len = (uint)incoming.Length; i < len; i++)
                _data[i] = new IncomingChannel(incoming[i].CurrentSize, i);
        }

        /// <summary>
        /// Executes on the primary channel
        /// </summary>
        /// <param name="context">The graph context</param>
        public override void ExecuteForward(IGraphSequenceContext context)
        {
            ExecuteForwardInternal(context, 0);
        }

        /// <summary>
        /// Executes on a secondary channel
        /// </summary>
        /// <param name="context">The graph context</param>
        /// <param name="channel">The channel</param>
        protected override void ExecuteForwardInternal(IGraphSequenceContext context, uint channel)
        {
            if (_data.TryGetValue(channel, out var data)) {
                data.SetData(context.Data.GetMatrix(), context.Source);
                if(_data.All(kv => kv.Value.IsValid)) {
                    Activate(context, _data.Select(kv => kv.Value).ToList());

                    // reset the inputs
                    foreach (var item in _data)
                        item.Value.Clear();
                }
            }
        }

        public override (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source)
        {
            IGraphData next = NullGraphData.Instance;
            Func<IBackpropagate>? backProp = null;

            if (_data.TryGetValue(channel, out var data)) {
                data.SetData(signal.GetMatrix(), source);
                if(_data.All(kv => kv.Value.IsValid)) {
                    IFloatMatrix matrix;
                    (matrix, backProp) = Activate2(context, _data.Select(kv => kv.Value).ToList());
                    next = signal.ReplaceWith(matrix);

                    // reset the inputs
                    foreach (var item in _data)
                        item.Value.Clear();
                }
            }

            return (next, backProp);
        }

        /// <summary>
        /// Called when all registered inputs have arrived
        /// </summary>
        /// <param name="context">The graph context</param>
        /// <param name="data">The list of incoming signals</param>
        protected abstract void Activate(IGraphSequenceContext context, List<IncomingChannel> data);

        protected abstract (IFloatMatrix Next, Func<IBackpropagate>? BackProp) Activate2(IGraphSequenceContext context, List<IncomingChannel> data);

        /// <summary>
        /// Records the network activity
        /// </summary>
        /// <param name="context">The graph context</param>
        /// <param name="data">The list of incoming signals</param>
        /// <param name="output">Output signal</param>
        /// <param name="backpropagation">Backpropagation creator (optional)</param>
        protected void AddHistory(IGraphSequenceContext context, List<IncomingChannel> data, IFloatMatrix output, Func<IBackpropagate> backpropagation)
        {
            var sources = data.Where(d => d.Source != null).Select(d => d.Source!).ToArray();
            context.AddForward(this, new MatrixGraphData(output), backpropagation, sources);
        }

	    /// <inheritdoc />
	    protected override (string Description, byte[] Data) GetInfo()
        {
            return ("MG", WriteData(WriteTo));
        }

	    /// <inheritdoc />
        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _data.Clear();

            var len = reader.ReadInt32();
            for(uint i = 0; i < len; i++) {
                var size = (uint)reader.ReadInt32();
                var channel = (uint)reader.ReadInt32();
                _data[i] = new IncomingChannel(size, channel);
            }
        }

	    /// <inheritdoc />
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_data.Count);
            foreach (var item in _data) {
                writer.Write((int)item.Value.Size);
                writer.Write((int)item.Value.Channel);
            }
        }
    }
}
