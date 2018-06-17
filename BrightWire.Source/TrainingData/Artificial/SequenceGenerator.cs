using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Generates random alphabetical sequences
    /// </summary>
    public class SequenceGenerator
    {
        readonly int _dictionarySize;
        readonly int _minSize, _maxSize;
        readonly bool _noRepeat;
        readonly Random _rnd;

        static char[] _dictionary;
        static Dictionary<char, int> _charTable;

        static SequenceGenerator()
        {
            _dictionary = Enumerable.Range(0, 26 * 2)
                .Select(i => (i < 26) ? (char)('A' + i) : (char)('a' + i - 26))
                .ToArray()
            ;
            _charTable = _dictionary
                .Select((ch, i) => (ch, i))
                .ToDictionary(d => d.Item1, d => d.Item2)
            ;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dictionarySize">The number of letters to use</param>
        /// <param name="minSize">The minimum size of each sequence</param>
        /// <param name="maxSize">The maximum size of each sequence</param>
        /// <param name="noRepeat">True to avoid repeating any previous character within each sequence</param>
        /// <param name="isStochastic">True to generate different sequences each time</param>
        public SequenceGenerator(int dictionarySize, int minSize, int maxSize, bool noRepeat = true, bool isStochastic = true)
        {
            _rnd = isStochastic ? new Random() : new Random(0);
            _noRepeat = noRepeat;
            _dictionarySize = Math.Min(_dictionary.Length, dictionarySize);
            if (noRepeat) {
                _minSize = Math.Min(_dictionarySize, minSize);
                _maxSize = Math.Min(_dictionarySize, maxSize);
            } else {
                _minSize = minSize;
                _maxSize = maxSize;
            }
        }

        /// <summary>
        /// The number of letters to use
        /// </summary>
        public int DictionarySize => _dictionarySize;

        int _NextSequenceLength
        {
            get
            {
                if(_maxSize > _minSize)
                    return _rnd.Next(_minSize, _maxSize);
                return _minSize;
            }
        }

        /// <summary>
        /// Generates a new sequence
        /// </summary>
        /// <returns></returns>
        public string NextSequence()
        {
            var sb = new StringBuilder();
            if (_noRepeat) {
                var indices = Enumerable.Range(0, _dictionarySize).Shuffle(_rnd).Take(_NextSequenceLength);
                foreach(var index in indices)
                    sb.Append(_dictionary[index]);
            } else {
                for (int i = 0, len = _NextSequenceLength; i < len; i++)
                    sb.Append(_dictionary[_rnd.Next(0, _dictionarySize)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts the character to a one hot encoded vector
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public FloatVector Encode(char ch, float val = 1f)
        {
            var ret = new float[_dictionarySize];
            ret[_charTable[ch]] = val;
            return new FloatVector {
                Data = ret
            };
        }

        /// <summary>
        /// Encodes the characters as a dense vector
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public FloatVector Encode(IEnumerable<(char, float)> data)
        {
            var ret = new float[_dictionarySize];
            foreach(var item in data)
                ret[_charTable[item.Item1]] = item.Item2;
            return new FloatVector {
                Data = ret
            };
        }

        /// <summary>
        /// Encodes the string as a list of dense vectors within a matrix (each character becomes a row in the matrix)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public FloatMatrix Encode(string str)
        {
            var data = new FloatVector[str.Length];
            for(int i = 0, len = str.Length; i < len; i++)
                data[i] = Encode(str[i]);

            return FloatMatrix.Create(data);
        }

        /// <summary>
        /// Generator function to generate new sequences
        /// </summary>
        /// <returns>Infinite number of sequences</returns>
        public IEnumerable<string> GenerateSequences()
        {
            while (true)
                yield return NextSequence();
        }
    }
}
