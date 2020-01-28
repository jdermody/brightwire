using BrightML.Learning;
using BrightML.Models;

namespace BrightML
{
    public static class ExtensionMethods
    {
        public static ITrainer<LogisticRegression, LogisticRegressionTrainer.LogisticRegressionTrainingData> GetLogisticRegressionTrainer(this BrightTable.IDataTable table)
        {
            return LogisticRegressionTrainer.GetTrainer(table);
        }
    }
}
