using BrightWire.Bayesian;
using BrightWire.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    /// <summary>
    /// Static entry point
    /// </summary>
    public static partial class Provider
    {
        /// <summary>
        /// Creates a linear algebra provider that runs on the CPU
        /// </summary>
        /// <param name="stochastic">False to disable random number generation</param>
        public static ILinearAlgebraProvider CreateCPULinearAlgebra(bool stochastic = true)
        {
            return new NumericsProvider(stochastic);
        }

        /// <summary>
        /// Create a markov model trainer of window size 2
        /// </summary>
        /// <typeparam name="T">The markov chain data type</typeparam>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer2<T> CreateMarkovTrainer2<T>(int minObservations = 1)
        {
            return new MarkovModelTrainer2<T>(minObservations);
        }

        /// <summary>
        /// Create a markov model trainer of window size 3
        /// </summary>
        /// <typeparam name="T">The markov chain data type</typeparam>
        /// <param name="minObservations">Minimum number of data points to record an observation</param>
        public static IMarkovModelTrainer3<T> CreateMarkovTrainer3<T>(int minObservations = 1)
        {
            return new MarkovModelTrainer3<T>(minObservations);
        }
    }
}
