using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Net4.Helper
{
    internal class MiniBatch : IMiniBatch
    {
        public IMatrix Input { get; private set; }
        public IMatrix ExpectedOutput { get; private set; }

        public MiniBatch(IMatrix input, IMatrix expectedOutput)
        {
            Input = input;
            ExpectedOutput = expectedOutput;
        }

        public void Dispose()
        {
            Input.Dispose();
            ExpectedOutput.Dispose();
        }
    }
}
