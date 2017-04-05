using BrightWire.Descriptor.GradientDescent;
using BrightWire.Descriptor.WeightInitialisation;
using BrightWire.ExecutionGraph.Activation;
using BrightWire.ExecutionGraph.Execution;
using BrightWire.ExecutionGraph.GradientDescent;
using BrightWire.ExecutionGraph.Input;
using BrightWire.ExecutionGraph.Layer;
using BrightWire.ExecutionGraph.WeightInitialisation;
using BrightWire.Helper;
using System;
using System.Collections.Generic;
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

        public ILayer CreateFeedForward(int inputSize, int outputSize, IPropertySet propertySet)
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

        public IGraphInput CreateInput(IDataTable dataTable, IDataTableVectoriser vectoriser = null)
        {
            var dataSource = new DataTableAdaptor(dataTable, vectoriser);
            var miniBatchProvider = new MiniBatchProvider(dataSource, _lap);
            return new MiniBatchGraphInput(miniBatchProvider);
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

        public ITrainingEngine CreateTrainingEngine(float learningRate, int batchSize, params IGraphInput[] input)
        {
            var context = new Context(_lap, learningRate, batchSize, true, false);
            return new TrainingEngine(context, input);
        }

        public IExecutionEngine CreateEngine(params IGraphInput[] input)
        {
            return new Engine(input);
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
            public ILayer LeakyRelu { get; } = new LeakyRelu();
            public ILayer Relu { get; } = new Relu();
            public ILayer Sigmoid { get; } = new Sigmoid();
            public ILayer Tanh { get; } = new Tanh();
        }
        public ActivationFunctionProvider Activation { get; } = new ActivationFunctionProvider();
    }
}
