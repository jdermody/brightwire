using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.Models;
using System.Collections.Generic;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// GRU recurrent neural network
    /// https://en.wikipedia.org/wiki/Gated_recurrent_unit
    /// </summary>
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

            var Wz = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wz");
            var Uz = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uz");

            var Wr = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wr");
            var Ur = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ur");

            // add sigmoids to the gates
            var Rt = graph.Add(Wr, Ur).AddBackwardAction(new ConstrainSignal()).Add(graph.SigmoidActivation("Rt")).LastNode;
            var Zt = graph.Add(Wz, Uz).AddBackwardAction(new ConstrainSignal()).Add(graph.SigmoidActivation("Zt")).LastNode;

            // h1 = tanh(Wh(x) + Uh(Ht1xRt))
            var Wh = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var Uh = graph.Multiply(hiddenLayerSize, Rt, _memory).AddFeedForward(hiddenLayerSize, "Uh");
            var h1 = graph.Add(Wh, Uh).AddBackwardAction(new ConstrainSignal()).Add(graph.TanhActivation());

            // h2 = h1x(1-Zt)
            var h2 = graph.Multiply(h1, graph.Connect(hiddenLayerSize, Zt).Add(graph.CreateOneMinusInput()));

            // h = h1xh2
            var previous = graph.Multiply(hiddenLayerSize, Zt, _memory);
            _output = graph
                .Add(h2, previous)
                .AddForwardAction(_memory.SetMemoryAction)
                //.Add(new HookErrorSignal(context => {
                //    if (_lastBackpropagation != null) {
                //        foreach (var item in _lastBackpropagation)
                //            context.AppendErrorSignal(item.Value, item.Key);
                //        _lastBackpropagation = null;
                //    }
                //}))
                .LastNode
            ;
            _start = new OneToMany(SubNodes, bp => _lastBackpropagation = bp);
        }

        public override List<IWire> Output => _output.Output;
        public INode Memory => _memory;

        public override void ExecuteForward(IContext context)
        {
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart)
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

        public override void WriteTo(BinaryWriter writer)
        {
            var Wh = _input.FindByName("Wh");
            var Wr = _input.FindByName("Wr");
            var Wz = _input.FindByName("Wz");
            var Uh = _memory.FindByName("Uh");
            var Ur = _memory.FindByName("Ur");
            var Uz = _memory.FindByName("Uz");

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

            var Wh = _input.FindByName("Wh");
            var Wr = _input.FindByName("Wr");
            var Wz = _input.FindByName("Wz");
            var Uh = _memory.FindByName("Uh");
            var Ur = _memory.FindByName("Ur");
            var Uz = _memory.FindByName("Uz");

            Wh.ReadFrom(factory, reader);
            Wr.ReadFrom(factory, reader);
            Wz.ReadFrom(factory, reader);
            Uh.ReadFrom(factory, reader);
            Ur.ReadFrom(factory, reader);
            Uz.ReadFrom(factory, reader);
        }
    }
}
