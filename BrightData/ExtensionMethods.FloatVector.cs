using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Hardware dependent size of a numeric vector of floats
        /// </summary>
        public static readonly int NumericsVectorSize = Vector<float>.Count;

        /// <summary>
        /// Vectorized cosine distance (0 for perpendicular, 1 for orthogonal, 2 for opposite)
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>Cosine distance between the two vectors</returns>
        /// <exception cref="ArgumentException"></exception>
        public static float CosineDistance(this float[] v1, float[] v2) => new ReadOnlySpan<float>(v1, 0, v1.Length).CosineDistance(v2);

        /// <summary>
        /// Find the minimum value and index in a vector
        /// </summary>
        /// <param name="vector">Vector to analyze</param>
        /// <returns>Tuple containing the minimum value and its index</returns>
        public static (float Value, uint Index) Minimum(this float[] vector) => new ReadOnlySpan<float>(vector, 0, vector.Length).Minimum();

        /// <summary>
        /// Returns the index of the minimum value within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static uint MinimumIndex(this float[] vector) => Minimum(vector).Index;

        /// <summary>
        /// Returns the minimum value
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static float MinimumValue(this float[] vector) => Minimum(vector).Value;

        /// <summary>
        /// Returns the maximum value and index within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns>Tuple containing the maximum value and its index</returns>
        public static (float Value, uint Index) Maximum(this float[] vector) => new ReadOnlySpan<float>(vector, 0, vector.Length).Maximum();

        /// <summary>
        /// Returns the maximum value within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static uint MaximumIndex(this float[] vector) => Maximum(vector).Index;

        /// <summary>
        /// Returns the index of the maximum value within a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns></returns>
        public static float MaximumValue(this float[] vector) => Maximum(vector).Value;

        /// <summary>
        /// Calculates the softmax of a vector
        /// https://en.wikipedia.org/wiki/Softmax_function
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static MemoryOwner<float> Softmax(this float[] vector) => new ReadOnlySpan<float>(vector, 0, vector.Length).Softmax();

        /// <summary>
        /// Finds the average of each value from a collection of vectors
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static float[]? GetAverage(this IEnumerable<float[]> vectors)
        {
            int count = 0, length = 0;
            float[]? ret = null;

            foreach(var vector in vectors) {
                ret ??= new float[length = vector.Length];
                for (var i = 0; i < length; i++)
                    ret[i] += vector[i];
                ++count;
            }

            if (ret != null) {
                for (var i = 0; i < length; i++)
                    ret[i] /= count;
            }
            return ret;
        }
    }
}
