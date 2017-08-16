using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.ExecutionGraph.Node.Operation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Layer
{
    /// <summary>
    /// Batch normalisation layer
    /// https://arxiv.org/abs/1502.03167
    /// </summary>
    class BatchNormalisation : NodeBase
    {
        int _inputSize;
        FlowThrough _input;
        MultiplyWithParameter _gamma;
        MultiplyWithParameter _beta;
        INode _output, _input2;
        OneToMany _start;

        public BatchNormalisation(GraphFactory graph, int inputSize, string name = null) : base(name)
        {
            _inputSize = inputSize;
            _Create(graph, 1f, 0f);
        }

        void _Create(GraphFactory graph, float gamma, float beta)
        {
            _input = new FlowThrough();
            _input2 = new CalculateVariance();
            var main = graph.Subtract(_inputSize, _input, _input2);
            var mainNode = main.LastNode;
            main.Add(new InputSquared());
            main.Add(new CalculateVariance());
            main.Add(new SquareRootOfInput());
            main.Add(new OneDividedByInput());
            main = graph.Multiply(_inputSize, mainNode, main.LastNode);

            _gamma = new MultiplyWithParameter(gamma);
            _beta = new MultiplyWithParameter(beta);

            main = graph.Multiply(_inputSize, _gamma, main.LastNode);
            _output = graph.Add(_inputSize, _beta, main.LastNode).LastNode;
            _start = new OneToMany(SubNodes, null);
        }

        public override void ExecuteForward(IContext context)
        {
            _start.ExecuteForward(context);
        }

        public override List<IWire> Output => _output.Output;

        public override IEnumerable<INode> SubNodes
        {
            get
            {
                yield return _input;
                yield return _input2;
                yield return _gamma;
                yield return _beta;
            }
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("BN", _WriteData(WriteTo));
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _inputSize = reader.ReadInt32();
            var gamma = reader.ReadSingle();
            var beta = reader.ReadSingle();
            if (_gamma == null)
                _Create(factory, gamma, beta);
            else {
                _gamma.Parameter = gamma;
                _beta.Parameter = beta;
            }
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_inputSize);
            writer.Write(_gamma.Parameter);
            writer.Write(_beta.Parameter);
        }
    }
}
