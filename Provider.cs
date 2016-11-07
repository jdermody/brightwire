using BrightWire.Bayesian;
using BrightWire.Connectionist;
using BrightWire.LinearAlgebra;
using BrightWire.TabularData;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="stochastic">False to use the same random number generation each time</param>
        public static ILinearAlgebraProvider CreateLinearAlgebra(bool stochastic = true)
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

        /// <summary>
        /// Creates a data table from a stream
        /// </summary>
        /// <param name="dataStream">The stream that the data table was written to</param>
        /// <param name="indexStream">The stream that the index was written to (optional)</param>
        public static IDataTable CreateDataTable(Stream dataStream, Stream indexStream = null)
        {
            if (indexStream == null)
                return DataTable.Create(dataStream);
            else
                return DataTable.Create(dataStream, indexStream);
        }

        /// <summary>
        /// Creates a neural network factory
        /// </summary>
        /// <param name="lap">Linear alegebra provider</param>
        /// <param name="stochastic">False to use the same random number generation each time</param>
        /// <returns></returns>
        public static INeuralNetworkFactory CreateNeuralNetworkFactory(ILinearAlgebraProvider lap, bool stochastic)
        {
            return new ConnectionistFactory(lap, stochastic);
        }
    }
}
