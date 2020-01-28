using System;
using System.Collections.Generic;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightML.Models;
using BrightTable;

namespace BrightML.Learning
{
    public static class LogisticRegressionTrainer
    {
        public class LogisticRegressionTrainingData : ITrainingData
        {
            private readonly IBrightDataContext _context;
            public IComputableMatrix Feature { get; }
            public IComputableVector Target { get; }
            public IComputableVector Theta { get; }

            public LogisticRegressionTrainingData(IBrightDataContext context, IComputableMatrix feature, IComputableVector theta, IComputableVector target)
            {
                _context = context;
                Feature = feature;
                Theta = theta;
                Target = target;
            }

            public void Dispose()
            {
                Feature.Dispose();
                Target.Dispose();
                Theta.Dispose();
            }

            public LogisticRegression CreateModel() => new LogisticRegression(Theta.ToVector(_context));

            public IComputableVector Derivative(IComputableVector theta, float lambda)
            {
                using var p0 = Feature.Multiply(theta);
                using var p1 = p0.ReshapeAsVector();
                using var p = p1.Sigmoid();
                using var e0 = Target.Subtract(p);
                using var e = e0.ReshapeAsSingleRowMatrix();
                using var e2 = e.Multiply(Feature);

                e2.MultiplyInPlace(1f / Feature.RowCount);
                var ret = e2.ReshapeAsVector();

                if (FloatMath.IsNotZero(lambda)) {
                    using var regularisation = theta.Clone();
                    regularisation.MultiplyInPlace(lambda / Feature.ColumnCount);
                    ret.AddInPlace(regularisation);
                }
                return ret;
            }
        }

        public static ITrainer<LogisticRegression, LogisticRegressionTrainingData> GetTrainer(BrightTable.IDataTable dataTable)
        {
            var columns = dataTable.AllColumns();

            // find the classification target
            var classificationTarget = columns.SingleOrDefault(c => c.IsTarget() && c.IsNumeric());
            if (classificationTarget == null)
                throw new ArgumentException("Table does not contain a numeric target column");

            // find the numeric input columns
            var numericColumns = columns.Where(c => c != classificationTarget && c.IsNumeric()).ToList();
            if (!numericColumns.Any())
                throw new ArgumentException("Table does not contain any numeric data columns");

            // copy the feature vectors
            var context = dataTable.Context;
            var feature = context.CreateMatrix<float>(dataTable.RowCount, (uint)numericColumns.Count + 1);
            uint columnIndex = 1;
            foreach (var column in numericColumns)
                column.CopyTo(feature.Column(columnIndex++).Data);
            feature.Column(0).Data.Initialize(1f);

            // copy the target vector
            var target = context.CreateVector<float>(dataTable.RowCount);
            classificationTarget.CopyTo(target.Data);

            var theta = context.CreateVector<float>(feature.ColumnCount);
            theta.Initialize(0);
            //theta.InitializeRandomly();

            var data = new LogisticRegressionTrainingData(context, feature.AsComputable(), theta.AsComputable(), target.AsComputable());
            return new Trainer<LogisticRegression, LogisticRegressionTrainingData>(data, _Cost, _GradientDescent, _Evaluate, _GetModel);
        }

        static LogisticRegression _GetModel(LogisticRegressionTrainingData data) => data.CreateModel();

        static void _GradientDescent(ITrainer<LogisticRegression, LogisticRegressionTrainingData> trainer, ITrainingContext context)
        {
            var data = trainer.Data;
            var theta = data.Theta;

            using var d = data.Derivative(theta, context.Lambda);
            d.MultiplyInPlace(context.LearningRate);
            theta.AddInPlace(d);
        }

        static float _Cost(ITrainer<LogisticRegression, LogisticRegressionTrainingData> trainer, ITrainingContext context)
        {
            //var data = trainer.Data;
            //return data.Cost(data.Theta, context.Lambda);
            return 0;
        }

        static IReadOnlyList<(float Output, float Target)> _Evaluate(ITrainer<LogisticRegression, LogisticRegressionTrainingData> trainer)
        {
            var feature = trainer.Data.Feature;
            var theta = trainer.Data.Theta;
            var target = trainer.Data.Target;

            using var h0 = feature.Multiply(theta);
            using var h1 = h0.ReshapeAsVector();
            using var h = h1.Sigmoid();

            return h.GetInternalArray().Zip(target.GetInternalArray(), (o, t) => (o, t)).ToList();
        }
    }
}
