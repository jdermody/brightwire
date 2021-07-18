using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.Serialization;

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
        public static T Create<T>(Type type, params object?[]? args)
        {
            var ret = Activator.CreateInstance(type, args);
            return ret != null
                ? (T)ret
                : throw new Exception($"Could not create object of type: {type}")
            ;
        }

        /// <summary>
        /// Creates a new object
        /// </summary>
        /// <typeparam name="T1">Type to cast created object to</typeparam>
        /// <typeparam name="T2">Type to cast created object to</typeparam>
        /// <param name="type">Type of object to create</param>
        /// <param name="args">Arguments to pass to constructor</param>
        public static (T1, T2) Create<T1, T2>(Type type, params object?[]? args)
        {
            var ret = Activator.CreateInstance(type, args);
            return ret != null
                ? ((T1)ret, (T2)ret)
                : throw new Exception($"Could not create object of type: {type}")
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
        /// Creates a new unitialized object (constructor is not invoked)
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <returns></returns>
        public static T CreateUninitialized<T>()
        {
            return CreateUninitialized<T>(typeof(T));
        }

        /// <summary>
        /// Creates a new unitialized object (constructor is not invoked)
        /// </summary>
        /// <param name="type">Type to create</param>
        /// <typeparam name="T">Type to return (created object cast to this type)</typeparam>
        /// <returns></returns>
        public static T CreateUninitialized<T>(Type type)
        {
            return (T)FormatterServices.GetUninitializedObject(type);
        }

        static Func<T> CompileCreator<T>(Type type) => Expression.Lambda<Func<T>>(Expression.New(type)).Compile();
    }
}
