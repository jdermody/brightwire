using BrightWire.Descriptor.GradientDescent;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.GradientDescent;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.ExecutionGraph.Input;
using BrightWire.ExecutionGraph.Node.Gate;
using BrightWire.ExecutionGraph.Node.Helper;
using BrightWire.ExecutionGraph.Node.Layer;
using BrightWire.ExecutionGraph.WeightInitialisation;
using BrightWire.Helper;
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

        IGradientDescentOptimisation _GetGradientDescent(IMatrix weight)
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

            //if (ret == null) {
                // look for a descriptor
                //var descriptor = propertySet.WeightInitialisationDescriptor;
                //if (descriptor != null)
                //    ret = descriptor.Create(propertySet);
            //}

            return ret ?? WeightInitialisation.Gaussian;
        }

        public ILearningContext CreateLearningContext(float learningRate, int batchSize, bool calculateTrainingError = true, bool deferUpdates = false)
        {
            return new LearningContext(_lap, learningRate, batchSize, calculateTrainingError, deferUpdates);
        }

        public IGraphTrainingEngine CreateTrainingEngine(IDataSource dataSource, float trainingRate = 0.1f, int batchSize = 128, bool isStochastic = true)
        {
            var learningContext = new LearningContext(_lap, trainingRate, batchSize, true, dataSource.IsSequential);
            return new TrainingEngine(_lap, dataSource, isStochastic, learningContext);
        }

        public IGraphEngine CreateEngine(Models.ExecutionGraph graph)
        {
            return new ExecutionEngine(_lap, graph);
        }

        public IDataSource GetDataSource(IDataTable dataTable, IDataTableVectoriser vectoriser = null)
        {
            var columns = dataTable.Columns;
            if (columns.Count == 2 && columns[0].Type == ColumnType.Matrix && columns[1].Type == ColumnType.Matrix)
                return new SequentialDataTableAdaptor(dataTable);
            
            // default adapator
            return new DataTableAdaptor(dataTable, vectoriser);
        }

        public INode CreateFeedForward(int inputSize, int outputSize, string name = null)
        {
            // create weights and bias
            var weightInit = _GetWeightInitialisation();
            var bias = weightInit.CreateBias(outputSize);
            var weight = weightInit.CreateWeight(inputSize, outputSize);

            // get the gradient descent optimisations
            var optimisation = _GetGradientDescent( weight);

            // create the layer
            return new FeedForward(bias, weight, optimisation, name);
        }

        public INode CreateSimpleRecurrent(int inputSize, float[] memory, INode activation, string name = null)
        {
            return new SimpleRecurrent(this, inputSize, memory, activation, name);
        }

        public INode CreateOneMinusInput()
        {
            return new OneMinusInput();
        }

        public INode CreateGru(int inputSize, float[] memory, string name = null)
        {
            return new GatedRecurrentUnit(this, inputSize, memory, name);
        }

        public INode CreateLstm(int inputSize, float[] memory, string name = null)
        {
            return new LongShortTermMemory(this, inputSize, memory, name);
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
            //var add = new AddGate();
            //var wireToPrimary = new WireToNode(add);
            //var wireToSecondary = new WireToNode(add, false);

            //input1.Build().Output.Add(wireToPrimary);
            //input2.Build().Output.Add(wireToSecondary);

            //return new WireBuilder(this, input1.CurrentSize, add);
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
            //var multiply = new MultiplyGate();
            //var wireToPrimary = new WireToNode(multiply);
            //var wireToSecondary = new WireToNode(multiply, false);

            //input1.Build().Output.Add(wireToPrimary);
            //input2.Build().Output.Add(wireToSecondary);

            //return new WireBuilder(this, input1.CurrentSize, multiply);
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
        public IWeightInitialisation GaussiantWeightInitialisation(float stdDev = 0.1f) => new Gaussian(_lap, stdDev);
        public IWeightInitialisation IdentityWeightInitialisation(float identityValue = 1f) => new Identity(_lap, identityValue);
        public IWeightInitialisation XavierWeightInitialisation(float parameter = 6) => new Xavier(_lap, parameter);

        public ICreateTemplateBasedGradientDescent AdaGrad() => new AdaGradDescriptor();
        public ICreateTemplateBasedGradientDescent Adam(float decay = 0.9f, float decay2 = 0.99f) => new AdamDescriptor(decay, decay2);
        public ICreateGradientDescent L1(float lambda) => new L1RegularisationDescriptor(lambda);
        public ICreateGradientDescent L2(float lambda) => new L2RegularisationDescriptor(lambda);
        public ICreateTemplateBasedGradientDescent Momentum(float momentum = 0.9f) => new MomentumDescriptor(momentum);
        public ICreateTemplateBasedGradientDescent NesterovMomentum(float momentum = 0.9f) => new NesterovMomentumDescriptor(momentum);
        public ICreateTemplateBasedGradientDescent RmsProp(float decay = 0.9f) => new RmsPropDescriptor(decay);

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
