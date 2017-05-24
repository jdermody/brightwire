using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Gate;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class GatedRecurrentUnit : NodeBase, IHaveMemoryNode
    {
        IReadOnlyDictionary<INode, IGraphData> _lastBackpropagation = null;
        int _inputSize;
        MemoryFeeder _memory;
        INode _input, _output = null;
        OneToMany _start;

        public GatedRecurrentUnit(GraphFactory graph, int inputSize, float[] memory, string name = null)
            : base(name)
        {
            _Create(graph, inputSize, memory, null);
        }

        void _Create(GraphFactory graph, int inputSize, float[] memory, string memoryId)
        {
            _inputSize = inputSize;
            int hiddenLayerSize = memory.Length;
            _memory = new MemoryFeeder(memory, null, memoryId);
            _input = new FlowThrough();

            //graph.PushPropertySet(ps => ps.Use(graph.WeightInitialisation.Zeroes));
            var Wz = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wz");
            var Uz = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uz");
            //graph.PopPropertyStack();

            //graph.PushPropertySet(ps => ps.Use(graph.WeightInitialisation.Ones));
            var Wr = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wr");
            var Ur = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ur");
            //graph.PopPropertyStack();

            // add sigmoids to the gates
            var Rt = graph.Add(Wr, Ur).AddBackwardAction(new ConstrainErrorSignal()).Add(graph.SigmoidActivation("Rt")).Build();
            var Zt = graph.Add(Wz, Uz).AddBackwardAction(new ConstrainErrorSignal()).Add(graph.SigmoidActivation("Zt")).Build();

            // h1 = tanh(Wh(x) + Uh(Ht1xRt))
            var Wh = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var Uh = graph.Multiply(hiddenLayerSize, Rt, _memory).AddFeedForward(hiddenLayerSize, "Uh");
            var h1 = graph.Add(Wh, Uh).AddBackwardAction(new ConstrainErrorSignal()).Add(graph.TanhActivation());

            // h2 = h1x(1-Zt)
            var h2 = graph.Multiply(h1, graph.Build(hiddenLayerSize, Zt).Add(graph.CreateOneMinusInput()));

            // h = h1xh2
            var previous = graph.Multiply(hiddenLayerSize, Zt, _memory);
            _output = graph
                .Add(h2, previous)
                .AddForwardAction(_memory.SetMemoryAction)
                .Add(new RestoreErrorSignal(context => {
                    if (_lastBackpropagation != null) {
                        foreach (var item in _lastBackpropagation)
                            context.AppendErrorSignal(item.Value, item.Key);
                        _lastBackpropagation = null;
                    }
                }))
                .Build()
            ;
            _start = new OneToMany(SubNodes, bp => _lastBackpropagation = bp);
        }

        public override List<IWire> Output => _output.Output;
        public INode Memory => _memory;

        public override void ExecuteForward(IContext context)
        {
            if (context.BatchSequence.Type == MiniBatchType.SequenceStart)
                _lastBackpropagation = null;

            _start.ExecuteForward(context);
        }

        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("GRU", _WriteData(WriteTo));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = _input.Find("Wh") as INode;
            var Wr = _input.Find("Wr") as INode;
            var Wz = _input.Find("Wz") as INode;
            var Uh = _memory.Find("Uh") as INode;
            var Ur = _memory.Find("Ur") as INode;
            var Uz = _memory.Find("Uz") as INode;

            writer.Write(_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);

            Wh.WriteTo(writer);
            Wr.WriteTo(writer);
            Wz.WriteTo(writer);
            Uh.WriteTo(writer);
            Ur.WriteTo(writer);
            Uz.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = FloatVector.ReadFrom(reader);

            if (_memory == null)
                _Create(factory, inputSize, memory.Data, memoryId);
            else
                _memory.Data = memory;

            var Wh = _input.Find("Wh") as INode;
            var Wr = _input.Find("Wr") as INode;
            var Wz = _input.Find("Wz") as INode;
            var Uh = _memory.Find("Uh") as INode;
            var Ur = _memory.Find("Ur") as INode;
            var Uz = _memory.Find("Uz") as INode;

            Wh.ReadFrom(factory, reader);
            Wr.ReadFrom(factory, reader);
            Wz.ReadFrom(factory, reader);
            Uh.ReadFrom(factory, reader);
            Ur.ReadFrom(factory, reader);
            Uz.ReadFrom(factory, reader);
        }
    }
}
