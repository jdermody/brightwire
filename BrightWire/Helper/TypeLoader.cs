﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BrightWire.Helper
{
	/// <summary>
	/// Loads types in an assembly agnostic fashion
	/// </summary>
    internal static class TypeLoader
    {
		[return:DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
	    public static Type LoadType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]string typeName)
	    {
		    var type = Type.GetType(typeName, false) ?? Type.GetType(
                typeName, 
                _ => Assembly.GetExecutingAssembly(), 
                (assembly, typeName2, caseInsensitive) => assembly?.GetType(typeName2, false, caseInsensitive),
                true
            );
			return type ?? throw new ArgumentException($"Unable to find: {typeName}");
	    }

		[return:DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public static string GetTypeName<T>(T obj) where T: notnull
		{
			var type = obj.GetType();
            return type.AssemblyQualifiedName ?? throw new Exception("Null AssemblyQualifiedName");
		}
    }
}
