using BrightWire;
using BrightWire.Helper;
using BrightWire.TrainingData;
using MathNet.Numerics.Distributions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class MarkovModelTests
    {
        static string _text;

        [ClassInitialize]
        public static void Load(TestContext context)
        {
            // download some text
            using(var client = new WebClient()) {
                var data = client.DownloadString("http://www.gutenberg.net.au/fsf/PAT-HOBBY.txt");
                var pos = data.IndexOf("<pre>");
                var pos2 = data.IndexOf("</pre>", pos);
                _text = data.Substring(pos + 5, pos2 - pos - 5);
            }
        }

        void _Train(IMarkovModelTrainer<string> trainer)
        {
            var tokens = SimpleTokeniser.Tokenise(_text).ToList();
            var sentences = SimpleTokeniser.FindSentences(tokens).ToList();
            foreach (var sentence in sentences)
                trainer.Add(sentence);
        }

        [TestMethod]
        public void TrainModel2()
        {
            var trainer = BrightWireProvider.CreateMarkovTrainer2<string>();
            _Train(trainer);
            var model = trainer.Build().AsDictionary;

            // generate some text
            var rand = new Random();
            string prev = default(string), curr = default(string);
            var output = new List<string>();
            for(var i = 0; i < 1024; i++) {
                var transitions = model.GetTransitions(prev, curr);
                var distribution = new Categorical(transitions.Select(d => Convert.ToDouble(d.Probability)).ToArray());
                var next = transitions[distribution.Sample()].NextState;
                output.Add(next);
                if (SimpleTokeniser.IsEndOfSentence(next))
                    break;
                prev = curr;
                curr = next;
            }
            Assert.IsTrue(output.Count < 1024);
        }

        [TestMethod]
        public void TrainModel3()
        {
            var trainer = BrightWireProvider.CreateMarkovTrainer3<string>();
            _Train(trainer);
            var model = trainer.Build().AsDictionary;

            // generate some text
            var rand = new Random();
            string prevPrev = default(string), prev = default(string), curr = default(string);
            var output = new List<string>();
            for (var i = 0; i < 1024; i++) {
                var transitions = model.GetTransitions(prevPrev, prev, curr);
                var distribution = new Categorical(transitions.Select(d => Convert.ToDouble(d.Probability)).ToArray());
                var next = transitions[distribution.Sample()].NextState;
                output.Add(next);
                if (SimpleTokeniser.IsEndOfSentence(next))
                    break;
                prevPrev = prev;
                prev = curr;
                curr = next;
            }
            Assert.IsTrue(output.Count < 1024);
        }
    }
}
