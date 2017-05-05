using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.ExecutionGraph.Execution
{
    public class TrainingEngine2
    {
        public enum GraphDataType
        {
            Vector,
            Matrix,
            Tensor
        }

        public interface IGraphData
        {
            GraphDataType CurrentType { get; }
        }

        public interface IWire
        {
            INode SendTo { get; }
            bool IsPrimary { get; }
        }

        public interface IAction
        {
            INode Source { get; }
            IGraphData Data { get; }
        }

        public class Action : IAction
        {
            readonly INode _source;
            readonly IGraphData _data;

            public Action(INode source, IGraphData data)
            {
                _source = source;
                _data = data;
            }

            public INode Source => _source;
            public IGraphData Data => _data;
        }

        public class Wire : IWire
        {
            readonly INode _node;
            readonly bool _isPrimary;

            public Wire(INode node, bool isPrimary)
            {
                _node = node;
                _isPrimary = isPrimary;
            }

            public INode SendTo => _node;
            public bool IsPrimary => _isPrimary;
        }

        public interface IContext
        {
            IGraphData Data { get; }
            void Add(IAction action);
        }

        public interface INode
        {
            string Id { get; }
            string Name { get; }
            IReadOnlyList<IWire> Output { get; }
            void SetPrimaryInput(IContext context);
            void SetSecondaryInput(IContext context);
            void AddOutput(IWire wire);
        }

        public abstract class NodeBase : INode
        {
            readonly string _id, _name;
            readonly List<IWire> _output = new List<IWire>();

            public NodeBase(string name)
            {
                _id = Guid.NewGuid().ToString("n");
                _name = name;
            }

            public string Id => _id;
            public string Name => _name;
            public IReadOnlyList<IWire> Output => _output;

            public void AddOutput(IWire wire) => _output.Add(wire);
            public abstract void SetPrimaryInput(IContext context);
            public virtual void SetSecondaryInput(IContext context)
            {
                SetPrimaryInput(context);
            }
        }

        public abstract class GateBase : NodeBase
        {
            IContext _primary = null, _secondary = null;

            public GateBase(string name) : base(name) { }

            public override void SetPrimaryInput(IContext context)
            {
                _primary = context;
            }

            public override void SetSecondaryInput(IContext context)
            {
                _secondary = context;
            }

            void _TryComplete()
            {
                if (_primary != null && _secondary != null) {
                    _Activate(_primary, _secondary);
                    _primary = _secondary = null;
                }
            }

            protected abstract void _Activate(IContext primary, IContext secondary);
        }

        public class FeedForward : NodeBase
        {
            public FeedForward(string name = null) : base(name)
            {
            }

            public override void SetPrimaryInput(IContext context)
            {
                context.Add(new Action(this, context.Data));
            }
        }

        public class Add : GateBase
        {
            public Add(string name) : base(name) { }

            protected override void _Activate(IContext primary, IContext secondary)
            {
                primary.Add(new Action(this, primary.Data));
            }
        }
    }
}
