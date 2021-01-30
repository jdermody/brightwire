using System.ComponentModel;

namespace BrightData.Converter
{
    internal class GenericConverter<T> where T: struct
    {
	    readonly TypeConverter _converter;
	    readonly T? _fallback;

	    public GenericConverter(T? fallback = default)
	    {
		    _converter = TypeDescriptor.GetConverter(typeof(T));
		    _fallback = fallback;
	    }

	    public (object? ConvertedValue, bool WasSuccessful) ConvertValue(object value)
	    {
		    try
		    {
			    return (_converter.ConvertFrom(value), true);
		    }
		    catch
		    {
			    return (_fallback, false);
		    }
	    }
    }
}
