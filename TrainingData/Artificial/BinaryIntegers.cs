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
        /// Converts a byte value to an array of floats, with 1 or 0 for each bit position
        /// </summary>
        /// <param name="val">The number to convert</param>
        static float[] _GetBitArray(byte val)
        {
            var data = new BitArray(new byte[] { val });
            var ret = new float[8];
            for (var i = 0; i < 8; i++)
                ret[i] = data.Get(i) ? 1f : 0f;
            return ret;
        }

        /// <summary>
        /// Creates random integers added together as feature vectors
        /// The input feature contains two features, one for each bit at that position
        /// The output feature contains a single feature: 1 or 0 if that bit is set in the result
        /// </summary>
        /// <param name="sampleCount">How many samples to generate</param>
        /// <returns>A list of sequences</returns>
        public static IReadOnlyList<IReadOnlyList<Tuple<float[], float[]>>> Addition(int sampleCount)
        {
            var rand = new Random(0);
            var ret = new List<IReadOnlyList<Tuple<float[], float[]>>>();

            for (var i = 0; i < sampleCount; i++) {
                // generate some random numbers
                var a = Convert.ToByte(rand.Next(128));
                var b = Convert.ToByte(rand.Next(128));

                
                var a2 = _GetBitArray(a);
                var b2 = _GetBitArray(b);
                var r2 = _GetBitArray(Convert.ToByte(a + b));

                var sequence = new List<Tuple<float[], float[]>>();
                for (int j = 0; j < 8; j++) {
                    var input = new[] { a2[j], b2[j] };
                    var output = new[] { r2[j] };
                    sequence.Add(Tuple.Create(input, output));
                }

                ret.Add(sequence);
            }
            return ret;
        }
    }
}
