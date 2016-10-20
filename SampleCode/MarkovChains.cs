using BrightWire.Helper;
using BrightWire.Models;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        public static void MarkovChains()
        {
            // tokenise the novel "The Beautiful and the Damned" by F. Scott Fitzgerald
            List<IReadOnlyList<string>> sentences;
            using (var client = new WebClient()) {
                var data = client.DownloadString("http://www.gutenberg.org/cache/epub/9830/pg9830.txt");
                var pos = data.IndexOf("CHAPTER I");
                sentences = SimpleTokeniser.FindSentences(SimpleTokeniser.Tokenise(data.Substring(pos))).ToList();
            }

            // create a markov trainer that uses a window of size 3
            var trainer = Provider.CreateMarkovTrainer3<string>();
            foreach (var sentence in sentences)
                trainer.Add(sentence);
            var model = trainer.Build().AsDictionary;

            // generate some text
            var rand = new Random();
            for (var i = 0; i < 50; i++) {
                var sb = new StringBuilder();
                string prevPrev = default(string), prev = default(string), curr = default(string);
                while (true) {
                    var transitions = model.GetTransitions(prevPrev, prev, curr);
                    var distribution = new Categorical(transitions.Select(d => Convert.ToDouble(d.Probability)).ToArray());
                    var next = transitions[distribution.Sample()].NextState;
                    if (Char.IsLetterOrDigit(next[0]) && sb.Length > 0) {
                        var lastChar = sb[sb.Length - 1];
                        if (lastChar != '\'' && lastChar != '-')
                            sb.Append(' ');
                    }
                    sb.Append(next);

                    if (SimpleTokeniser.IsEndOfSentence(next))
                        break;
                    prevPrev = prev;
                    prev = curr;
                    curr = next;
                }
                Console.WriteLine(sb.ToString());
            }
        }
    }
}
