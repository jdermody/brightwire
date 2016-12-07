using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.SampleCode
{
    public partial class Program
    {
        static IDataTable _LoadAdult(string path, bool skipFirstLine)
        {
            using (var streamReader = new StreamReader(path)) {
                if (skipFirstLine)
                    streamReader.ReadLine();
                return streamReader.ParseCSV(',', false);
            }
        }

        public static void IncomePrediction(string trainingDataPath, string testDataPath)
        {
            var trainingTable = _LoadAdult(trainingDataPath, false);
            var testTable = _LoadAdult(testDataPath, true);
            var targetColumnIndex = testTable.TargetColumnIndex;
            var testTable2 = testTable.Project(row => {
                var label = row.GetField<string>(targetColumnIndex);
                var ret = row.Data.Take(targetColumnIndex).Concat(new[] { label.Substring(0, label.Length - 1) }).ToList();
                return ret;
            });

            var naiveBayes = trainingTable.TrainNaiveBayes();
            Console.WriteLine("Naive bayes accuracy: {0:P}", testTable2
                .Classify(naiveBayes.CreateClassifier())
                .Average(d => d.Row.GetField<string>(targetColumnIndex) == d.Classification ? 1.0 : 0.0)
            );

            var adaBoost = trainingTable.CreateBoostedTrainer();
            adaBoost.AddClassifiers(3, dt => {
                var decisionTree = dt.TrainDecisionTree(null, 1, null, 1024);
                return decisionTree.CreateClassifier();
            });

            var results = adaBoost.Classify(testTable2);
            int finalCorrect = 0, finalTotal = 0;
            foreach (var item in results) {
                var label = item.Row.GetField<string>(targetColumnIndex);
                if (label == item.Classification)
                    ++finalCorrect;
                ++finalTotal;
            }
            Console.WriteLine("Final accuracy: {0:P}", (float)finalCorrect / finalTotal);
        }
    }
}
