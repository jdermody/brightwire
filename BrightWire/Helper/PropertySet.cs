using System.Collections.Generic;
using BrightData;

namespace BrightWire.Helper
{
    /// <summary>
    /// Property set implementation
    /// </summary>
    class PropertySet : IPropertySet
    {
        public const string WEIGHT_INITIALISATION = "bw:weight-initialisation";
        public const string WEIGHT_INITIALISATION_DESCRIPTOR = "bw:weight-initialisation-descriptor";
        public const string GRADIENT_DESCENT = "bw:gradient-descent";
        public const string TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR = "bw:template-gradient-descent-descriptor";
        public const string GRADIENT_DESCENT_DESCRIPTOR = "bw:gradient-descent-descriptor";

        readonly Dictionary<string, object> _data = new Dictionary<string, object>();

		public PropertySet(ILinearAlgebraProvider lap)
        {
            LinearAlgebraProvider = lap;
        }

        public IPropertySet Clone()
        {
            var ret = new PropertySet(LinearAlgebraProvider);
            foreach (var item in _data)
                ret._data.Add(item.Key, item.Value);
            return ret;
        }

		public ILinearAlgebraProvider LinearAlgebraProvider { get; }
		public IWeightInitialisation WeightInitialisation
        {
            get => Get<IWeightInitialisation>(WEIGHT_INITIALISATION);
			set => Set(WEIGHT_INITIALISATION, value);
		}
        public IGradientDescentOptimisation GradientDescent
        {
            get => Get<IGradientDescentOptimisation>(GRADIENT_DESCENT);
	        set => Set(GRADIENT_DESCENT, value);
        }
        public ICreateTemplateBasedGradientDescent TemplateGradientDescentDescriptor
        {
            get => Get<ICreateTemplateBasedGradientDescent>(TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR);
	        set => Set(TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR, value);
        }
        public ICreateGradientDescent GradientDescentDescriptor
        {
            get => Get<ICreateGradientDescent>(GRADIENT_DESCENT_DESCRIPTOR);
	        set => Set(GRADIENT_DESCENT_DESCRIPTOR, value);
        }

        public IPropertySet Use(ICreateTemplateBasedGradientDescent descriptor) { TemplateGradientDescentDescriptor = descriptor; return this; }
        public IPropertySet Use(ICreateGradientDescent descriptor) { GradientDescentDescriptor = descriptor; return this; }
        public IPropertySet Use(IGradientDescentOptimisation optimisation) { GradientDescent = optimisation; return this; }
        public IPropertySet Use(IWeightInitialisation weightInit) { WeightInitialisation = weightInit; return this; }

        public T Get<T>(string name, T defaultValue = default(T))
        {
	        if(_data.TryGetValue(name, out var obj))
                return (T)obj;
            return defaultValue;
        }

        public IPropertySet Set<T>(string name, T obj)
        {
            _data[name] = obj;
            return this;
        }

        public void Clear(string name)
        {
            _data.Remove(name);
        }
    }
}
