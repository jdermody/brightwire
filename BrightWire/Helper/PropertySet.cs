using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Helper
{
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

        public ILinearAlgebraProvider LinearAlgebraProvider { get { return _lap; } }
        public IWeightInitialisation WeightInitialisation
        {
            get { return Get<IWeightInitialisation>(WEIGHT_INITIALISATION); }
            set { Set<IWeightInitialisation>(WEIGHT_INITIALISATION, value); }
        }
        public ICreateWeightInitialisation WeightInitialisationDescriptor
        {
            get { return Get<ICreateWeightInitialisation>(WEIGHT_INITIALISATION_DESCRIPTOR); }
            set { Set<ICreateWeightInitialisation>(WEIGHT_INITIALISATION_DESCRIPTOR, value); }
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

        public void Use(ICreateTemplateBasedGradientDescent descriptor) { TemplateGradientDescentDescriptor = descriptor; }
        public void Use(ICreateGradientDescent descriptor) { GradientDescentDescriptor = descriptor; }
        public void Use(ICreateWeightInitialisation descriptor) { WeightInitialisationDescriptor = descriptor; }
        public void Use(IGradientDescentOptimisation optimisation) { GradientDescent = optimisation; }
        public void Use(IWeightInitialisation weightInit) { WeightInitialisation = weightInit; }

        public T Get<T>(string name, T defaultValue = default(T))
        {
            object obj;
            if(_data.TryGetValue(name, out obj))
                return (T)obj;
            return defaultValue;
        }

        public void Set<T>(string name, T obj)
        {
            _data[name] = obj;
        }

        public void Set(string name, object obj)
        {
            _data[name] = obj;
        }

        public void Clear(string name)
        {
            _data.Remove(name);
        }
    }
}
