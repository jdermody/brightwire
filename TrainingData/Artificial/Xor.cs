using BrightWire.Models.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Simple XOR training data
    /// </summary>
    public static class XorData
    {
        /// <summary>
        /// Gets the XOR training set as one hot encoded float arrays
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<TrainingExample> Get()
        {
            return new[] {
                new TrainingExample(new [] { 0.0f, 0.0f }, new [] { 0.0f }),
                new TrainingExample(new [] { 1.0f, 0.0f }, new [] { 1.0f }),
                new TrainingExample(new [] { 0.0f, 1.0f }, new [] { 1.0f }),
                new TrainingExample(new [] { 1.0f, 1.0f }, new [] { 0.0f })
            };
        }
    }
}
