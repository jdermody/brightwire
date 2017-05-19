using BrightWire.Descriptor.GradientDescent;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.DataSource;
using BrightWire.ExecutionGraph.DataTableAdaptor;
using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.GradientDescent;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Node.Filter;
using BrightWire.ExecutionGraph.Node.Gate;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Input;
using BrightWire.ExecutionGraph.Node.Layer;
using BrightWire.ExecutionGraph.WeightInitialisation;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    public class GraphFactory
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IGradientDescentOptimisation _simpleGradientDescent = new Simple();
        readonly ICreateTemplateBasedGradientDescent _rmsProp = new RmsPropDescriptor(0.9f);
        readonly List<(TypeInfo, Type, string)> _queryTypes = new List<(TypeInfo, Type, string)>();
        readonly Stack<IPropertySet> _propertySetStack = new Stack<IPropertySet>();
        IPropertySet _defaultPropertySet;

        public GraphFactory(ILinearAlgebraProvider lap, IPropertySet propertySet = null)
        {
            _lap = lap;
            WeightInitialisation = new WeightInitialisationProvider(_lap);
            _defaultPropertySet = propertySet ?? new PropertySet(_lap) {
                WeightInitialisation = WeightInitialisation.Gaussian,
                TemplateGradientDescentDescriptor = _rmsProp
            };

            // add the gradient descent descriptors
            _Add(typeof(L1RegularisationDescriptor), PropertySet.GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(L2RegularisationDescriptor), PropertySet.GRADIENT_DESCENT_DESCRIPTOR);

            // add the template based gradient descent descriptors
            _Add(typeof(AdaGradDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(AdamDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(MomentumDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(NesterovMomentumDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(RmsPropDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
        }

        public ILinearAlgebraProvider LinearAlgebraProvider => _lap;

        public IExecutionContext CreateExecutionContext()
        {
            return new ExecutionContext(_lap);
        }

        public IPropertySet CurrentPropertySet
        {
            get { return _propertySetStack.Any() ? _propertySetStack.Peek() : _defaultPropertySet; }
        }

        public void PushPropertySet(Action<IPropertySet> callback)
        {
            var newPropertySet = CurrentPropertySet.Clone();
            callback?.Invoke(newPropertySet);
            _propertySetStack.Push(newPropertySet);
        }

        public void PopPropertyStack()
        {
            if (_propertySetStack.Any())
                _propertySetStack.Pop();
        }

        void _Add(Type type, string name)
        {
            _queryTypes.Add((type.GetTypeInfo(), type, name));
        }

        public IGradientDescentOptimisation GetWeightUpdater(IMatrix weight)
        {
            var propertySet = CurrentPropertySet;

            // look for the interface directly
            var ret = propertySet.GradientDescent;

            // look for a descriptor
            var createGradientDescent = propertySet.GradientDescentDescriptor;
            if (createGradientDescent != null)
                ret = createGradientDescent.Create(propertySet);

            // look for a template based descriptor
            var createTemplateGradientDescent = propertySet.TemplateGradientDescentDescriptor;
            if (createTemplateGradientDescent != null)
                ret = createTemplateGradientDescent.Create(ret ?? _simpleGradientDescent, weight, propertySet);

            return ret ?? _simpleGradientDescent;
        }

        IWeightInitialisation _GetWeightInitialisation()
        {
            var propertySet = CurrentPropertySet;
            var ret = propertySet.WeightInitialisation;
            return ret ?? WeightInitialisation.Gaussian;
        }

        public ILearningContext CreateLearningContext(float learningRate, int batchSize, bool calculateTrainingError = true, bool deferUpdates = false)
        {
            return new LearningContext(_lap, learningRate, batchSize, calculateTrainingError, deferUpdates);
        }

        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, IExecutionContext executionContext, float trainingRate = 0.1f, int batchSize = 128)
        {
            var learningContext = new LearningContext(_lap, trainingRate, batchSize, true, dataSource.IsSequential);
            return new TrainingEngine(_lap, dataSource, learningContext, executionContext, null);
        }

        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, IExecutionContext executionContext, Models.ExecutionGraph graph, float trainingRate = 0.1f, int batchSize = 128)
        {
            var learningContext = new LearningContext(_lap, trainingRate, batchSize, true, dataSource.IsSequential);
            var input = this.CreateFrom(graph);
            return new TrainingEngine(_lap, dataSource, learningContext, executionContext, input);
        }

        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, IExecutionContext executionContext, ILearningContext learningContext)
        {
            return new TrainingEngine(_lap, dataSource, learningContext, executionContext, null);
        }

        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, IExecutionContext executionContext, ILearningContext learningContext, Models.ExecutionGraph graph)
        {
            var input = this.CreateFrom(graph);
            return new TrainingEngine(_lap, dataSource, learningContext, executionContext, input);
        }

        public IGraphEngine CreateEngine(Models.ExecutionGraph graph, IExecutionContext executionContext)
        {
            var input = this.CreateFrom(graph);
            return new ExecutionEngine(_lap, executionContext, graph, input);
        }

        public IDataSource GetDataSource(IReadOnlyList<FloatVector> vectorList)
        {
            return new VectorDataSource(_lap, vectorList);
        }

        public IDataSource GetDataSource(IReadOnlyList<FloatMatrix> sequenceList)
        {
            return new SequentialDataSource(_lap, sequenceList);
        }

        public IDataSource GetDataSource(IDataTable dataTable, IDataTableVectoriser vectoriser = null)
        {
            var columns = dataTable.Columns;
            if (columns.Count == 2) {
                var column1 = columns[0].Type;
                var column2 = columns[1].Type;

                // many to many
                if (column1 == ColumnType.Matrix && column2 == ColumnType.Matrix)
                    return new SequentialDataTableAdaptor(_lap, dataTable);

                // one to one
                else if (column1 == ColumnType.Vector && column2 == ColumnType.Vector)
                    return new VectorBasedDataTableAdaptor(_lap, dataTable);

                // one to many
                else if (column1 == ColumnType.Vector && column2 == ColumnType.Matrix)
                    return new OneToManyDataTableAdaptor(_lap, dataTable);

                // many to one
                else if (column1 == ColumnType.Matrix && column2 == ColumnType.Vector)
                    return new ManyToOneDataTableAdaptor(_lap, dataTable);
            }
            
            // default adapator
            return new DefaultDataTableAdaptor(_lap, dataTable, vectoriser);
        }

        public IDataSource GetDataSource(IDataTable dataTable, ILearningContext learningContext, IExecutionContext executionContext, Action<WireBuilder> dataConversionBuilder)
        {
            var columns = dataTable.Columns;
            if (columns.Count == 2) {
                var column1 = columns[0].Type;
                var column2 = columns[1].Type;

                // volume classification
                if (column1 == ColumnType.Tensor && column2 == ColumnType.Vector)
                    return new TensorBasedDataTableAdaptor(learningContext, executionContext, this, dataTable, dataConversionBuilder);

                // sequence to sequence
                else if (column1 == ColumnType.Matrix && column2 == ColumnType.Matrix)
                    return new SequenceToSequenceDataTableAdaptor(learningContext, executionContext, this, dataTable, dataConversionBuilder);
            }
            throw new ArgumentException($"{nameof(dataTable)} does not contain a recognised data format");
        }

        public IDataSource GetDataSource(IDataTable dataTable, IExecutionContext executionContext, Models.DataSourceModel dataSource, ILearningContext learningContext = null)
        {
            var input = this.CreateFrom(dataSource.Graph);

            var columns = dataTable.Columns;
            if (columns.Count == 2) {
                var column1 = columns[0].Type;
                var column2 = columns[1].Type;

                //// volume classification
                //if (column1 == ColumnType.Tensor && column2 == ColumnType.Vector)
                //    return new TensorBasedDataTableAdaptor(learningContext, executionContext, this, dataTable, dataConversionBuilder);

                //// sequence to sequence
                //else
                if (column1 == ColumnType.Matrix && column2 == ColumnType.Matrix)
                    return new SequenceToSequenceDataTableAdaptor(learningContext, executionContext, dataTable, input, dataSource);
            }
            throw new ArgumentException($"{nameof(dataTable)} does not contain a recognised data format");
        }

        public INode CreateFeedForward(int inputSize, int outputSize, string name = null)
        {
            // create weights and bias
            var weightInit = _GetWeightInitialisation();
            var bias = weightInit.CreateBias(outputSize);
            var weight = weightInit.CreateWeight(inputSize, outputSize);

            // get the gradient descent optimisations
            var optimisation = GetWeightUpdater( weight);

            // create the layer
            return new FeedForward(inputSize, outputSize, bias, weight, optimisation, name);
        }

        public INode CreateDropConnect(float dropoutPercentage, int inputSize, int outputSize, string name = null)
        {
            // create weights and bias
            var weightInit = _GetWeightInitialisation();
            var bias = weightInit.CreateBias(outputSize);
            var weight = weightInit.CreateWeight(inputSize, outputSize);

            // get the gradient descent optimisations
            var optimisation = GetWeightUpdater(weight);

            return new DropConnect(dropoutPercentage, inputSize, outputSize, bias, weight, optimisation, name);
        }

        public INode CreateTiedFeedForward(IFeedForward layer, string name = null)
        {
            var weightInit = _GetWeightInitialisation();
            return new TiedFeedForward(layer, weightInit, name);
        }

        public INode CreateConvolutional(int inputDepth, int filterCount, int padding, int filterWidth, int filterHeight, int stride, string name = null)
        {
            var weightInit = _GetWeightInitialisation();
            return new Convolutional(weightInit, weight => GetWeightUpdater(weight), inputDepth, filterCount, padding, filterWidth, filterHeight, stride, name);
        }

        public INode CreateSimpleRecurrent(int inputSize, float[] memory, INode activation, string name = null)
        {
            return new SimpleRecurrent(this, inputSize, memory, activation, name);
        }

        public INode CreateElman(int inputSize, float[] memory, INode activation, INode activation2, string name = null)
        {
            return new ElmanJordan(this, true, inputSize, memory, activation, activation2, name);
        }

        public INode CreateJordan(int inputSize, float[] memory, INode activation, INode activation2, string name = null)
        {
            return new ElmanJordan(this, false, inputSize, memory, activation, activation2, name);
        }

        public INode CreateOneMinusInput(string name = null)
        {
            return new OneMinusInput(name);
        }

        public INode CreateSequenceReverser(string name = null)
        {
            return new ReverseSequence(name);
        }

        public INode CreateGru(int inputSize, float[] memory, string name = null)
        {
            return new GatedRecurrentUnit(this, inputSize, memory, name);
        }

        public INode CreateLstm(int inputSize, float[] memory, string name = null)
        {
            return new LongShortTermMemory(this, inputSize, memory, name);
        }

        public INode CreateMaxPool(int width, int height, int stride, string name = null)
        {
            return new MaxPool(width, height, stride, name);
        }

        public INode CreateDropOut(float dropoutPercentage, string name = null)
        {
            return new DropOut(dropoutPercentage, name);
        }

        public WireBuilder Connect(IGraphEngine engine)
        {
            return new WireBuilder(this, engine);
        }

        public WireBuilder Build(int inputSize, INode node)
        {
            return new WireBuilder(this, inputSize, node);
        }

        public WireBuilder Add(WireBuilder input1, WireBuilder input2)
        {
            Debug.Assert(input1.CurrentSize == input2.CurrentSize);
            return Add(input1.CurrentSize, input1.Build(), input2.Build());
        }

        public WireBuilder Add(int inputSize, INode input1, INode input2)
        {
            var add = new AddGate();
            var wireToPrimary = new WireToNode(add);
            var wireToSecondary = new WireToNode(add, 1);

            input1.Output.Add(wireToPrimary);
            input2.Output.Add(wireToSecondary);

            return new WireBuilder(this, inputSize, add);
        }

        public WireBuilder Multiply(WireBuilder input1, WireBuilder input2)
        {
            Debug.Assert(input1.CurrentSize == input2.CurrentSize);
            return Multiply(input1.CurrentSize, input1.Build(), input2.Build());
        }

        public WireBuilder Multiply(int inputSize, INode input1, INode input2)
        {
            var multiply = new MultiplyGate();
            var wireToPrimary = new WireToNode(multiply);
            var wireToSecondary = new WireToNode(multiply, 1);

            input1.Output.Add(wireToPrimary);
            input2.Output.Add(wireToSecondary);

            return new WireBuilder(this, inputSize, multiply);
        }

        public WireBuilder Join(WireBuilder input1, WireBuilder input2, string name = null)
        {
            var ret = new JoinGate(name, input1, input2);
            var wireToPrimary = new WireToNode(ret);
            var wireToSecondary = new WireToNode(ret, 1);

            input1.LastNode.Output.Add(wireToPrimary);
            input2.LastNode.Output.Add(wireToSecondary);

            return new WireBuilder(this, input1.CurrentSize + input2.CurrentSize, ret);
        }

        public INode Create(Models.ExecutionGraph.Node node)
        {
            var type = Type.GetType(node.TypeName);
            var ret = (INode)FormatterServices.GetUninitializedObject(type);
            ret.Initialise(this, node.Id, node.Name, node.Description, node.Data);
            return ret;
        }

        public INode LeakyReluActivation(string name = null) => new LeakyRelu(name);
        public INode ReluActivation(string name = null) => new Relu(name);
        public INode SigmoidActivation(string name = null) => new Sigmoid(name);
        public INode TanhActivation(string name = null) => new Tanh(name);
        public INode SoftMaxActivation(string name = null) => new SoftMax(name);

        public IWeightInitialisation ConstantWeightInitialisation(float biasValue = 0f, float weightValue = 1f) => new Constant(_lap, biasValue, weightValue);
        public IWeightInitialisation GaussianWeightInitialisation(bool zeroBias = true, float stdDev = 0.1f) => new Gaussian(_lap, zeroBias, stdDev);
        public IWeightInitialisation IdentityWeightInitialisation(float identityValue = 1f) => new Identity(_lap, identityValue);
        public IWeightInitialisation XavierWeightInitialisation(float parameter = 6) => new Xavier(_lap, parameter);

        public ICreateTemplateBasedGradientDescent AdaGrad() => new AdaGradDescriptor();
        public ICreateTemplateBasedGradientDescent Adam(float decay = 0.9f, float decay2 = 0.99f) => new AdamDescriptor(decay, decay2);
        public ICreateGradientDescent L1(float lambda) => new L1RegularisationDescriptor(lambda);
        public ICreateGradientDescent L2(float lambda) => new L2RegularisationDescriptor(lambda);
        public ICreateTemplateBasedGradientDescent Momentum(float momentum = 0.9f) => new MomentumDescriptor(momentum);
        public ICreateTemplateBasedGradientDescent NesterovMomentum(float momentum = 0.9f) => new NesterovMomentumDescriptor(momentum);
        public ICreateTemplateBasedGradientDescent RmsProp(float decay = 0.9f) => new RmsPropDescriptor(decay);

        public class GradientDescentProvider
        {
            public ICreateTemplateBasedGradientDescent AdaGrad { get; } = new AdaGradDescriptor();
            public ICreateTemplateBasedGradientDescent Adam { get; } = new AdamDescriptor(0.9f, 0.99f);
            public ICreateGradientDescent L1 { get; } = new L1RegularisationDescriptor(0.1f);
            public ICreateGradientDescent L2 { get; } = new L1RegularisationDescriptor(0.1f);
            public ICreateTemplateBasedGradientDescent Momentum { get; } = new MomentumDescriptor(0.9f);
            public ICreateTemplateBasedGradientDescent NesterovMomentum { get; } = new NesterovMomentumDescriptor(0.9f);
            public ICreateTemplateBasedGradientDescent RmsProp { get; } = new RmsPropDescriptor(0.9f);
        }
        public GradientDescentProvider GradientDescent { get; } = new GradientDescentProvider();

        public class ErrorMetricProvider
        {
            public IErrorMetric BinaryClassification { get; } = new ErrorMetric.BinaryClassification();
            public IErrorMetric CrossEntropy { get; } = new ErrorMetric.CrossEntropy();
            public IErrorMetric OneHotEncoding { get; } = new ErrorMetric.OneHotEncoding();
            public IErrorMetric Quadratic { get; } = new ErrorMetric.Quadratic();
            public IErrorMetric Rmse { get; } = new ErrorMetric.Rmse();
        }
        public ErrorMetricProvider ErrorMetric { get; } = new ErrorMetricProvider();

        public class WeightInitialisationProvider
        {
            public IWeightInitialisation Ones { get; private set; }
            public IWeightInitialisation Zeroes { get; private set; }
            public IWeightInitialisation Gaussian { get; private set; }
            public IWeightInitialisation Xavier { get; private set; }
            public IWeightInitialisation Identity { get; private set; }
            public IWeightInitialisation Identity01 { get; private set; }

            public WeightInitialisationProvider(ILinearAlgebraProvider lap)
            {
                Ones = new Constant(lap, 0f, 1f);
                Zeroes = new Constant(lap, 0f, 0f);
                Gaussian = new Gaussian(lap);
                Xavier = new Xavier(lap);
                Identity = new Identity(lap, 1f);
                Identity01 = new Identity(lap, 0.1f);
            }
        }
        public WeightInitialisationProvider WeightInitialisation { get; private set; }
    }
}
