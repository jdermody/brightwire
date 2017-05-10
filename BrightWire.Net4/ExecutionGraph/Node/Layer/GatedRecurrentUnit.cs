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
    class GatedRecurrentUnit : NodeBase
    {
        int _inputSize;
        MemoryFeeder _memory;
        INode _input, _output = null;

        public GatedRecurrentUnit(GraphFactory graph, int inputSize, float[] memory, string name = null)
            : base(name)
        {
            _Create(graph, inputSize, memory);
        }

        void _Create(GraphFactory graph, int inputSize, float[] memory)
        {
            _inputSize = inputSize;
            int hiddenLayerSize = memory.Length;
            _memory = new MemoryFeeder(memory);
            _input = new FlowThrough();

            // set the reset gate to all ones
            var Wr = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wr");
            graph.PushPropertySet(ps => ps.Use(graph.WeightInitialisation.Ones));
            var Ur = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ur");
            graph.PopPropertyStack();

            // set the update gate to all zeroes
            var Wz = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wz");
            graph.PushPropertySet(ps => ps.Use(graph.WeightInitialisation.Zeroes));
            var Uz = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uz");
            graph.PopPropertyStack();

            // add sigmoids to the gates
            var Rt = graph.Add(Wr, Ur).Add(graph.SigmoidActivation("Rt")).Build();
            var Zt = graph.Add(Wz, Uz).Add(graph.SigmoidActivation("Zt")).Build();

            // h1 = tanh(Wh(x) + Uh(Ht1xRt))
            var Wh = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var rtHt1 = graph.Multiply(hiddenLayerSize, Rt, _memory);
            rtHt1.AddFeedForward(hiddenLayerSize, "Uh");
            var h1 = graph.Add(Wh, rtHt1).Add(graph.TanhActivation());

            // h2 = h1x(1-Zt)
            var zt1x = graph.Build(hiddenLayerSize, Zt).Add(graph.CreateOneMinusInput());
            var h2 = graph.Multiply(h1, zt1x);

            // h = h1xh2
            var ztHt1 = graph.Multiply(hiddenLayerSize, Zt, _memory);
            _output = graph
                .Add(h2, ztHt1)
                .Add(_memory.SetMemoryAction)
                .Build()
            ;
        }

        public override List<IWire> Output => _output.Output;

        public override void ExecuteForward(IContext context)
        {
            foreach (var node in SubNodes)
                node.ExecuteForward(context, 0);
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
            var Wh = _input.SearchFor("Wh") as INode;
            var Wr = _input.SearchFor("Wr") as INode;
            var Wz = _input.SearchFor("Wz") as INode;
            var Uh = _memory.SearchFor("Uh") as INode;
            var Ur = _memory.SearchFor("Ur") as INode;
            var Uz = _memory.SearchFor("Uz") as INode;

            writer.Write(_inputSize);
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
            var memory = FloatVector.ReadFrom(reader);

            if (_memory == null)
                _Create(factory, inputSize, memory.Data);
            else
                _memory.Data = memory;

            var Wh = _input.SearchFor("Wh") as INode;
            var Wr = _input.SearchFor("Wr") as INode;
            var Wz = _input.SearchFor("Wz") as INode;
            var Uh = _memory.SearchFor("Uh") as INode;
            var Ur = _memory.SearchFor("Ur") as INode;
            var Uz = _memory.SearchFor("Uz") as INode;

            Wh.ReadFrom(factory, reader);
            Wr.ReadFrom(factory, reader);
            Wz.ReadFrom(factory, reader);
            Uh.ReadFrom(factory, reader);
            Ur.ReadFrom(factory, reader);
            Uz.ReadFrom(factory, reader);
        }
    }
}
