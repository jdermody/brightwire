using BrightWire.Models.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Creates random integers and returns feature vectors against binary mathematical logic
    /// </summary>
    public static class BinaryIntegers
    {
        /// <summary>
        /// Converts a value to an array of floats, with 1 or 0 for each bit position
        /// </summary>
        /// <param name="val">The number to convert</param>
        static float[] _GetBitArray(int val)
        {
            var data = new BitArray(new int[] { val });
            var ret = new float[32];
            for (var i = 0; i < 32; i++)
                ret[i] = data.Get(i) ? 1f : 0f;
            return ret;
        }

        /// <summary>
        /// Creates random integers added together as feature vectors
        /// The input feature contains two features, one for each bit at that position
        /// The output feature contains a single feature: 1 or 0 if that bit is set in the result
        /// </summary>
        /// <param name="sampleCount">How many samples to generate</param>
        /// <param name="stochastic">True to generate random integers</param>
        /// <returns>A list of sequences</returns>
        public static IReadOnlyList<TrainingSequence> Addition(int sampleCount, bool stochastic)
        {
            Random rand = stochastic ? new Random() : new Random(0);
            var ret = new List<TrainingSequence>();

            for (var i = 0; i < sampleCount; i++) {
                // generate some random numbers (sized to prevent overflow)
                var a = rand.Next(int.MaxValue / 2);
                var b = rand.Next(int.MaxValue / 2);
                
                var a2 = _GetBitArray(a);
                var b2 = _GetBitArray(b);
                var r2 = _GetBitArray(a + b);

                var sequence = new List<TrainingExample>();
                for (int j = 0; j < r2.Length; j++) {
                    var input = new[] { a2[j], b2[j] };
                    var output = new[] { r2[j] };
                    sequence.Add(new TrainingExample(input, output));
                }

                ret.Add(new TrainingSequence {
                    Sequence = sequence.ToArray()
                });
            }
            return ret;
        }
    }
}
