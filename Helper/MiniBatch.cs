using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Helper
{
    /// <summary>
    /// Mini batch of training samples
    /// </summary>
    public class MiniBatch : IMiniBatch
    {
        /// <summary>
        /// A matrix with rows for each sample within the batch
        /// </summary>
        public IMatrix Input { get; private set; }

        /// <summary>
        /// A matrix with rows for each expected output (associated with each input row)
        /// </summary>
        public IMatrix ExpectedOutput { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input">Input matrix</param>
        /// <param name="expectedOutput">Expected output matrix</param>
        public MiniBatch(IMatrix input, IMatrix expectedOutput)
        {
            Input = input;
            ExpectedOutput = expectedOutput;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Input.Dispose();
            ExpectedOutput.Dispose();
        }
    }
}
