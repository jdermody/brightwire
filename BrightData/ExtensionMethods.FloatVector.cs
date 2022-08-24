using BrightData.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Vectorised cosine distance
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>Cosine distance between the two vectors</returns>
        /// <exception cref="ArgumentException"></exception>
        public static float CosineDistance(this float[] v1, float[] v2)
        {
            var length = v1.Length;
            if (length != v2.Length)
                throw new ArgumentException($"Arrays were of different size: ({v1.Length} vs {v2.Length})");

            if (length >= Consts.MinimumSizeForVectorised) {
                var leftVec = MemoryMarshal.Cast<float, Vector<float>>(v1);
                var rightVec = MemoryMarshal.Cast<float, Vector<float>>(v2);
                var numVectors = length / SpanExtensions.NumericsVectorSize;
                var nextIndex = numVectors * SpanExtensions.NumericsVectorSize;
                Vector<float> ab = new(0f), aa = new(0f), bb = new(0f);
                for (var i = 0; i < numVectors; i++) {
                    ab += leftVec[i] * rightVec[i];
                    aa += leftVec[i] * leftVec[i];
                    bb += rightVec[i] * rightVec[i];
                }

                float ab2 = Vector.Dot(ab, Vector<float>.One), aa2 = Vector.Dot(aa, Vector<float>.One), bb2 = Vector.Dot(bb, Vector<float>.One);
                for (; nextIndex < length; nextIndex++) {
                    float a = v1[nextIndex], b = v2[nextIndex];
                    ab2 += a * b;
                    aa2 += a * a;
                    bb2 += b * b;
                }
                return 1f - ab2 / (MathF.Sqrt(aa2) * MathF.Sqrt(bb2));
            }
            else {
                float aa = 0, bb = 0, ab = 0;
                for (int i = 0, len = v1.Length; i < len; i++) {
                    var a = v1[i];
                    var b = v2[i];
                    ab += a * b;
                    aa += a * a;
                    bb += b * b;
                }
                return 1f - ab / (MathF.Sqrt(aa) * MathF.Sqrt(bb));
            }
        }

        /// <summary>
        /// Find the minimum value and index in a vector
        /// </summary>
        /// <param name="vector">Vector to analyse</param>
        /// <returns>Tuple containing the minimum value and its index</returns>
        public static (float Value, uint Index) Minimum(this float[] vector)
        {
            var ret = uint.MaxValue;
            var lowestValue = float.MaxValue;

            for (uint i = 0, len = (uint)vector.Length; i < len; i++) {
                var val = vector[i];
                if (val < lowestValue) {
                    lowestValue = val;
                    ret = i;
                }
            }

            return (lowestValue, ret);
        }

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
        public static (float Value, uint Index) Maximum(this float[] vector)
        {
            var ret = uint.MaxValue;
            var highestValue = float.MinValue;

            for (uint i = 0, len = (uint)vector.Length; i < len; i++) {
                var val = vector[i];
                if (val > highestValue) {
                    highestValue = val;
                    ret = i;
                }
            }

            return (highestValue, ret);
        }

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
        public static float[] Softmax(this float[] vector)
        {
            var max = MaximumValue(vector);

            var softmax = vector.Select(v => MathF.Exp(v - max)).ToArray();
            var sum = softmax.Sum();
            if (FloatMath.IsNotZero(sum))
                softmax = softmax.Select(v => v / sum).ToArray();
            return softmax;
        }
    }
}
