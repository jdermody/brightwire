using BrightWire.Models.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Generates Reber grammar sequences: https://www.willamette.edu/~gorr/classes/cs449/reber.html
    /// </summary>
    public class ReberGrammar
    {
        static char[] CHARS = "BTSXPVE".ToCharArray();
        static Dictionary<char, int> _ch = CHARS.Select((c, i) => Tuple.Create(c, i)).ToDictionary(d => d.Item1, d => d.Item2);

        /// <summary>
        /// Gets the character at the specified index
        /// </summary>
        /// <param name="index">Index to query</param>
        public static char GetChar(int index)
        {
            return CHARS[index];
        }

        /// <summary>
        /// Gets the index for the specified character
        /// </summary>
        /// <param name="ch">The character to query</param>
        /// <returns></returns>
        public static int GetIndex(char ch)
        {
            return _ch[ch];
        }

        /// <summary>
        /// One hot encodes the REBER strings
        /// </summary>
        /// <param name="strList">A list of REBER sequences</param>
        /// <returns>A sequence of tuples of { input, output }</returns>
        public static IEnumerable<TrainingExample[]> GetOneHot(IEnumerable<string> strList)
        {
            // build the following item table
            HashSet<int> temp;
            var following = new Dictionary<string, HashSet<int>>();
            foreach (var str in strList) {
                var sb = new StringBuilder();
                string prev = null;
                foreach (var ch in str) {
                    sb.Append(ch);
                    var key = sb.ToString();
                    if (prev != null) {
                        if (!following.TryGetValue(prev, out temp))
                            following.Add(prev, temp = new HashSet<int>());
                        temp.Add(_ch[ch]);
                    }
                    prev = key;
                }
            }

            var ret = new List<TrainingExample[]>();
            foreach (var str in strList) {
                var sequence = new TrainingExample[str.Length];
                var sb = new StringBuilder();
                for (var i = 0; i < str.Length; i++) {
                    var ch = str[i];
                    sb.Append(ch);
                    var input = new float[_ch.Count];
                    var output = new float[_ch.Count];
                    input[_ch[ch]] = 1f;
                    if (following.ContainsKey(sb.ToString())) {
                        foreach (var item in following[sb.ToString()])
                            output[item] = 1f;
                    }
                    sequence[i] = new TrainingExample(input, output);
                }
                ret.Add(sequence);
            }
            return ret;
        }

        /// <summary>
        /// The number of REBER characters
        /// </summary>
        public static int Size { get { return _ch.Count; } }

        readonly Random _rnd;

        /// <summary>
        /// Creates a reber grammar builder
        /// </summary>
        /// <param name="stochastic">True to generate random sequences</param>
        public ReberGrammar(bool stochastic = true)
        {
            _rnd = stochastic ? new Random() : new Random(0);
        }

        /// <summary>
        /// Generates an unlimited number of reber sequences
        /// </summary>
        /// <param name="length">Optional required length of the sequences</param>
        public IEnumerable<string> Get(int? length)
        {
            while (true) {
                var ret = Generate();
                if (!length.HasValue || (length.HasValue && ret.Length == length.Value))
                    yield return ret;
            }
        }

        /// <summary>
        /// Generates an unlimited number of extended reber sequences
        /// </summary>
        /// <param name="length">Optional required length of the sequences</param>
        public IEnumerable<string> GetExtended(int? length)
        {
            while (true) {
                var ret = GenerateExtended();
                if (!length.HasValue || (length.HasValue && ret.Length == length.Value))
                    yield return ret;
            }
        }

        string GenerateExtended()
        {
            if (_rnd.NextDouble() < 0.5)
                return "BT" + Generate() + "TE";
            else
                return "BP" + Generate() + "PE";
        }

        string Generate()
        {
            return DoNode0("B");
        }

        string DoNode0(string curr)
        {
            if (_rnd.NextDouble() < 0.5)
                return DoNode1(curr + 'T');
            else
                return DoNode2(curr + 'P');
        }

        string DoNode1(string curr)
        {
            if (_rnd.NextDouble() < 0.5)
                return DoNode1(curr + 'S');
            else
                return DoNode3(curr + 'X');
        }

        string DoNode2(string curr)
        {
            if (_rnd.NextDouble() < 0.5)
                return DoNode2(curr + 'T');
            else
                return DoNode4(curr + 'V');
        }

        string DoNode3(string curr)
        {
            if (_rnd.NextDouble() < 0.5)
                return DoNode2(curr + 'X');
            else
                return DoNode5(curr + 'S');
        }

        string DoNode4(string curr)
        {
            if (_rnd.NextDouble() < 0.5)
                return DoNode3(curr + 'P');
            else
                return DoNode5(curr + 'V');
        }

        string DoNode5(string curr)
        {
            return curr + 'E';
        }
    }
}
