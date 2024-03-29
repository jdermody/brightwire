﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightWire.Models.Linear;

namespace BrightWire.Linear.Training
{
    /// <summary>
    /// Logistic regression with multiple possible classifications
    /// </summary>
    internal static class MultinomialLogisticRegressionTrainner
    {
        public static MultinomialLogisticRegression Train(IRowOrientedDataTable table, uint trainingIterations, float trainingRate, float lambda, Func<float, bool>? costCallback = null)
        {
            var context = table.Context;
            var targetColumnIndex = table.GetTargetColumnOrThrow();
            var featureColumns = table.ColumnIndicesOfFeatures().ToArray();
            var targetGroups = new Dictionary<string, HashSet<object[]>>();
            table.ForEachRow(row => {
                var label = row[targetColumnIndex].ToString() ?? throw new Exception("Cannot get string representation");
                if (!targetGroups.TryGetValue(label, out var set))
                    targetGroups.Add(label, set = new HashSet<object[]>());
                set.Add(row);
            });

            // create a new table for each label
            var trainingTables = targetGroups.Select(d => {
                var builder = context.BuildTable();
                builder.CopyColumnsFrom(table, featureColumns);
                builder.AddColumn(BrightDataType.Float, "Target").SetTarget(true);

                // add the rows for this classification
                foreach (var row in d.Value) {
                    row[targetColumnIndex] = 1f;
                    builder.AddRow((object[])row.Clone());
                }

                // add the rows for the contrary classifications
                foreach (var other in targetGroups.Where(k => k.Key != d.Key)) {
                    foreach (var row in other.Value) {
                        row[targetColumnIndex] = 0f;
                        builder.AddRow((object[])row.Clone());
                    }
                }

                return (Label: d.Key, Table: builder.BuildRowOriented());
            });

            // train the classifiers on each training data set
            var classifier = new List<LogisticRegression>();
            var labels = new List<string>();
            foreach (var (label, rowOrientedDataTable) in trainingTables) {
                classifier.Add(rowOrientedDataTable.TrainLogisticRegression(trainingIterations, trainingRate, lambda, costCallback));
                labels.Add(label);
            }

            // build the model
            return new MultinomialLogisticRegression {
                Classification = labels.ToArray(),
                Model = classifier.ToArray(),
                FeatureColumn = featureColumns
            };
        }
    }
}
