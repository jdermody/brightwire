using BrightWire.Helper;
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
    data = data.Substring(pos);
    sentences = SimpleTokeniser.FindSentences(SimpleTokeniser.Tokenise(data)).ToList();
}

            // create a markov trainer that uses a window of size 3
            var trainer = Provider.CreateMarkovTrainer3<string>();
            foreach (var sentence in sentences)
                trainer.Add(sentence);
            var model = trainer.All.ToDictionary(d => Tuple.Create(d.Item1, d.Item2, d.Item3), d => d.Transition);

// generate some text
var rand = new Random();
var sb = new StringBuilder();
for (var i = 0; i < 50; i++) {
    string prevPrev = default(string), prev = default(string), curr = default(string);
    while (true) {
        var possibleStates = model[Tuple.Create(prevPrev, prev, curr)];
        var distribution = new Categorical(possibleStates.Select(d => Convert.ToDouble(d.Probability)).ToArray());
        var next = possibleStates[distribution.Sample()].NextState;
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
    sb.Append('\n');
}
Console.WriteLine(sb.ToString());
        }
    }
}
