using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icbld.BrightWire.TrainingData.Artificial
{
    public static class ReberGrammar
    {
        static Random rnd = new Random(0);
        static Dictionary<char, int> _ch = "BTSXPVE".ToCharArray().Select((c, i) => Tuple.Create(c, i)).ToDictionary(d => d.Item1, d => d.Item2);

        public static char GetChar(int index)
        {
            return "BTSXPVE".ToCharArray()[index];
        }

        public static int GetIndex(char ch)
        {
            return _ch[ch];
        }

        public static IEnumerable<Tuple<float[], float[]>[]> GetOneHot(IEnumerable<string> strList)
        {
            // build the following item table
            HashSet<int> temp;
            var following = new Dictionary<string, HashSet<int>>();
            foreach (var str in strList) {
                var sb = new StringBuilder();
                foreach (var ch in str) {
                    sb.Append(ch);
                    var key = sb.ToString();
                    if (!following.TryGetValue(key, out temp))
                        following.Add(key, temp = new HashSet<int>());
                    temp.Add(_ch[ch]);
                }
            }

            var ret = new List<Tuple<float[], float[]>[]>();
            foreach (var str in strList) {
                var sequence = new Tuple<float[], float[]>[str.Length];
                var sb = new StringBuilder();
                for (var i = 0; i < str.Length; i++) {
                    var input = new float[_ch.Count];
                    var output = new float[_ch.Count];
                    var ch = str[i];

                    input[_ch[ch]] = 1f;
                    sb.Append(ch);
                    foreach (var item in following[sb.ToString()])
                        output[item] = 1f;
                    sequence[i] = Tuple.Create(input, output);
                }
                ret.Add(sequence);
            }
            return ret;
        }

        public static int Size { get { return _ch.Count; } }

        public static IEnumerable<string> Get(int length)
        {
            while (true) {
                var ret = Generate();
                if (ret.Length == length)
                    yield return ret;
            }
        }

        public static IEnumerable<string> GetExtended(int? length)
        {
            while (true) {
                var ret = GenerateExtended();
                if (!length.HasValue || (length.HasValue && ret.Length == length.Value))
                    yield return ret;
            }
        }

        static string GenerateExtended()
        {
            if (rnd.NextDouble() < 0.5)
                return "BT" + Generate() + "TE";
            else
                return "BP" + Generate() + "PE";
        }

        static string Generate()
        {
            return DoNode0("B");
        }

        static string DoNode0(string curr)
        {
            if (rnd.NextDouble() < 0.5)
                return DoNode1(curr + 'T');
            else
                return DoNode2(curr + 'P');
        }

        static string DoNode1(string curr)
        {
            if (rnd.NextDouble() < 0.5)
                return DoNode1(curr + 'S');
            else
                return DoNode3(curr + 'X');
        }

        static string DoNode2(string curr)
        {
            if (rnd.NextDouble() < 0.5)
                return DoNode2(curr + 'T');
            else
                return DoNode4(curr + 'V');
        }

        static string DoNode3(string curr)
        {
            if (rnd.NextDouble() < 0.5)
                return DoNode2(curr + 'X');
            else
                return DoNode5(curr + 'S');
        }

        static string DoNode4(string curr)
        {
            if (rnd.NextDouble() < 0.5)
                return DoNode3(curr + 'P');
            else
                return DoNode5(curr + 'V');
        }

        static string DoNode5(string curr)
        {
            return curr + 'E';
        }
    }
}
