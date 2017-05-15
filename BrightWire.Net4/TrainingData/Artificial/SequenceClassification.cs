using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TrainingData.Artificial
{
    public class SequenceClassification
    {
        readonly int _dictionarySize;
        readonly int _minSize, _maxSize;
        readonly bool _noRepeat;
        readonly Random _rnd;

        static char[] _dictionary;
        static Dictionary<char, int> _charTable = new Dictionary<char, int>();

        static SequenceClassification()
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

        public SequenceClassification(int dictionarySize, int minSize, int maxSize, bool noRepeat = true, bool isStochastic = true)
        {
            _rnd = isStochastic ? new Random() : new Random(0);
            _noRepeat = noRepeat;
            _dictionarySize = Math.Min(_dictionary.Length, dictionarySize);
            if (noRepeat) {
                _minSize = Math.Min(_dictionarySize, _minSize);
                _maxSize = Math.Min(_dictionarySize, _maxSize);
            } else {
                _minSize = minSize;
                _maxSize = maxSize;
            }
        }

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

        public FloatVector Encode(char ch, float val = 1f)
        {
            var ret = new float[_dictionarySize];
            ret[_charTable[ch]] = val;
            return new FloatVector {
                Data = ret
            };
        }

        public FloatVector Encode(IEnumerable<(char, float)> data)
        {
            var ret = new float[_dictionarySize];
            foreach(var item in data)
                ret[_charTable[item.Item1]] = item.Item2;
            return new FloatVector {
                Data = ret
            };
        }

        public FloatMatrix Encode(string str)
        {
            var data = new FloatVector[str.Length];
            for(int i = 0, len = str.Length; i < len; i++)
                data[i] = Encode(str[i]);

            return new FloatMatrix {
                Row = data
            };
        }

        public IEnumerable<string> GenerateSequences()
        {
            while (true)
                yield return NextSequence();
        }
    }
}
