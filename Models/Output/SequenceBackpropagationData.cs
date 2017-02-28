using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Models.Output
{
    public class SequenceBackpropagationData
    {
        readonly Stack<INeuralNetworkRecurrentBackpropagation> _layerStack = new Stack<INeuralNetworkRecurrentBackpropagation>();
        IMatrix _layerOutput, _expectedOutput;
        readonly ISequentialMiniBatch _batch;
        readonly int _sequenceIndex;

        public SequenceBackpropagationData(ISequentialMiniBatch batch, int sequenceIndex)
        {
            _batch = batch;
            _sequenceIndex = sequenceIndex;
        }

        public void Add(INeuralNetworkRecurrentBackpropagation backprop)
        {
            _layerStack.Push(backprop);
        }

        public int SequenceIndex { get { return _sequenceIndex; } }
        public ISequentialMiniBatch MiniBatch { get { return _batch; } }
        public Stack<INeuralNetworkRecurrentBackpropagation> LayerBackProp { get { return _layerStack; } }

        public IMatrix Output
        {
            get { return _layerOutput; }
            internal set { _layerOutput = value; }
        }

        public IMatrix ExpectedOutput
        {
            get { return _expectedOutput; }
            set { _expectedOutput = value; }
        }
    }
}
