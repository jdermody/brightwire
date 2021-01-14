using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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
    }
}
