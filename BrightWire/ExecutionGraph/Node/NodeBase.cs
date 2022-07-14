using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using BrightData;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Helper;
using BrightWire.Models;

namespace BrightWire.ExecutionGraph.Node
{
    /// <summary>
    /// Base class for graph nodes
    /// </summary>
    public abstract class NodeBase : ICanInitialiseNode, IDisposable, ICanSerialise
    {
        string _id;
        string? _name;
        List<WireToNode> _output = new();

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
        public virtual List<WireToNode> Output => _output;

        /// <summary>
        /// Executes the graph
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <param name="signal">Initial data</param>
        /// <param name="context">Context</param>
        /// <param name="channel"></param>
        /// <param name="prev"></param>
        public void Forward(IGraphData signal, IGraphContext context, uint channel = 0, NodeBase? prev = null)
        {
            // execute the node
            var (from, output, backProp) = ForwardSingleStep(signal, channel, context, prev);

            // add to the context history
            if (prev != null)
                context.AddForwardHistory(from, output, backProp, prev);
            else
                context.AddForwardHistory(from, output, backProp);

            // send output to connected nodes
            if (output.HasValue || this is FlowThrough) {
                foreach (var wire in from.Output) {
                    wire.SendTo.Forward(output, context, wire.Channel, from);
                }
            }
        }

        /// <summary>
        /// Executes a single forward step
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="channel"></param>
        /// <param name="context"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public abstract (NodeBase FromNode, IGraphData Output, Func<IBackpropagate>? BackProp) ForwardSingleStep(IGraphData signal, uint channel, IGraphContext context, NodeBase? source);

        /// <summary>
        /// Serialise this node and any connected nodes
        /// </summary>
        /// <param name="existing">Set of nodes that have already been serialised in the current context</param>
        /// <param name="connectedTo">List of nodes this node is connected to</param>
        /// <param name="wireList">List of wires between all connected nodes</param>
        /// <returns></returns>
        public virtual ExecutionGraphModel.Node SerialiseTo(HashSet<NodeBase>? existing, List<ExecutionGraphModel.Node>? connectedTo, HashSet<ExecutionGraphModel.Wire>? wireList)
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
        public virtual void OnDeserialise(IReadOnlyDictionary<string, NodeBase> graph)
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
        public void Initialise(GraphFactory factory, string id, string? name, string? description, byte[]? data)
        {
            _id = id;
            _name = name;
            _output = new List<WireToNode>();
            Initalise(factory, description, data);
        }

        /// <summary>
        /// Finds a connected node by friendly name
        /// </summary>
        /// <param name="name">The node's name to search for</param>
        /// <returns></returns>
        public NodeBase? FindByName(string name)
        {
            var context = new Stack<NodeBase>();
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
        public NodeBase? FindById(string id)
        {
            var context = new Stack<NodeBase>();
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
        public virtual IEnumerable<NodeBase> SubNodes => Enumerable.Empty<NodeBase>();

        /// <summary>
        /// Serialise the node to the writer
        /// </summary>
        /// <param name="node">The node to serialise</param>
        /// <param name="writer">The binary writer</param>
        protected static void Serialise(NodeBase node, BinaryWriter writer)
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
        protected static NodeBase Hydrate(GraphFactory factory, BinaryReader reader)
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
        public NodeBase FindSubNodeByNameOrThrow(string name)
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

        /// <summary>
        /// Removes the wire that connects this from a direct descendant
        /// </summary>
        /// <param name="directDescendant"></param>
        public void RemoveDirectDescendant(NodeBase directDescendant)
        {
            var wire = _output.Single(w => w.SendTo == directDescendant);
            _output.Remove(wire);
        }

        public virtual void ApplyError(ErrorType type, ITensor delta, ILearningContext context)
        {
            throw new NotImplementedException();
        }
    }
}
