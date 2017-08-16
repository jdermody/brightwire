using System.Collections.Generic;

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
        readonly ILinearAlgebraProvider _lap;

        public PropertySet(ILinearAlgebraProvider lap)
        {
            _lap = lap;
        }

        public IPropertySet Clone()
        {
            var ret = new PropertySet(_lap);
            foreach (var item in _data)
                ret._data.Add(item.Key, item.Value);
            return ret;
        }

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }
        public IWeightInitialisation WeightInitialisation
        {
            get { return Get<IWeightInitialisation>(WEIGHT_INITIALISATION); }
            set { Set<IWeightInitialisation>(WEIGHT_INITIALISATION, value); }
        }
        public IGradientDescentOptimisation GradientDescent
        {
            get { return Get<IGradientDescentOptimisation>(GRADIENT_DESCENT); }
            set { Set<IGradientDescentOptimisation>(GRADIENT_DESCENT, value); }
        }
        public ICreateTemplateBasedGradientDescent TemplateGradientDescentDescriptor
        {
            get { return Get<ICreateTemplateBasedGradientDescent>(TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR); }
            set { Set<ICreateTemplateBasedGradientDescent>(TEMPLATE_GRADIENT_DESCENT_DESCRIPTOR, value); }
        }
        public ICreateGradientDescent GradientDescentDescriptor
        {
            get { return Get<ICreateGradientDescent>(GRADIENT_DESCENT_DESCRIPTOR); }
            set { Set<ICreateGradientDescent>(GRADIENT_DESCENT_DESCRIPTOR, value); }
        }

        public IPropertySet Use(ICreateTemplateBasedGradientDescent descriptor) { TemplateGradientDescentDescriptor = descriptor; return this; }
        public IPropertySet Use(ICreateGradientDescent descriptor) { GradientDescentDescriptor = descriptor; return this; }
        public IPropertySet Use(IGradientDescentOptimisation optimisation) { GradientDescent = optimisation; return this; }
        public IPropertySet Use(IWeightInitialisation weightInit) { WeightInitialisation = weightInit; return this; }

        public T Get<T>(string name, T defaultValue = default(T))
        {
            object obj;
            if(_data.TryGetValue(name, out obj))
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
