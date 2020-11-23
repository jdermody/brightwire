using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData.FloatTensors;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// LSTM recurrent neural network
    /// https://en.wikipedia.org/wiki/Long_short-term_memory
    /// </summary>
    class LongShortTermMemory : NodeBase, IHaveMemoryNode
    {
        uint _inputSize;
        MemoryFeeder _memory, _state;
        INode _input, _output = null;
        IReadOnlyDictionary<INode, IGraphData> _lastBackpropagation = null;
        OneToMany _start;

        public LongShortTermMemory(GraphFactory graph, uint inputSize, float[] memory, string name = null) : base(name)
        {
            _Create(graph, inputSize, memory, null);
        }

        void _Create(GraphFactory graph, uint inputSize, float[] memory, string memoryId)
        {
            _inputSize = inputSize;
            var hiddenLayerSize = (uint)memory.Length;
            _memory = new MemoryFeeder(graph.Context, memory, null, memoryId);
            _state = new MemoryFeeder(graph.Context, new float[memory.Length]);
            _input = new FlowThrough();

            var Wf = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wf");
            var Wi = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wi");
            var Wo = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wo");
            var Wc = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wc");

            var Uf = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uf");
            var Ui = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ui");
            var Uo = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uo");
            var Uc = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uc");

            var Ft = graph.Add(Wf, Uf).Add(graph.SigmoidActivation("Ft"));
            var It = graph.Add(Wi, Ui).Add(graph.SigmoidActivation("It"));
            var Ot = graph.Add(Wo, Uo).Add(graph.SigmoidActivation("Ot"));

            var ftCt1 = graph.Multiply(hiddenLayerSize, Ft.LastNode, _state);
            var Ct = graph.Add(ftCt1, graph.Multiply(It, graph.Add(Wc, Uc).Add(graph.TanhActivation())))
                .AddForwardAction(_state.SetMemoryAction)
            ;

            _output = graph.Multiply(Ot, Ct.Add(graph.TanhActivation()))
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
                yield return _state;
            }
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("LSTM", _WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wf = (FeedForward)_input.FindByName("Wf");
            var Wi = (FeedForward)_input.FindByName("Wi");
            var Wo = (FeedForward)_input.FindByName("Wo");
            var Wc = (FeedForward)_input.FindByName("Wc");
            var Uf = (FeedForward)_memory.FindByName("Uf");
            var Ui = (FeedForward)_memory.FindByName("Ui");
            var Uo = (FeedForward)_memory.FindByName("Uo");
            var Uc = (FeedForward)_memory.FindByName("Uc");

            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
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
            var inputSize = (uint)reader.ReadInt32();
            var memoryId = reader.ReadString();
            var memory = FloatVector.ReadFrom(factory.Context, reader);

            if (_memory == null)
                _Create(factory, inputSize, memory.Segment.ToArray(), memoryId);
            else
                _memory.Data = memory;

            var Wf = (FeedForward)_input.FindByName("Wf");
            var Wi = (FeedForward)_input.FindByName("Wi");
            var Wo = (FeedForward)_input.FindByName("Wo");
            var Wc = (FeedForward)_input.FindByName("Wc");
            var Uf = (FeedForward)_memory.FindByName("Uf");
            var Ui = (FeedForward)_memory.FindByName("Ui");
            var Uo = (FeedForward)_memory.FindByName("Uo");
            var Uc = (FeedForward)_memory.FindByName("Uc");

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
