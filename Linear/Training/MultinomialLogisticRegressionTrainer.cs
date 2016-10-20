using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BrightWire.Linear.Training
{
    internal static class MultinomialLogisticRegressionTrainner
    {
        public static MultinomialLogisticRegression Train(IDataTable table, ILinearAlgebraProvider lap, int trainingIterations, float trainingRate, float lambda)
        {
            var trainingData = table.ConvertToBinaryClassification();

            // train the classifiers on each training data set
            var classifier = new List<LogisticRegression>();
            var label = new List<string>();
            foreach(var item in trainingData) {
                classifier.Add(item.Table.TrainLogisticRegression(lap, trainingIterations, trainingRate, lambda));
                label.Add(item.Classification);
            }

            // build the model
            return new MultinomialLogisticRegression {
                Classification = label.ToArray(),
                Model = classifier.ToArray(),
                FeatureColumn = Enumerable.Range(0, table.ColumnCount).Where(c => c != table.TargetColumnIndex).ToArray()
            };
        }
    }
}
