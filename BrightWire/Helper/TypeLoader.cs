﻿using System;
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
				    (assembly, typeName2, caseInsensitive) => assembly?.GetType(typeName2, false, caseInsensitive),
				    true
			    );
		    }
			if(type == null)
				throw new ArgumentException($"Unable to find: {typeName}");
			return type;
	    }

        public static string GetTypeName<T>(T obj) where T: notnull
		{
			var type = obj.GetType();
            return type.AssemblyQualifiedName ?? throw new Exception("Null AssemblyQualifiedName");
		}
    }
}
