using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.Helper
{
    /// <summary>
    /// Creates a sequence of training feature vectors using a window of surrounding features at each point in the input sequence
    /// </summary>
    public class SequenceWindowBuilder
    {
        readonly int _before, _after;
        int _outputSize = 0;
        const int DATA_SIZE = 158;

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

        public IReadOnlyList<Tuple<float[], float[]>[]> Get(IReadOnlyList<Tuple<float[], float[]>[]> data)
        {
            int offset;
            var ret = new List<Tuple<float[], float[]>[]>();
            for (var z = 0; z < data.Count; z++) {
                var curr = data[z];
                var currList = new List<Tuple<float[], float[]>>();
                for (var i = 0; i < curr.Length; i++) {
                    var item = curr[i].Item1;
                    var size = item.Length;
                    var windowSize = size;

                    var context = new float[_outputSize = (_before + _after) * windowSize + size];
                    if (_before > 0) {
                        offset = (_before - 1) * size;
                        for (var j = 1; j <= _before && i - j >= 0; j++) {
                            item = curr[i - j].Item1;
                            for (var k = 0; k < windowSize; k++)
                                context[offset + k] = item[k];
                            offset -= windowSize;
                        }
                    }
                    offset = windowSize * _before;
                    for (var j = 0; j < size; j++)
                        context[offset + j] = item[j];

                    offset = windowSize * _before + size;
                    for (var j = 1; j <= _after && i + j < curr.Length; j++) {
                        item = curr[i + j].Item1;
                        for (var k = 0; k < windowSize; k++)
                            context[offset + k] = item[k];
                        offset += windowSize;
                    }
                    currList.Add(Tuple.Create(context, curr[i].Item2));
                }
                ret.Add(currList.ToArray());
            }
            return ret.ToList();
        }

        public IReadOnlyList<float[]> Get(IReadOnlyList<float[]> data)
        {
            var ret = new List<float[]>();
            for (var i = 0; i < data.Count; i++) {
                var item = data[i];
                int size = item.Length, offset;
                var windowSize = size;

                var context = new float[_outputSize = (_before + _after) * windowSize + size];
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
        public int OutputSize { get { return _outputSize; } }
    }
}
