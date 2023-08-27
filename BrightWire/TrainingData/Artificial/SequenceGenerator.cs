using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Generates random alphabetical sequences
    /// </summary>
    public class SequenceGenerator
    {
	    readonly uint _minSize, _maxSize;
        readonly BrightDataContext _context;
        readonly bool _noRepeat;
        readonly Random _rnd;

        static readonly char[] Dictionary;
        static readonly Dictionary<char, int> CharTable;

        static SequenceGenerator()
        {
            Dictionary = Enumerable.Range(0, 26 * 2)
                .Select(i => (i < 26) ? (char)('A' + i) : (char)('a' + i - 26))
                .ToArray()
            ;
            CharTable = Dictionary
                .Select((ch, i) => (Char: ch, Index: i))
                .ToDictionary(d => d.Char, d => d.Index)
            ;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dictionarySize">The number of letters to use</param>
        /// <param name="minSize">The minimum size of each sequence</param>
        /// <param name="maxSize">The maximum size of each sequence</param>
        /// <param name="noRepeat">True to avoid repeating any previous character within each sequence</param>
        public SequenceGenerator(BrightDataContext context, int dictionarySize, uint minSize, uint maxSize, bool noRepeat = true)
        {
            _rnd = context.Random;
            _context = context;
            _noRepeat = noRepeat;
            DictionarySize = (uint)Math.Min(Dictionary.Length, dictionarySize);
            if (noRepeat) {
                _minSize = Math.Min(DictionarySize, minSize);
                _maxSize = Math.Min(DictionarySize, maxSize);
            } else {
                _minSize = minSize;
                _maxSize = maxSize;
            }
        }

        /// <summary>
        /// The number of letters to use
        /// </summary>
        public uint DictionarySize { get; }

	    uint NextSequenceLength
        {
            get
            {
                if(_maxSize > _minSize)
                    return (uint)_rnd.Next((int)_minSize, (int)_maxSize);
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
                var indices = DictionarySize.AsRange().Shuffle(_rnd).Take((int)NextSequenceLength);
                foreach(var index in indices)
                    sb.Append(Dictionary[index]);
            } else {
                for (uint i = 0, len = NextSequenceLength; i < len; i++)
                    sb.Append(Dictionary[_rnd.Next(0, (int)DictionarySize)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts the character to a one hot encoded vector
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public IReadOnlyVector Encode(char ch, float val = 1f)
        {
            var ret = new float[DictionarySize];
            ret[CharTable[ch]] = val;
            return _context.CreateReadOnlyVector(ret);
        }

        /// <summary>
        /// Encodes the characters as a dense vector
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IReadOnlyVector Encode(IEnumerable<(char, float)> data)
        {
            var ret = new float[DictionarySize];
            foreach(var item in data)
                ret[CharTable[item.Item1]] = item.Item2;
            return _context.CreateReadOnlyVector(ret);
        }

        /// <summary>
        /// Encodes the string as a list of dense vectors within a matrix (each character becomes a row in the matrix)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public IReadOnlyMatrix Encode(string str)
        {
            var data = new IReadOnlyVector[str.Length];
            for(int i = 0, len = str.Length; i < len; i++)
                data[i] = Encode(str[i]);

            return _context.CreateReadOnlyMatrixFromRows(data);
        }

        /// <summary>
        /// Generator function to generate new sequences
        /// </summary>
        /// <returns>Infinite number of sequences</returns>
        public IEnumerable<string> GenerateSequences()
        {
            while (true)
                yield return NextSequence();
            // ReSharper disable once IteratorNeverReturns
        }

        /// <summary>
        /// Converts character indices to a string
        /// </summary>
        /// <param name="characters">Character indices</param>
        /// <returns></returns>
        public string Decode(IEnumerable<uint> characters) => new(characters.Select(i => Dictionary[i]).ToArray());
    }
}
