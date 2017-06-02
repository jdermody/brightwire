using BrightWire.ExecutionGraph.Action;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Helper;

namespace BrightWire.ExecutionGraph
{
    /// <summary>
    /// Wires nodes together to build strands of a graph
    /// </summary>
    public class WireBuilder
    {
        readonly GraphFactory _factory;
        readonly INode _first;
        INode _node;
        int _size;

        /// <summary>
        /// Connects new nodes starting from the specified node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="size">Initial wire size</param>
        /// <param name="node">The node to build from</param>
        public WireBuilder(GraphFactory factory, int size, INode node)
        {
            _factory = factory;
            _first = _node = node;
            _size = size;
        }

        /// <summary>
        /// Connects new nodes to the engine output node
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="engine"></param>
        public WireBuilder(GraphFactory factory, IGraphEngine engine) 
            : this(factory, engine.DataSource.InputSize, engine.Input)
        {
        }

        /// <summary>
        /// The current wire size
        /// </summary>
        public int CurrentSize => _size;

        /// <summary>
        /// Changes the current wire's input size
        /// </summary>
        /// <param name="delta">Amount to add to the current wire size</param>
        /// <returns></returns>
        public WireBuilder IncrementSizeBy(int delta)
        {
            _size += delta;
            return this;
        }

        void _SetNode(INode node)
        {
            if (_node != null)
                _node.Output.Add(new WireToNode(node));
            _node = node;
        }

        /// <summary>
        /// Connects a row classifier
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="dataTable"></param>
        /// <param name="analysis"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddClassifier(IRowClassifier classifier, IDataTable dataTable, IDataTableAnalysis analysis = null, string name = null)
        {
            var node = _factory.CreateClassifier(classifier, dataTable, analysis, name);
            _SetNode(node.RowClassifier);
            _size = node.OutputSize;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputSize"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddFeedForward(int outputSize, string name = null)
        {
            INode node = _factory.CreateFeedForward(_size, outputSize, name);
            _SetNode(node);
            _size = outputSize;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddTiedFeedForward(IFeedForward layer, string name = null)
        {
            _SetNode(_factory.CreateTiedFeedForward(layer, name));
            _size = layer.InputSize;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dropOutPercentage"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddDropOut(float dropOutPercentage, string name = null)
        {
            _SetNode(_factory.CreateDropOut(dropOutPercentage, name));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dropOutPercentage"></param>
        /// <param name="outputSize"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddDropConnect(float dropOutPercentage, int outputSize, string name = null)
        {
            _SetNode(_factory.CreateDropConnect(dropOutPercentage, _size, outputSize, name));
            _size = outputSize;
            return this;
        }

        public WireBuilder Add(INode node)
        {
            _SetNode(node);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddForwardAction(IAction action, string name = null)
        {
            _SetNode(new ExecuteForwardAction(action, name));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackwardAction(IAction action, string name = null)
        {
            _SetNode(new ExecuteBackwardAction(action, name));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activation"></param>
        /// <param name="initialMemory"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddSimpleRecurrent(INode activation, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateSimpleRecurrent(_size, initialMemory, activation, name));
            _size = initialMemory.Length;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activation"></param>
        /// <param name="activation2"></param>
        /// <param name="initialMemory"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddElman(INode activation, INode activation2, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateElman(_size, initialMemory, activation, activation2, name));
            _size = initialMemory.Length;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activation"></param>
        /// <param name="activation2"></param>
        /// <param name="initialMemory"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddJordan(INode activation, INode activation2, float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateJordan(_size, initialMemory, activation, activation2, name));
            _size = initialMemory.Length;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialMemory"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddGru(float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateGru(_size, initialMemory, name));
            _size = initialMemory.Length;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialMemory"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddLstm(float[] initialMemory, string name = null)
        {
            _SetNode(_factory.CreateLstm(_size, initialMemory, name));
            _size = initialMemory.Length;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddMaxPooling(int width, int height, int stride, string name = null)
        {
            _SetNode(_factory.CreateMaxPool(width, height, stride, name));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder ReverseSequence(string name = null)
        {
            _SetNode(_factory.CreateSequenceReverser(name));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputDepth"></param>
        /// <param name="filterCount"></param>
        /// <param name="padding"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="stride"></param>
        /// <param name="shouldBackpropagate"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddConvolutional(int inputDepth, int filterCount, int padding, int filterWidth, int filterHeight, int stride, bool shouldBackpropagate = true, string name = null)
        {
            _SetNode(_factory.CreateConvolutional(inputDepth, filterCount, padding, filterWidth, filterHeight, stride, shouldBackpropagate, name));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSize"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Transpose(int newSize, string name = null)
        {
            // TODO: calculate the graph output size by executing the graph up to this point?
            _SetNode(new TransposeSignal(name));
            _size = newSize;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMetric"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackpropagation(IErrorMetric errorMetric, string name = null)
        {
            AddForwardAction(new Backpropagate(errorMetric), name);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMetric"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddBackpropagationThroughTime(IErrorMetric errorMetric, string name = null)
        {
            AddForwardAction(new BackpropagateThroughTime(errorMetric), name);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder ConstrainErrorSignal(float min = -1f, float max = 1f, string name = null)
        {
            AddForwardAction(new ConstrainErrorSignal(min, max), name);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="node"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder WriteNodeMemoryToSlot(string slotName, IHaveMemoryNode node, string name = null)
        {
            AddForwardAction(new CopyNamedMemory(slotName, node), name);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder JoinInputWithMemory(string slotName, string name = null)
        {
            AddForwardAction(new JoinInputWithMemory(slotName), name);
            return this;
        }

        /// <summary>
        /// Tries to find the specified node
        /// </summary>
        /// <param name="name">The friendly name of the node</param>
        /// <returns></returns>
        public INode Find(string name)
        {
            return _first.Find(name);
        }

        /// <summary>
        /// The last added node
        /// </summary>
        public INode LastNode => _node;
    }
}
