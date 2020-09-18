using BrightTable;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections;
using BrightData;
using BrightData.FloatTensors;
using BrightWire.TrainingData.Helper;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Creates random integers and returns feature vectors against binary mathematical logic
    /// </summary>
    public class BinaryIntegers
    {
        /// <summary>
        /// Converts a value to an array of floats, with 1 or 0 for each bit position
        /// </summary>
        /// <param name="val">The number to convert</param>
        static float[] _GetBitArray(int val)
        {
            var data = new BitArray(new[] { val });
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
        /// <param name="context"></param>
        /// <param name="sampleCount">How many samples to generate</param>
        /// <returns>A list of sequences</returns>
        public static IRowOrientedDataTable Addition(IBrightDataContext context, int sampleCount)
        {
            Random rand = context.Random;
            var builder = DataTableBuilder.CreateTwoColumnMatrixTableBuilder(context);

            for (var i = 0; i < sampleCount; i++) {
                // generate some random numbers (sized to prevent overflow)
                var a = rand.Next(int.MaxValue / 2);
                var b = rand.Next(int.MaxValue / 2);

                var a2 = _GetBitArray(a);
                var b2 = _GetBitArray(b);
                var r2 = _GetBitArray(a + b);

                var inputList = new Vector<float>[r2.Length];
                var outputList = new Vector<float>[r2.Length];
                for (int j = 0; j < r2.Length; j++) {
                    inputList[j] = context.CreateVector(a2[j], b2[j]);
                    outputList[j] = context.CreateVector(r2[j]);
                }
                builder.AddRow(context.CreateMatrixFromRows(inputList), context.CreateMatrixFromRows(outputList));
            }
            return builder.Build();
        }
    }
}
