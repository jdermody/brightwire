using System;
using System.Reflection;

namespace BrightWire.Helper
{
	/// <summary>
	/// Loads types in an assembly agnostic fashion
	/// </summary>
    internal static class TypeLoader
    {
	    public static Type LoadType(string typeName)
	    {
		    var type = Type.GetType(typeName, false);
		    if (type == null) {
			    // check if the type exists in the current assembly
			    type = Type.GetType(
				    typeName, 
				    assemblyName => Assembly.GetExecutingAssembly(), 
				    (assembly, typeName2, caseInsensitive) => assembly.GetType(typeName2, false, caseInsensitive),
				    true
			    );
		    }
			return type;
	    }
    }
}
