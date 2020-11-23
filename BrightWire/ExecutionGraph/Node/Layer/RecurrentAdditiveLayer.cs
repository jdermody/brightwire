using BrightWire.ExecutionGraph.Node.Input;
using System.Collections.Generic;
using System.IO;
using BrightData.FloatTensors;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Implementation of Recurrent Additive Network
    /// http://www.kentonl.com/pub/llz.2017.pdf
    /// </summary>
    class RecurrentAdditiveLayer : NodeBase, IHaveMemoryNode
    {
        IReadOnlyDictionary<INode, IGraphData> _lastBackpropagation = null;
        uint _inputSize;
        MemoryFeeder _memory;
        INode _input, _output = null;
        OneToMany _start;

        public RecurrentAdditiveLayer(GraphFactory graph, uint inputSize, float[] memory, string name = null) : base(name)
        {
            _Create(graph, inputSize, memory, null);
        }

        void _Create(GraphFactory graph, uint inputSize, float[] memory, string memoryId)
        {
            var hiddenLayerSize = (uint)memory.Length;
            _inputSize = inputSize;

            _memory = new MemoryFeeder(graph.Context, memory, null, memoryId);
            _input = new FlowThrough();

            var Wx = graph.Connect(inputSize, _input).AddFeedForward(hiddenLayerSize, "Wx").LastNode;

            var Wi = graph.Connect(hiddenLayerSize, Wx).AddFeedForward(hiddenLayerSize, "Wi");
            var Ui = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Ui");

            var Wf = graph.Connect(hiddenLayerSize, Wx).AddFeedForward(hiddenLayerSize, "Wf");
            var Uf = graph.Connect(hiddenLayerSize, _memory).AddFeedForward(hiddenLayerSize, "Uf");

            var It = graph.Add(Wi, Ui).Add(graph.SigmoidActivation());
            var Ft = graph.Add(Wf, Uf).Add(graph.SigmoidActivation());

            _output = graph
                .Add(graph.Multiply(hiddenLayerSize, Wx, It.LastNode), graph.Multiply(hiddenLayerSize, _memory, Ft.LastNode))
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

        /// <summary>
        /// Expose the output node so that we can append nodes to it
        /// </summary>
        public override List<IWire> Output => _output.Output;

        public INode Memory => _memory;

        /// <summary>
        /// The list of sub nodes
        /// </summary>
        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _memory;
            }
        }

        /// <summary>
        /// Execute on the the start node, which will execute each sub node in turn
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteForward(IContext context)
        {
            if (context.BatchSequence.Type == MiniBatchSequenceType.SequenceStart)
                _lastBackpropagation = null;

            _start.ExecuteForward(context);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("RAN", _WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            var Wx = _input.FindByName("Wx");
            var Wi = _input.FindByName("Wi");
            var Wf = _input.FindByName("Wf");
            var Ui = _memory.FindByName("Ui");
            var Uf = _memory.FindByName("Uf");

            writer.Write((int)_inputSize);
            writer.Write(_memory.Id);
            _memory.Data.WriteTo(writer);

            Wx.WriteTo(writer);
            Wi.WriteTo(writer);
            Wf.WriteTo(writer);
            Ui.WriteTo(writer);
            Uf.WriteTo(writer);
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

            var Wx = _input.FindByName("Wx");
            var Wi = _input.FindByName("Wi");
            var Wf = _input.FindByName("Wf");
            var Ui = _memory.FindByName("Ui");
            var Uf = _memory.FindByName("Uf");

            Wx.ReadFrom(factory, reader);
            Wi.ReadFrom(factory, reader);
            Wf.ReadFrom(factory, reader);
            Ui.ReadFrom(factory, reader);
            Uf.ReadFrom(factory, reader);
        }
    }
}
