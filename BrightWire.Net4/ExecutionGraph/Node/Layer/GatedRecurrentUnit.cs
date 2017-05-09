using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Gate;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class GatedRecurrentUnit : NodeBase
    {
        readonly MemoryFeeder _memoryFeeder;
        readonly INode _input, _output = null;

        public GatedRecurrentUnit(GraphFactory graph, int inputSize, float[] memory, string name = null)
            : base(name)
        {
            int hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(memory);
            _input = new FlowThrough();

            var Wh = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wh");
            var Wr = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wr");
            var Wz = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wz");

            var Ur = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize, "Ur");
            var Uz = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize, "Uz");

            var Rt = graph.Add(Wr, Ur).Add(graph.SigmoidActivation("Rt"));
            var Zt = graph.Add(Wz, Uz).Add(graph.SigmoidActivation("Zt"));

            var rtHt1 = graph.Multiply(hiddenLayerSize, Rt.Build(), _memoryFeeder.MemoryInput);
            rtHt1.AddFeedForward(hiddenLayerSize, "Uh");

            var ztHt1 = graph.Multiply(hiddenLayerSize, Zt.Build(), _memoryFeeder.MemoryInput);

            var zt1x = Zt.Add(graph.CreateOneMinusInput());

            var main = graph.Add(Wh, rtHt1).Add(graph.TanhActivation());
            var main2 = graph.Multiply(main, zt1x);
            _output = graph.Add(main2, ztHt1).Add(_memoryFeeder.SetMemoryAction).Build();
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
                yield return _memoryFeeder;
            }
        }
    }
}
