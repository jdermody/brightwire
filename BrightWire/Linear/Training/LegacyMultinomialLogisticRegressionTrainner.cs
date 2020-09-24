using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightTable;
using BrightWire.Models.Linear;

namespace BrightWire.Linear.Training
{
    public class LegacyMultinomialLogisticRegressionTrainner
    {
        public static MultinomialLogisticRegression Train(IRowOrientedDataTable table, ILinearAlgebraProvider lap, uint trainingIterations, float trainingRate, float lambda, Func<float, bool> costCallback = null)
        {
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var groupedData = table.GroupBy(targetColumnIndex).ToArray();
            var trainingData = groupedData.Select(ld => (ld.Label, Table: table.Mutate(row => {
                row[targetColumnIndex] = (row[targetColumnIndex].ToString() == ld.Label) ? 1f : 0f;
                return row;
            }))).ToArray();

            // train the classifiers on each training data set
            var classifier = new List<LogisticRegression>();
            var label = new List<string>();
            foreach (var item in trainingData) {
                item.Table.SetTargetColumn(targetColumnIndex);
                var trainer = new LegacyLogisticRegressionTrainer(lap, item.Table);
                classifier.Add(trainer.GradientDescent(trainingIterations, trainingRate, lambda, costCallback));
                label.Add(item.Label);
            }

            // build the model
            return new MultinomialLogisticRegression {
                Classification = label.ToArray(),
                Model = classifier.ToArray(),
                FeatureColumn = table.ColumnCount.AsRange().Where(c => c != targetColumnIndex).ToArray()
            };
        }
    }
}
