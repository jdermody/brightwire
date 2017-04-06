using BrightWire.Descriptor.GradientDescent;
using BrightWire.Descriptor.WeightInitialisation;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Execution;
using BrightWire.ExecutionGraph.GradientDescent;
using BrightWire.ExecutionGraph.Input;
using BrightWire.ExecutionGraph.Layer;
using BrightWire.ExecutionGraph.WeightInitialisation;
using BrightWire.ExecutionGraph.Wire;
using BrightWire.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BrightWire.ExecutionGraph
{
    public class GraphFactory
    {
        readonly ILinearAlgebraProvider _lap;
        readonly IGradientDescentOptimisation _simpleGradientDescent = new Simple();
        readonly IWeightInitialisation _gaussianWeightInitialisation;
        readonly ICreateTemplateBasedGradientDescent _rmsProp = new RmsPropDescriptor(0.9f);
        readonly List<(TypeInfo, Type, string)> _queryTypes = new List<(TypeInfo, Type, string)>();

        public GraphFactory(ILinearAlgebraProvider lap)
        {
            _lap = lap;
            _gaussianWeightInitialisation = new Gaussian(_lap);

            // add the gradient descent descriptors
            _Add(typeof(L1RegularisationDescriptor), PropertySet.GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(L2RegularisationDescriptor), PropertySet.GRADIENT_DESCENT_DESCRIPTOR);

            // add the template based gradient descent descriptors
            _Add(typeof(AdaGradDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(AdamDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(MomentumDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(NesterovMomentumDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
            _Add(typeof(RmsPropDescriptor), PropertySet.TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);

            // add the weight initialisation descriptors
            _Add(typeof(ConstantDescriptor), PropertySet.WEIGHT_INITIALISATION_DESCRIPTOR);
            _Add(typeof(GaussianDescriptor), PropertySet.WEIGHT_INITIALISATION_DESCRIPTOR);
            _Add(typeof(IdentityDescriptor), PropertySet.WEIGHT_INITIALISATION_DESCRIPTOR);
            _Add(typeof(XavierDescriptor), PropertySet.WEIGHT_INITIALISATION_DESCRIPTOR);
        }

        void _Add(Type type, string name)
        {
            _queryTypes.Add((type.GetTypeInfo(), type, name));
        }

        IGradientDescentOptimisation _GetGradientDescent(IPropertySet propertySet, IMatrix weight)
        {
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

        IWeightInitialisation _GetWeightInitialisation(IPropertySet propertySet)
        {
            var ret = propertySet.WeightInitialisation;

            // look for a descriptor
            var descriptor = propertySet.WeightInitialisationDescriptor;
            if (descriptor != null)
                ret = descriptor.Create(propertySet);

            return ret;
        }

        public IComponent CreateFeedForward(int inputSize, int outputSize, IPropertySet propertySet)
        {
            // create weights and bias
            var weightInit = _GetWeightInitialisation(propertySet);
            var bias = weightInit.CreateBias(outputSize);
            var weight = weightInit.CreateWeight(inputSize, outputSize);

            // get the gradient descent optimisations
            var optimisation = _GetGradientDescent(propertySet, weight);

            // create the layer
            return new FeedForward(bias, weight, optimisation);
        }

        public IMiniBatchProvider CreateMiniBatchProvider(IDataTable dataTable, IDataTableVectoriser vectoriser = null)
        {
            IDataSource dataSource;
            if (dataTable.Template == DataTableTemplate.Matrix)
                dataSource = new SequentialDataTableAdaptor(dataTable);
            else
                dataSource = new DataTableAdaptor(dataTable, vectoriser);

            return new MiniBatchProvider(dataSource, _lap);
        }

        public IGraphInput CreateGraphInput(IMiniBatchProvider provider)
        {
            var dataSource = provider.DataSource;
            return new MiniBatchGraphInput(dataSource, _lap);
        }

        public IPropertySet CreatePropertySet()
        {
            return new PropertySet(_lap) {
                WeightInitialisation = _gaussianWeightInitialisation,
                TemplateGradientDescentDescriptor = _rmsProp
            };
        }

        public WireBuilder Connect(IGraphInput input, IPropertySet propertySet)
        {
            return new WireBuilder(this, input, propertySet);
        }

        public WireBuilder Connect(IWire wire, IPropertySet propertySet)
        {
            return new WireBuilder(this, wire, propertySet);
        }

        public WireBuilder Build(int inputSize, IPropertySet propertySet)
        {
            return new WireBuilder(this, inputSize, propertySet);
        }

        public WireBuilder Add(int channel, IPropertySet propertySet, params IWire[] wires)
        {
            var addWire = new AddWires(wires.First().LastWire.OutputSize, channel, wires);
            foreach (var wire in wires)
                wire.LastWire.SetDestination(addWire);
            return Connect(addWire, propertySet);
        }

        public ITrainingEngine CreateTrainingEngine(float learningRate, int batchSize, IGraphInput input)
        {
            var context = new LearningContext(_lap, learningRate, batchSize, true, input.IsSequential);
            return new TrainingEngine(context, input);
        }

        public IExecutionEngine CreateEngine(IMiniBatchProvider provider, IGraphInput input)
        {
            return new Engine(provider, input);
        }

        public class ErrorMetricProvider
        {
            public IErrorMetric BinaryClassification { get; } = new ErrorMetric.BinaryClassification();
            public IErrorMetric CrossEntropy { get; } = new ErrorMetric.CrossEntropy();
            public IErrorMetric OneHotEncoding { get; } = new ErrorMetric.OneHotEncoding();
            public IErrorMetric Quadratic { get; } = new ErrorMetric.Quadratic();
            public IErrorMetric Rmse { get; } = new ErrorMetric.Rmse();
        }
        public ErrorMetricProvider ErrorMetric { get; } = new ErrorMetricProvider();

        public class ActivationFunctionProvider
        {
            public IComponent LeakyRelu { get; } = new LeakyRelu();
            public IComponent Relu { get; } = new Relu();
            public IComponent Sigmoid { get; } = new Sigmoid();
            public IComponent Tanh { get; } = new Tanh();
            public IComponent SoftMax { get; } = new SoftMax();
        }
        public ActivationFunctionProvider Activation { get; } = new ActivationFunctionProvider();
    }
}
