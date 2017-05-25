using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using System.IO;
using System.Diagnostics;
using ProtoBuf;

namespace BrightWire.ExecutionGraph.Node
{
    public abstract class NodeBase : INode
    {
        string _id, _name;
        List<IWire> _output = new List<IWire>();

        public NodeBase(string name, string id = null)
        {
            _id = id ?? Guid.NewGuid().ToString("n");
            _name = name;
        }

        protected virtual void _Initalise(GraphFactory graph, string description, byte[] data)
        {
            Debug.Assert(data == null);
        }

        #region Disposal
        ~NodeBase()
        {
            _Dispose(false);
        }
        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void _Dispose(bool isDisposing) { } 
        #endregion

        public string Id => _id;
        public string Name => _name;
        public virtual List<IWire> Output => _output;

        public abstract void ExecuteForward(IContext context);
        public virtual void _ExecuteForward(IContext context, int channel)
        {
            ExecuteForward(context);
        }
        public void ExecuteForward(IContext context, int channel)
        {
            if (channel == 0)
                ExecuteForward(context);
            else
                _ExecuteForward(context, channel);
        }

        protected void _AddNextGraphAction(IContext context, IGraphData data, Func<IBackpropagation> backProp)
        {
            context.AddForward(new TrainingAction(this, data, context.Source), backProp);
        }

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

        protected virtual (string Description, byte[] Data) _GetInfo()
        {
            return (GetType().Name, null);
        }

        protected byte[] _WriteData(Action<BinaryWriter> callback)
        {
            using (var stream = new MemoryStream()) {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    callback(writer);
                return stream.ToArray();
            }
        }

        protected void _ReadFrom(byte[] data, Action<BinaryReader> callback)
        {
            using (var reader = new BinaryReader(new MemoryStream(data), Encoding.UTF8)) {
                callback(reader);
            }
        }

        public virtual void WriteTo(BinaryWriter writer)
        {
            // nop
        }

        public virtual void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            // nop
        }

        void ICanInitialiseNode.Initialise(GraphFactory factory, string id, string name, string description, byte[] data)
        {
            _id = id;
            _name = name;
            if (_output == null)
                _output = new List<IWire>();
            _Initalise(factory, description, data);
        }

        public INode Find(string name)
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

        public virtual IEnumerable<INode> SubNodes => Enumerable.Empty<INode>();

        protected void _Serialise(INode node, BinaryWriter writer)
        {
            using (var buffer = new MemoryStream()) {
                Serializer.Serialize(buffer, node.SerialiseTo(null, null));
                var activationData = buffer.ToArray();
                writer.Write(activationData.Length);
                writer.Write(activationData);
            }
        }

        protected INode _Hydrate(GraphFactory factory, BinaryReader reader)
        {
            var bufferSize = reader.ReadInt32();
            Models.ExecutionGraph.Node model;
            using (var buffer = new MemoryStream(reader.ReadBytes(bufferSize)))
                model = Serializer.Deserialize<Models.ExecutionGraph.Node>(buffer);
            return factory.Create(model);
        }

        public virtual void OnDeserialise(IReadOnlyDictionary<string, INode> graph)
        {
            // nop
        }

        public void RemoveChildenFromGraph()
        {
            
        }
    }
}
