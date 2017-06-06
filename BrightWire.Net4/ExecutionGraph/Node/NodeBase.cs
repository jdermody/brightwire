using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using ProtoBuf;

namespace BrightWire.ExecutionGraph.Node
{
    /// <summary>
    /// Base class for graph nodes
    /// </summary>
    public abstract class NodeBase : INode
    {
        string _id, _name;
        List<IWire> _output = new List<IWire>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the node (optional)</param>
        /// <param name="id">The node's unique id (optional)</param>
        public NodeBase(string name, string id = null)
        {
            _id = id ?? Guid.NewGuid().ToString("n");
            _name = name;
        }

        /// <summary>
        /// Called when serialising the node
        /// </summary>
        /// <param name="graph">Graph factory</param>
        /// <param name="description">Node description</param>
        /// <param name="data">Node serialisation data</param>
        protected virtual void _Initalise(GraphFactory graph, string description, byte[] data)
        {
            if(data != null)
                _ReadFrom(data, reader => ReadFrom(graph, reader));
        }

        #region Disposal
        /// <summary>
        /// Destructor
        /// </summary>
        ~NodeBase()
        {
            _Dispose(false);
        }
        /// <summary>
        /// Disposal
        /// </summary>
        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposal
        /// </summary>
        /// <param name="isDisposing"></param>
        protected virtual void _Dispose(bool isDisposing) { } 
        #endregion

        /// <summary>
        /// The node's unique id
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// The node's name in the graph
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// The list of outgoing wires along which the output signal will be sent
        /// </summary>
        public virtual List<IWire> Output => _output;

        /// <summary>
        /// Called when executing forward on the primary channel
        /// </summary>
        /// <param name="context"></param>
        public abstract void ExecuteForward(IContext context);

        /// <summary>
        /// Called when executing forward on a non-primary channel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        protected virtual void _ExecuteForward(IContext context, int channel)
        {
            ExecuteForward(context);
        }

        /// <summary>
        /// Called when executing forward
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        public void ExecuteForward(IContext context, int channel)
        {
            if (channel == 0)
                ExecuteForward(context);
            else
                _ExecuteForward(context, channel);
        }

        /// <summary>
        /// Records the node execution and queues the output nodes for execution
        /// </summary>
        /// <param name="context"></param>
        /// <param name="data"></param>
        /// <param name="backProp"></param>
        protected void _AddNextGraphAction(IContext context, IGraphData data, Func<IBackpropagation> backProp)
        {
            context.AddForward(new TrainingAction(this, data, context.Source), backProp);
        }

        /// <summary>
        /// Serialise this node and any connected nodes
        /// </summary>
        /// <param name="connectedTo">The list of nodes this node is connected to</param>
        /// <param name="wireList">The list of wires between all connected nodes</param>
        /// <returns></returns>
        public virtual Models.ExecutionGraph.Node SerialiseTo(List<Models.ExecutionGraph.Node> connectedTo, List<Models.ExecutionGraph.Wire> wireList)
        {
            var info = _GetInfo();
            var ret = new Models.ExecutionGraph.Node {
                Id = _id,
                Name = _name,
                Data = info.Data,
                Description = info.Description,
                TypeName = GetType().FullName
            };

            // get the connected nodes
            if (connectedTo != null && wireList != null) {
                foreach (var wire in Output) {
                    var sendTo = wire.SendTo;
                    wireList.Add(new Models.ExecutionGraph.Wire {
                        FromId = _id,
                        InputChannel = wire.Channel,
                        ToId = sendTo.Id
                    });
                    connectedTo.Add(sendTo.SerialiseTo(connectedTo, wireList));
                }
            }
            return ret;
        }

        /// <summary>
        /// Returns serialisation information
        /// </summary>
        protected virtual (string Description, byte[] Data) _GetInfo()
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
        void ICanInitialiseNode.Initialise(GraphFactory factory, string id, string name, string description, byte[] data)
        {
            _id = id;
            _name = name;
            if (_output == null)
                _output = new List<IWire>();
            _Initalise(factory, description, data);
        }

        /// <summary>
        /// Finds a connected node by friendly name
        /// </summary>
        /// <param name="name">The node's name to search for</param>
        /// <returns></returns>
        public INode FindByName(string name)
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
        public INode FindById(string id)
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
        protected static void _Serialise(INode node, BinaryWriter writer)
        {
            using (var buffer = new MemoryStream()) {
                Serializer.Serialize(buffer, node.SerialiseTo(null, null));
                var activationData = buffer.ToArray();
                writer.Write(activationData.Length);
                writer.Write(activationData);
            }
        }

        /// <summary>
        /// Helper function to write data to a binary writer
        /// </summary>
        /// <param name="callback">Callback to receive the writer</param>
        /// <returns></returns>
        protected static byte[] _WriteData(Action<BinaryWriter> callback)
        {
            using (var stream = new MemoryStream()) {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    callback(writer);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Helper function to read from a binary reader
        /// </summary>
        /// <param name="data">The data to read</param>
        /// <param name="callback">Callback to receive the writer</param>
        protected static void _ReadFrom(byte[] data, Action<BinaryReader> callback)
        {
            using (var reader = new BinaryReader(new MemoryStream(data), Encoding.UTF8)) {
                callback(reader);
            }
        }

        /// <summary>
        /// Reads serialisation information and creates a node
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected static INode _Hydrate(GraphFactory factory, BinaryReader reader)
        {
            var bufferSize = reader.ReadInt32();
            Models.ExecutionGraph.Node model;
            using (var buffer = new MemoryStream(reader.ReadBytes(bufferSize)))
                model = Serializer.Deserialize<Models.ExecutionGraph.Node>(buffer);
            return factory?.Create(model);
        }

        /// <summary>
        /// Loads parameters into an existing node
        /// </summary>
        /// <param name="nodeData">Serialised node parameters</param>
        public virtual void LoadParameters(Models.ExecutionGraph.Node nodeData)
        {
            if(nodeData.Data != null)
                _ReadFrom(nodeData.Data, reader => ReadFrom(null, reader));
        }
    }
}
