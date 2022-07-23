using System;
using System.Collections;
using BrightData;
using BrightData.LinearAlgebra;
using BrightWire.TrainingData.Helper;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

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
        static float[] GetBitArray(int val)
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
        public static BrightDataTable Addition(BrightDataContext context, int sampleCount)
        {
            var rand = context.Random;
            var builder = context.CreateTwoColumnMatrixTableBuilder();

            for (var i = 0; i < sampleCount; i++) {
                // generate some random numbers (sized to prevent overflow)
                var a = rand.Next(int.MaxValue / 2);
                var b = rand.Next(int.MaxValue / 2);

                var a2 = GetBitArray(a);
                var b2 = GetBitArray(b);
                var r2 = GetBitArray(a + b);

                var inputList = new IReadOnlyVector[r2.Length];
                var outputList = new IReadOnlyVector[r2.Length];
                for (var j = 0; j < r2.Length; j++) {
                    inputList[j] = context.CreateReadOnlyVector(a2[j], b2[j]);
                    outputList[j] = context.CreateReadOnlyVector(r2[j]);
                }

                var input = context.CreateReadOnlyMatrixFromRows(inputList);
                var output = context.CreateReadOnlyMatrixFromRows(outputList);
                builder.AddRow(input, output);
            }
            return builder.BuildInMemory();
        }
    }
}
