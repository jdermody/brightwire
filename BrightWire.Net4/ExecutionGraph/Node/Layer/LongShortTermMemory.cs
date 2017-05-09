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
        readonly INode _output = null, _input;

        public LongShortTermMemory(GraphFactory graph, int inputSize, float[] memory, string name = null) : base(name)
        {
            int hiddenLayerSize = memory.Length;
            _memoryFeeder = new MemoryFeeder(_id, graph.LinearAlgebraProvider, memory);
            _memoryFeeder2 = new MemoryFeeder(_id + ":output", graph.LinearAlgebraProvider, new float[memory.Length]);
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

        public override List<IWire> Output => _output?.Output ?? base.Output;

        public override void SetPrimaryInput(IContext context)
        {
            // fire the memory channel
            if (context.BatchSequence.SequenceIndex == 1) {
                _memoryFeeder.OnStart(context);
                _memoryFeeder2.OnStart(context);
            } else {
                _memoryFeeder.OnNext(context);
                _memoryFeeder2.OnNext(context);
            }

            // fire the input channel
            _input.SetPrimaryInput(context);
        }
    }
}
