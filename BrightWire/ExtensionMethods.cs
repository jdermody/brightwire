using System;
using System.Collections.Generic;
using System.Text;
using BrightTable;
using BrightWire.Learning;
using BrightWire.Models;

namespace BrightWire
{
    public static class ExtensionMethods
    {
        public static ITrainer<LogisticRegression, LogisticRegressionTrainer.LogisticRegressionTrainingData> GetLogisticRegressionTrainer(this BrightTable.IDataTable table)
        {
            return LogisticRegressionTrainer.GetTrainer(table);
        }
    }
}
