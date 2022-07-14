using System.Collections.Generic;
using BrightData;
using BrightData.LinearAlgebra;

namespace BrightWire.Helper
{
    /// <summary>
    /// Property set implementation
    /// </summary>
    internal class PropertySet : IPropertySet
    {
        public const string WeightInitialisationLabel = "bw:weight-initialisation";
        public const string GradientDescentLabel = "bw:gradient-descent";
        public const string TemplateGradientDescentDescriptorLabel = "bw:template-gradient-descent-descriptor";
        public const string GradientDescentDescriptorLabel = "bw:gradient-descent-descriptor";

        readonly Dictionary<string, object> _data = new();

		public PropertySet(LinearAlgebraProvider lap)
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

        /// <inheritdoc />
		public LinearAlgebraProvider LinearAlgebraProvider { get; }
		public IWeightInitialisation? WeightInitialisation
        {
            get => Get<IWeightInitialisation>(WeightInitialisationLabel);
			set => Set(WeightInitialisationLabel, value);
		}
        public IGradientDescentOptimisation? GradientDescent
        {
            get => Get<IGradientDescentOptimisation>(GradientDescentLabel);
	        set => Set(GradientDescentLabel, value);
        }
        public ICreateTemplateBasedGradientDescent? TemplateGradientDescentDescriptor
        {
            get => Get<ICreateTemplateBasedGradientDescent>(TemplateGradientDescentDescriptorLabel);
	        set => Set(TemplateGradientDescentDescriptorLabel, value);
        }
        public ICreateGradientDescent? GradientDescentDescriptor
        {
            get => Get<ICreateGradientDescent>(GradientDescentDescriptorLabel);
	        set => Set(GradientDescentDescriptorLabel, value);
        }

        public IPropertySet Use(ICreateTemplateBasedGradientDescent descriptor) { TemplateGradientDescentDescriptor = descriptor; return this; }
        public IPropertySet Use(ICreateGradientDescent descriptor) { GradientDescentDescriptor = descriptor; return this; }
        public IPropertySet Use(IGradientDescentOptimisation optimisation) { GradientDescent = optimisation; return this; }
        public IPropertySet Use(IWeightInitialisation weightInit) { WeightInitialisation = weightInit; return this; }

        public T? Get<T>(string name) where T: class
        {
	        if(_data.TryGetValue(name, out var obj))
                return (T?)obj;
            return null;
        }

        public IPropertySet Set<T>(string name, T? obj) where T: class
        {
            if (obj == null)
                _data.Remove(name);
            else
                _data[name] = obj;
            return this;
        }

        public void Clear(string name)
        {
            _data.Remove(name);
        }
    }
}
