using System;
using System.Collections.Generic;
using System.Text;

namespace BrightWire.Connectionist.Helper
{
    internal class SequentialMiniBatch : ISequentialMiniBatch
    {
        readonly IMatrix[] _expectedOutput;

        public int SequenceLength { get; private set; }
        public int BatchSize { get; private set; }
        public IMatrix[] Input { get; private set; }
        public int[] CurrentRows { get; private set; }

        public SequentialMiniBatch(IMatrix[] input, IMatrix[] output, int[] rows)
        {
            Input = input;
            SequenceLength = Input.Length;
            BatchSize = Input[0].RowCount;
            CurrentRows = rows;
            _expectedOutput = output;
        }

        public void Dispose()
        {
            foreach (var item in _expectedOutput)
                item.Dispose();
            foreach (var item in Input)
                item.Dispose();
        }

        public IMatrix GetExpectedOutput(IReadOnlyList<IMatrix> output, int k)
        {
            return _expectedOutput[k];
        }
    }
}
