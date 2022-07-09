using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.DataTable2;
using BrightData.LinearAlgebra;
using BrightWire.TrainingData.Helper;

namespace BrightWire.TrainingData.Artificial
{
    /// <summary>
    /// Generates Reber grammar sequences: https://www.willamette.edu/~gorr/classes/cs449/reber.html
    /// </summary>
    public class ReberGrammar
    {
        static readonly char[] Chars = "BTSXPVE".ToCharArray();
        static readonly Dictionary<char, int> Ch = Chars.Select((c, i) => (Char: c, Index: i)).ToDictionary(d => d.Char, d => d.Index);

        /// <summary>
        /// Gets the character at the specified index
        /// </summary>
        /// <param name="index">Index to query</param>
        public static char GetChar(int index)
        {
            return Chars[index];
        }

        /// <summary>
        /// Gets the index for the specified character
        /// </summary>
        /// <param name="ch">The character to query</param>
        /// <returns></returns>
        public static int GetIndex(char ch)
        {
            return Ch[ch];
        }

        /// <summary>
        /// Encodes a reber sequence as a sequence of one hot encoded vectors
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sequence">The reber sequence to encode</param>
        public static IMatrixInfo Encode(BrightDataContext context, string sequence)
        {
            return context.CreateMatrixInfoFromRows(sequence.Select(ch => {
                    var ret = new float[Ch.Count];
                    ret[Ch[ch]] = 1f;
                    return context.CreateVectorInfo(ret);
                }).ToArray()
            );
        }

        /// <summary>
        /// One hot encodes the REBER strings
        /// </summary>
        /// <param name="context"></param>
        /// <param name="strList">A list of REBER sequences</param>
        /// <returns>A data table with matrices to represent the sequences of vectors and their corresponding outputs</returns>
        public static BrightDataTable GetOneHot(BrightDataContext context, IEnumerable<string> strList)
        {
	        var strList2 = strList.ToList();

            // build the following item table
            var following = new Dictionary<string, HashSet<int>>();
            foreach (var str in strList2) {
                var sb = new StringBuilder();
                string? prev = null;
                foreach (var ch in str) {
                    sb.Append(ch);
                    var key = sb.ToString();
                    if (prev != null) {
                        if (!following.TryGetValue(prev, out var temp))
                            following.Add(prev, temp = new HashSet<int>());
                        temp.Add(Ch[ch]);
                    }
                    prev = key;
                }
            }

            var builder = context.CreateTwoColumnMatrixTableBuilder();
            foreach (var str in strList2) {
                var inputList = new IVectorInfo[str.Length];
                var outputList = new IVectorInfo[str.Length];

                var sb = new StringBuilder();
                for (var i = 0; i < str.Length; i++) {
                    var ch = str[i];
                    sb.Append(ch);
                    var input = new float[Ch.Count];
                    var output = new float[Ch.Count];
                    input[Ch[ch]] = 1f;
                    if (following.TryGetValue(sb.ToString(), out var temp)) {
                        foreach (var item in temp)
                            output[item] = 1f;
                    }
                    inputList[i] = context.CreateVectorInfo(input);
                    outputList[i] = context.CreateVectorInfo(output);
                }
                builder.AddRow(context.CreateMatrixInfoFromRows(inputList), context.CreateMatrixInfoFromRows(outputList));
            }
            return builder.BuildInMemory();
        }

        /// <summary>
        /// The number of REBER characters
        /// </summary>
        public static int Size => Ch.Count;

	    readonly Random _rnd;

        /// <summary>
        /// Creates a reber grammar builder
        /// </summary>
        /// <param name="random">Random number provider</param>
        public ReberGrammar(Random random)
        {
            _rnd = random;
        }

        /// <summary>
        /// Generates an unlimited number of reber sequences
        /// </summary>
        /// <param name="minlength">Minimum length of the sequences (optional)</param>
        /// <param name="maxLength">Mimimum length of the sequences (optional)</param>
        public IEnumerable<string> Get(int? minlength = null, int? maxLength = null)
        {
            while (true) {
                var ret = Generate();
	            if (minlength.HasValue && ret.Length < minlength.Value)
		            continue;
	            if (maxLength.HasValue && ret.Length > maxLength.Value)
		            continue;
	            yield return ret;
            }
        }

	    /// <summary>
	    /// Generates an unlimited number of extended reber sequences
	    /// </summary>
	    /// <param name="minlength">Minimum length of the sequences (optional)</param>
	    /// <param name="maxLength">Mimimum length of the sequences (optional)</param>
	    public IEnumerable<string> GetExtended(int? minlength = null, int? maxLength = null)
        {
            while (true) {
                var ret = GenerateExtended();
                if (minlength.HasValue && ret.Length < minlength.Value)
                    continue;
	            if (maxLength.HasValue && ret.Length > maxLength.Value)
		            continue;
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
