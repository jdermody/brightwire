using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using BrightWire.Helper;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Node
{
    /// <summary>
    /// Base class for graph nodes
    /// </summary>
    public abstract class NodeBase : INode
    {
        string _id;
        string? _name;
        List<IWire> _output = new List<IWire>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the node (optional)</param>
        /// <param name="id">The node's unique id (optional)</param>
        protected NodeBase(string? name, string? id = null)
        {
            _id = id ?? Guid.NewGuid().ToString("n");
            _name = name;
        }

        /// <summary>
        /// Called when deserialising the node
        /// </summary>
        /// <param name="graph">Graph factory</param>
        /// <param name="description">Node description</param>
        /// <param name="data">Node serialisation data</param>
        protected virtual void Initalise(GraphFactory graph, string? description, byte[]? data)
        {
            ReadFrom(data, reader => ReadFrom(graph, reader));
        }

        #region Disposal
        /// <summary>
        /// Destructor
        /// </summary>
        ~NodeBase()
        {
            DisposeInternal(false);
        }
        /// <summary>
        /// Disposal
        /// </summary>
        public void Dispose()
        {
            DisposeInternal(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposal
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void DisposeInternal(bool isDisposing) { } 
        #endregion

        /// <summary>
        /// The node's unique id
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// The node's name in the graph
        /// </summary>
        public string? Name => _name;

        /// <summary>
        /// The list of outgoing wires along which the output signal will be sent
        /// </summary>
        public virtual List<IWire> Output => _output;

        /// <summary>
        /// Called when executing forward on the primary channel
        /// </summary>
        /// <param name="context"></param>
        public abstract void ExecuteForward(IGraphSequenceContext context);

        /// <summary>
        /// Called when executing forward on a non-primary channel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        protected virtual void ExecuteForwardInternal(IGraphSequenceContext context, uint channel)
        {
            ExecuteForward(context);
        }

        /// <summary>
        /// Called when executing forward
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        public void ExecuteForward(IGraphSequenceContext context, uint channel)
        {
            if (channel == 0)
                ExecuteForward(context);
            else
                ExecuteForwardInternal(context, channel);
        }

        public IGraphData Forward(IGraphData signal, IGraphSequenceContext context)
        {
            return Forward(this, signal, 0, context, null);
        }

        static IGraphData Forward(INode node, IGraphData signal, uint channel, IGraphSequenceContext context, INode? prev)
        {
            var (ret, backProp) = node.Forward(signal, channel, context, prev);
            if (ret.HasValue) {
                if (prev != null)
                    context.AddForward(node, ret, backProp, prev);
                else
                    context.AddForward(node, ret, backProp);

                IGraphData next = NullGraphData.Instance;
                foreach (var wire in node.Output) {
                    var result = Forward(wire.SendTo, ret, wire.Channel, context, node);
                    if (result.HasValue) {
                        Debug.Assert(!next.HasValue || ReferenceEquals(result, next));
                        next = result;
                    }
                }

                return next.HasValue ? next : ret;
            }

            return ret;
        }

        public abstract (IGraphData Next, Func<IBackpropagate>? BackProp) Forward(IGraphData signal, uint channel, IGraphSequenceContext context, INode? source);

        /// <summary>
        /// Records the node execution and queues the output nodes for execution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="data"></param>
        /// <param name="backProp"></param>
        protected void AddNextGraphAction(IGraphSequenceContext context, IGraphData data, Func<IBackpropagate>? backProp)
        {
            context.AddForward(this, data, backProp, context.Source!);
        }

        /// <summary>
        /// Serialise this node and any connected nodes
        /// </summary>
        /// <param name="existing">Set of nodes that have already been serialised in the current context</param>
        /// <param name="connectedTo">List of nodes this node is connected to</param>
        /// <param name="wireList">List of wires between all connected nodes</param>
        /// <returns></returns>
        public virtual ExecutionGraphModel.Node SerialiseTo(HashSet<INode>? existing, List<ExecutionGraphModel.Node>? connectedTo, HashSet<ExecutionGraphModel.Wire>? wireList)
        {
            var (description, data) = GetInfo();
            var ret = new ExecutionGraphModel.Node {
                Id = _id,
                Name = _name,
                Data = data,
                Description = description,
                TypeName = TypeLoader.GetTypeName(this)
            };

            // get the connected nodes
            if (existing != null && connectedTo != null && wireList != null) {
                foreach (var wire in Output) {
                    var sendTo = wire.SendTo;
                    wireList.Add(new ExecutionGraphModel.Wire {
                        FromId = _id,
                        InputChannel = wire.Channel,
                        ToId = sendTo.Id
                    });
                    if (existing.Add(sendTo))
                        connectedTo.Add(sendTo.SerialiseTo(existing, connectedTo, wireList));
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns serialisation information
        /// </summary>
        protected virtual (string Description, byte[]? Data) GetInfo()
        {
            return (GetType().Name, null);
        }

        /// <summary>
        /// Called after the graph has been completely deserialised
        /// </summary>
        /// <param name="graph">The complete graph</param>
        public virtual void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            // nop
        }

        /// <summary>
        /// Serialise the node
        /// </summary>
        /// <param name="writer">Binary writer</param>
        public virtual void WriteTo(BinaryWriter writer)
        {
            // nop
        }

        /// <summary>
        /// Deserialise the node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="reader">Binary reader</param>
        public virtual void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            // nop
        }

        /// <summary>
        /// Initialise the node from serialised data
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="id">Unique id</param>
        /// <param name="name">Node name</param>
        /// <param name="description">Node description</param>
        /// <param name="data">Serialisation data</param>
        void ICanInitialiseNode.Initialise(GraphFactory factory, string id, string? name, string? description, byte[]? data)
        {
            _id = id;
            _name = name;
            _output = new List<IWire>();
            Initalise(factory, description, data);
        }

        /// <summary>
        /// Finds a connected node by friendly name
        /// </summary>
        /// <param name="name">The node's name to search for</param>
        /// <returns></returns>
        public INode? FindByName(string name)
        {
            var context = new Stack<INode>();
            context.Push(this);

            while(context.Any()) {
                var curr = context.Pop();
                if (curr.Name == name)
                    return curr;

                foreach(var next in curr.Output)
                    context.Push(next.SendTo);
            }
            return null;
        }

        /// <summary>
        /// Finds a connected node by id
        /// </summary>
        /// <param name="id">Unique id to find</param>
        /// <returns></returns>
        public INode? FindById(string id)
        {
            var context = new Stack<INode>();
            context.Push(this);

            while (context.Any()) {
                var curr = context.Pop();
                if (curr.Id == id)
                    return curr;

                foreach (var next in curr.Output)
                    context.Push(next.SendTo);
            }
            return null;
        }

        /// <summary>
        /// The list of sub-nodes
        /// </summary>
        public virtual IEnumerable<INode> SubNodes => Enumerable.Empty<INode>();

        /// <summary>
        /// Serialise the node to the writer
        /// </summary>
        /// <param name="node">The node to serialise</param>
        /// <param name="writer">The binary writer</param>
        protected static void Serialise(INode node, BinaryWriter writer)
        {
            node.SerialiseTo(null, null, null).WriteTo(writer);
        }

        /// <summary>
        /// Helper function to write data to a binary writer
        /// </summary>
        /// <param name="callback">Callback to receive the writer</param>
        /// <returns></returns>
        protected static byte[] WriteData(Action<BinaryWriter> callback)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                callback(writer);
            return stream.ToArray();
        }

        /// <summary>
        /// Helper function to read from a binary reader
        /// </summary>
        /// <param name="data">The data to read</param>
        /// <param name="callback">Callback to receive the writer</param>
        protected static void ReadFrom(byte[]? data, Action<BinaryReader> callback)
        {
            if (data != null) {
                using var reader = new BinaryReader(new MemoryStream(data), Encoding.UTF8);
                callback(reader);
            }
        }

        /// <summary>
        /// Reads serialisation information and creates a node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected static INode Hydrate(GraphFactory factory, BinaryReader reader)
        {
            var model = new ExecutionGraphModel.Node(reader);
            return factory.Create(model);
        }

        /// <summary>
        /// Loads parameters into an existing node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="nodeData">Serialised node parameters</param>
        public virtual void LoadParameters(GraphFactory factory, ExecutionGraphModel.Node nodeData)
        {
            if(nodeData.Data != null)
                ReadFrom(nodeData.Data, reader => ReadFrom(factory, reader));
        }

        /// <summary>
        /// Finds a sub node by name, or throws an exception if not found
        /// </summary>
        /// <param name="name">Sub node name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public INode FindSubNodeByNameOrThrow(string name)
        {
            foreach (var subNode in SubNodes) {
                var ret = subNode.FindByName(name);
                if (ret != null)
                    return ret;
            }
            throw new ArgumentException($"Node not found: {name}");
        }

        /// <summary>
        /// Writes a sub node to the binary writer
        /// </summary>
        /// <param name="name">Sub node name</param>
        /// <param name="writer"></param>
        protected void WriteSubNode(string name, BinaryWriter writer) => FindSubNodeByNameOrThrow(name).WriteTo(writer);

        /// <summary>
        /// Initializes a sub node from a binary reader
        /// </summary>
        /// <param name="name">Sub node name</param>
        /// <param name="factory"></param>
        /// <param name="reader"></param>
        protected void ReadSubNode(string name, GraphFactory factory, BinaryReader reader) => FindSubNodeByNameOrThrow(name).ReadFrom(factory, reader);
    }
}
