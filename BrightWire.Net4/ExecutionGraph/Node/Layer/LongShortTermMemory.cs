using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    class LongShortTermMemory : NodeBase
    {
        readonly MemoryFeeder _memoryFeeder, _memoryFeeder2;
        readonly INode _input, _output = null;

        public LongShortTermMemory(GraphFactory graph, int inputSize, float[] memory, string name = null) : base(name)
        {
            int hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(memory);
            _memoryFeeder2 = new MemoryFeeder(new float[memory.Length]);
            _input = new FlowThrough();

            var Wf = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wf");
            var Wi = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wi");
            var Wo = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wo");
            var Wc = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wc");

            var Uf = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize, "Uf");
            var Ui = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize, "Ui");
            var Uo = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize, "Uo");
            var Uc = graph.Build(hiddenLayerSize, _memoryFeeder.MemoryInput).AddFeedForward(hiddenLayerSize, "Uo");

            var Ft = graph.Add(Wf, Uf).Add(graph.SigmoidActivation("Ft"));
            var It = graph.Add(Wi, Ui).Add(graph.SigmoidActivation("It"));
            var Ot = graph.Add(Wo, Uo).Add(graph.SigmoidActivation("Ot"));

            var ftCt1 = graph.Multiply(hiddenLayerSize, Ft.Build(), _memoryFeeder2.MemoryInput);
            var Ct = graph.Add(ftCt1, graph.Multiply(It, graph.Add(Wc, Uc).Add(graph.TanhActivation())))
                .Add(_memoryFeeder2.SetMemoryAction)
            ;
            _output = graph.Multiply(Ot, Ct.Add(graph.TanhActivation()))
                .Add(_memoryFeeder.SetMemoryAction)
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
                yield return _memoryFeeder;
                yield return _memoryFeeder2;
            }
        }
    }
}
