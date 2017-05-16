using BrightWire.ExecutionGraph.Helper;
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
    class LongShortTermMemory : NodeBase
    {
        int _inputSize;
        MemoryFeeder _memory, _state;
        INode _input, _output = null;
        IReadOnlyDictionary<INode, IGraphData> _lastBackpropagation = null;
        OneToMany _start;

        public LongShortTermMemory(GraphFactory graph, int inputSize, float[] memory, string name = null) : base(name)
        {
            _Create(graph, inputSize, memory);
        }

        void _Create(GraphFactory graph, int inputSize, float[] memory)
        {
            _inputSize = inputSize;
            int hiddenLayerSize = memory.Length;
            _memory = new MemoryFeeder(memory);
            _state = new MemoryFeeder(new float[memory.Length]);
            _input = new FlowThrough();

            var Wf = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wf");
            var Wi = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wi");
            var Wo = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wo");
            var Wc = graph.Build(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wc");

            var Uf = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uf");
            var Ui = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ui");
            var Uo = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uo");
            var Uc = graph.Build(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uc");

            var Ft = graph.Add(Wf, Uf).Add(graph.SigmoidActivation("Ft"));
            var It = graph.Add(Wi, Ui).Add(graph.SigmoidActivation("It"));
            var Ot = graph.Add(Wo, Uo).Add(graph.SigmoidActivation("Ot"));

            var ftCt1 = graph.Multiply(hiddenLayerSize, Ft.Build(), _state);
            var Ct = graph.Add(ftCt1, graph.Multiply(It, graph.Add(Wc, Uc).Add(graph.TanhActivation())))
                .Add(_state.SetMemoryAction)
            ;

            _output = graph.Multiply(Ot, Ct.Add(graph.TanhActivation()))
                .Add(_memory.SetMemoryAction)
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

        public override void ExecuteForward(IContext context)
        {
            _start.ExecuteForward(context);
        }

        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
                yield return _state;
            }
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("LSTM", _WriteData(WriteTo));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wf = _input.SearchFor("Wf") as FeedForward;
            var Wi = _input.SearchFor("Wi") as FeedForward;
            var Wo = _input.SearchFor("Wo") as FeedForward;
            var Wc = _input.SearchFor("Wc") as FeedForward;
            var Uf = _memory.SearchFor("Uf") as FeedForward;
            var Ui = _memory.SearchFor("Ui") as FeedForward;
            var Uo = _memory.SearchFor("Uo") as FeedForward;
            var Uc = _memory.SearchFor("Uc") as FeedForward;

            writer.Write(_inputSize);
            _memory.Data.WriteTo(writer);

            Wf.WriteTo(writer);
            Wi.WriteTo(writer);
            Wo.WriteTo(writer);
            Wc.WriteTo(writer);

            Uf.WriteTo(writer);
            Ui.WriteTo(writer);
            Uo.WriteTo(writer);
            Uc.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            var inputSize = reader.ReadInt32();
            var memory = FloatVector.ReadFrom(reader);

            if (_memory == null)
                _Create(factory, inputSize, memory.Data);
            else
                _memory.Data = memory;

            var Wf = _input.SearchFor("Wf") as FeedForward;
            var Wi = _input.SearchFor("Wi") as FeedForward;
            var Wo = _input.SearchFor("Wo") as FeedForward;
            var Wc = _input.SearchFor("Wc") as FeedForward;
            var Uf = _memory.SearchFor("Uf") as FeedForward;
            var Ui = _memory.SearchFor("Ui") as FeedForward;
            var Uo = _memory.SearchFor("Uo") as FeedForward;
            var Uc = _memory.SearchFor("Uc") as FeedForward;

            Wf.ReadFrom(factory, reader);
            Wi.ReadFrom(factory, reader);
            Wo.ReadFrom(factory, reader);
            Wc.ReadFrom(factory, reader);

            Uf.ReadFrom(factory, reader);
            Ui.ReadFrom(factory, reader);
            Uo.ReadFrom(factory, reader);
            Uc.ReadFrom(factory, reader);
        }
    }
}
