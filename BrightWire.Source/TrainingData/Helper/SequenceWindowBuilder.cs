using System.Collections.Generic;

namespace BrightWire.TrainingData.Helper
{
    /// <summary>
    /// Creates a new training feature vector using a window of surrounding features at each point in the input sequence
    /// </summary>
    public class SequenceWindowBuilder
    {
        readonly int _before, _after;

	    /// <summary>
        /// Creates a new sequence window builder
        /// </summary>
        /// <param name="before">The number of previous items to include before each item</param>
        /// <param name="after">The number of following items to include after each item</param>
        public SequenceWindowBuilder(int before, int after)
        {
            _before = before;
            _after = after;
        }

        /// <summary>
        /// Augments a single sequence
        /// </summary>
        /// <param name="data">The sequence to analyse</param>
        /// <returns>A new sequence, augmented with contextual information</returns>
        public IReadOnlyList<float[]> Get(IReadOnlyList<float[]> data)
        {
            var ret = new List<float[]>();
            for (var i = 0; i < data.Count; i++) {
                var item = data[i];
                int size = item.Length, offset;
                var windowSize = size;

                var context = new float[OutputSize = (_before + _after) * windowSize + size];
                if (_before > 0) {
                    offset = (_before - 1) * size;
                    for (var j = 1; j <= _before && i - j >= 0; j++) {
                        item = data[i - j];
                        for (var k = 0; k < windowSize; k++)
                            context[offset + k] = item[k];
                        offset -= windowSize;
                    }
                }
                offset = windowSize * _before;
                for (var j = 0; j < size; j++)
                    context[offset + j] = item[j];

                offset = windowSize * _before + size;
                for (var j = 1; j <= _after && i + j < data.Count; j++) {
                    item = data[i + j];
                    for (var k = 0; k < windowSize; k++)
                        context[offset + k] = item[k];
                    offset += windowSize;
                }
                ret.Add(context);
            }
            return ret;
        }

        /// <summary>
        /// The size of the generated training data
        /// </summary>
        public int OutputSize { get; set; } = 0;
    }
}
