using System.ComponentModel;

namespace BrightData.Converter
{
	/// <summary>
	/// Converts via a type converter
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="fallback"></param>
    internal class GenericConverter<T>(T? fallback = default)
        where T : struct
    {
	    readonly TypeConverter _converter = TypeDescriptor.GetConverter(typeof(T));

        public (object? ConvertedValue, bool WasSuccessful) ConvertValue(object value)
	    {
		    try
		    {
			    return (_converter.ConvertFrom(value), true);
		    }
		    catch
		    {
			    return (fallback, false);
		    }
	    }
    }
}
