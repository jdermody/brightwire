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
        int _width, _height, _depth;

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
            _width = size;
            _height = 1;
            _depth = 1;
        }

        /// <summary>
        /// Connects new nodes starting from the specified node
        /// </summary>
        /// <param name="factory">Graph factory</param>
        /// <param name="width">Initial input width</param>
        /// <param name="height">Initial input height</param>
        /// <param name="depth">Initial input depth</param>
        /// <param name="node">The node to build from</param>
        public WireBuilder(GraphFactory factory, int width, int height, int depth, INode node)
        {
            _factory = factory;
            _first = _node = node;
            _width = width;
            _height = height;
            _depth = depth;
        }

        /// <summary>
        /// Connects new nodes to the engine output node
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="engine"></param>
        public WireBuilder(GraphFactory factory, IGraphEngine engine) 
            : this(factory, engine.DataSource.InputSize, engine.Input)
        {
            if(engine.DataSource is IVolumeDataSource volumeDataSource) {
                _width = volumeDataSource.Width;
                _height = volumeDataSource.Height;
                _depth = volumeDataSource.Depth;
            }
        }

        /// <summary>
        /// The current wire size
        /// </summary>
        public int CurrentSize => _width * _height * _depth;

        void _SetNewSize(int newSize)
        {
            _width = newSize;
            _height = 1;
            _depth = 1;
        }

        /// <summary>
        /// Changes the current wire's input size
        /// </summary>
        /// <param name="delta">Amount to add to the current wire size</param>
        /// <returns></returns>
        public WireBuilder IncrementSizeBy(int delta)
        {
            _SetNewSize(_width + delta);
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
            _SetNewSize(node.OutputSize);
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
            INode node = _factory.CreateFeedForward(CurrentSize, outputSize, name);
            _SetNode(node);
            _SetNewSize(outputSize);
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
            _SetNewSize(layer.InputSize);
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
            _SetNode(_factory.CreateDropConnect(dropOutPercentage, CurrentSize, outputSize, name));
            _SetNewSize(outputSize);
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
            _SetNode(_factory.CreateSimpleRecurrent(CurrentSize, initialMemory, activation, name));
            _SetNewSize(initialMemory.Length);
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
            _SetNode(_factory.CreateElman(CurrentSize, initialMemory, activation, activation2, name));
            _SetNewSize(initialMemory.Length);
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
            _SetNode(_factory.CreateJordan(CurrentSize, initialMemory, activation, activation2, name));
            _SetNewSize(initialMemory.Length);
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
            _SetNode(_factory.CreateGru(CurrentSize, initialMemory, name));
            _SetNewSize(initialMemory.Length);
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
            _SetNode(_factory.CreateLstm(CurrentSize, initialMemory, name));
            _SetNewSize(initialMemory.Length);
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
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="stride"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddMaxPooling(int filterWidth, int filterHeight, int stride, string name = null)
        {
            _SetNode(_factory.CreateMaxPool(filterWidth, filterHeight, stride, name));

            _width = (_width - filterWidth) / stride + 1;
            _height = (_height - filterHeight) / stride + 1;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterCount"></param>
        /// <param name="padding"></param>
        /// <param name="filterWidth"></param>
        /// <param name="filterHeight"></param>
        /// <param name="stride"></param>
        /// <param name="shouldBackpropagate"></param>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder AddConvolutional(int filterCount, int padding, int filterWidth, int filterHeight, int stride, bool shouldBackpropagate = true, string name = null)
        {
            _SetNode(_factory.CreateConvolutional(_depth, filterCount, padding, filterWidth, filterHeight, stride, shouldBackpropagate, name));

            _width = (_width + (2 * padding) - filterWidth) / stride + 1;
            _height = (_height + (2 * padding) - filterHeight) / stride + 1;
            _depth = filterCount;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Optional name to give the node</param>
        /// <returns></returns>
        public WireBuilder Transpose(string name = null)
        {
            _SetNode(new TransposeSignal(name));
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
