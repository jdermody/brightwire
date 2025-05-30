﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace BrightData.Helper
{
    /// <summary>
    /// Generic object creator
    /// </summary>
    public static class GenericActivator
    {
        /// <summary>
        /// Creates a new object
        /// </summary>
        /// <typeparam name="T">Type to cast created object to</typeparam>
        /// <param name="type">Type of object to create</param>
        /// <param name="args">Arguments to pass to constructor</param>
        public static T Create<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, params object?[]? args)
        {
            var ret = Activator.CreateInstance(type, args);
            return ret != null
                ? (T)ret
                : throw new ArgumentException($"Could not create object of type: {type}", nameof(type))
            ;
        }

        static readonly ConcurrentDictionary<Type, Delegate> TypeCreators = new();

        /// <summary>
        /// Creates a new object (via default constructor)
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <returns></returns>
        public static T Create<T>()
        {
            var creator = (Func<T>)TypeCreators.GetOrAdd(typeof(T), CompileCreator<T>);
            return creator();
        }

        /// <summary>
        /// Creates a new uninitialized object (constructor is not invoked)
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <returns></returns>
        public static T CreateUninitialized<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>()
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }

        /// <summary>
        /// Creates a new uninitialized object (constructor is not invoked)
        /// </summary>
        /// <param name="type">Type to create</param>
        /// <typeparam name="T">Type to return (created object cast to this type)</typeparam>
        /// <returns></returns>
        public static T CreateUninitialized<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type)
        {
            return (T)RuntimeHelpers.GetUninitializedObject(type);
        }

        static Func<T> CompileCreator<T>(Type type) => Expression.Lambda<Func<T>>(Expression.New(type)).Compile();
    }
}
