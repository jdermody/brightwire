using BrightWire.ExecutionGraph.Descriptor;
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
        readonly bool _isStochastic;
        readonly Dictionary<WeightInitialisationType, Lazy<IWeightInitialisation>> _weightInitialisation = new Dictionary<WeightInitialisationType, Lazy<IWeightInitialisation>>();
        readonly IGradientDescentOptimisation _simple = new Simple();
        readonly List<(TypeInfo, Type, string)> _queryTypes = new List<(TypeInfo, Type, string)>();

        const string WEIGHT_INITIALISATION = "weight-initialisation";
        const string TEMPLATE_GRADIENT_DESCENT = "template-gradient-descent";
        const string GRADIENT_DESCENT = "gradient-descent";

        public GraphFactory(ILinearAlgebraProvider lap, bool isStochastic)
        {
            _lap = lap;

            _Add(WeightInitialisationType.Gaussian, () => new Gaussian(_lap, _isStochastic));

            _Add(typeof(IWeightInitialisation), WEIGHT_INITIALISATION);
            _Add(typeof(MomentumDescriptor), GRADIENT_DESCENT);
        }

        void _Add(Type type, string name)
        {
            _queryTypes.Add((type.GetTypeInfo(), type, name));
        }

        void _Add(WeightInitialisationType type, Func<IWeightInitialisation> creator)
        {
            _weightInitialisation.Add(type, new Lazy<IWeightInitialisation>(creator));
        }

        public IWeightInitialisation Get(WeightInitialisationType type)
        {
            return _weightInitialisation[type].Value;
        }

        IWeightInitialisation _GetWeightInitialisation(IPropertySet propertySet)
        {
            return propertySet.Get<IWeightInitialisation>(WEIGHT_INITIALISATION) ?? Get(WeightInitialisationType.Gaussian);
        }

        public ILayer CreateFeedForward(int inputSize, int outputSize, IPropertySet propertySet)
        {
            // create weights and bias
            var weightInit = propertySet.Get<IWeightInitialisation>(WEIGHT_INITIALISATION);
            var bias = weightInit.CreateBias(outputSize);
            var weight = weightInit.CreateWeight(inputSize, outputSize);

            // get the gradient descent optimisations
            IGradientDescentOptimisation optimisation = propertySet.Get<IGradientDescentOptimisation>(GRADIENT_DESCENT);
            var templateGradientDescent = propertySet.Get<ITemplateBasedCreateGradientDescent>(TEMPLATE_GRADIENT_DESCENT);
            if (templateGradientDescent != null)
                optimisation = templateGradientDescent.Create(optimisation, weight, propertySet);

            // create the layer
            return new FeedForward(bias, weight, optimisation);
        }

        public IGraphInput CreateTrainingInput(IDataTable dataTable)
        {
            var dataSource = new DataTableAdaptor(dataTable);
            var miniBatchProvider = new MiniBatchProvider(dataSource, _lap, _isStochastic);
            return new MiniBatchFeeder(miniBatchProvider, true);
        }

        public IPropertySet GetPropertySet(params object[] input)
        {
            var ret = new PropertySet(_lap);
            ret.Set(WEIGHT_INITIALISATION, Get(WeightInitialisationType.Gaussian));
            ret.Set(GRADIENT_DESCENT, _simple);
            ret.Set(TEMPLATE_GRADIENT_DESCENT, new RmsPropDescriptor(0.9f));

            foreach (var item in input) {
                if(item != null) {
                    var typeInfo = item.GetType().GetTypeInfo();
                    foreach(var item2 in _queryTypes) {
                        if (typeInfo.IsAssignableFrom(item2.Item1))
                            ret.Set(item2.Item3, Convert.ChangeType(item, item2.Item2));
                    }
                }
            }

            return ret;
        }
    }
}
