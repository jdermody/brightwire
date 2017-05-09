using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightWire.Models;
using System.IO;
using System.Diagnostics;

namespace BrightWire.ExecutionGraph.Node
{
    public abstract class NodeBase : INode
    {
        string _id, _name;
        readonly List<IWire> _output = new List<IWire>();

        public NodeBase(string name, string id = null)
        {
            _id = id ?? Guid.NewGuid().ToString("n");
            _name = name;
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
            if(_output.Any())
                context.Forward(new GraphAction(this, data, context.Source), backProp);
        }

        public virtual void SerialiseTo(List<Models.ExecutionGraph.Node> nodeList, List<Models.ExecutionGraph.Wire> wireList)
        {
            var info = _GetInfo();
            nodeList.Add(new Models.ExecutionGraph.Node {
                Id = _id,
                Name = _name,
                Data = info.Data,
                Description = info.Description,
                TypeName = GetType().FullName
            });
            foreach(var wire in Output) {
                wireList.Add(new Models.ExecutionGraph.Wire {
                    FromId = _id,
                    InputChannel = wire.Channel,
                    ToId = wire.SendTo.Id
                });
            }
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

        public virtual void ReadFrom(BinaryReader reader)
        {
            // nop
        }

        void ICanInitialiseNode.Initialise(string id, string name, byte[] data)
        {
            _id = id;
            _name = name;
            _Initalise(data);
        }
        protected virtual void _Initalise(byte[] data)
        {
            Debug.Assert(data == null);
        }

        public INode SearchFor(string name)
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
    }
}
