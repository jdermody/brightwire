using BrightData;
using BrightTable;
using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Helper;
using BrightWire.Models;

namespace BrightWire.Learning
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

            public float Cost(IComputableVector theta, float lambda)
            {
                using var h0 = Feature.Multiply(theta);
                using var h1 = h0.Column(0);
                using var h = h1.Sigmoid();
                using var hLog = h.Log();
                using var t = Target.Clone();
                var a = Target.DotProduct(hLog);

                t.MultiplyInPlace(-1f);
                t.AddInPlace(1f);

                h.MultiplyInPlace(-1f);
                h.AddInPlace(1f);

                var b = t.DotProduct(hLog);
                var ret = -(a + b) / Feature.RowCount;
                if (FloatMath.IsNotZero(lambda))
                    ret += theta.GetInternalArray().Skip(1).Select(v => v * v).Sum() * lambda / (2 * Feature.RowCount);
                return ret;
            }

            public IComputableVector Derivative(IComputableVector theta, float lambda)
            {
                using var p0 = Feature.Multiply(theta);
                using var p1 = p0.Column(0);
                using var p = p1.Sigmoid();
                using var e0 = p.Subtract(Target);
                using var e = e0.ReshapeAsRowMatrix();
                using var e2 = e.Multiply(Feature);

                e2.MultiplyInPlace(1f / Feature.RowCount);
                var ret = e2.Row(0);
                if (FloatMath.IsNotZero(lambda)) {
                    var size = theta.Size;
                    var reg = new float[size];
                    var term = lambda / Feature.RowCount;
                    for (var i = 1; i < size; i++) {
                        reg[i] = theta[i] * term;
                    }

                    using var regVector = _context.CreateVector(reg).AsComputable();
                    ret.AddInPlace(regVector);
                }
                return ret;
            }
        }

        public static ITrainer<LogisticRegression, LogisticRegressionTrainingData> GetTrainer(BrightTable.IDataTable dataTable)
        {
            var columns = dataTable.AllColumns();

            // find the classification target
            var classificationTarget = columns.SingleOrDefault(c => c.IsTarget() && c.IsNumeric());
            if(classificationTarget == null)
                throw new ArgumentException("Table does not contain a numeric target column");

            // find the numeric input columns
            var numericColumns = columns.Where(c => c != classificationTarget && c.IsNumeric()).ToList();
            if(!numericColumns.Any())
                throw new ArgumentException("Table does not contain any numeric data columns");

            // copy the feature vectors
            var context = dataTable.Context;
            var feature = context.CreateMatrix<float>(dataTable.RowCount, (uint)numericColumns.Count+1);
            uint columnIndex = 1;
            foreach(var column in numericColumns)
                column.CopyTo(feature.Column(columnIndex++).Data);
            feature.Column(0).Data.Initialize(1f);

            // copy the target vector
            var target = context.CreateVector<float>(dataTable.RowCount);
            classificationTarget.CopyTo(target.Data);

            var theta = context.CreateVector<float>(feature.ColumnCount);
            theta.InitializeRandomly();

            var data = new LogisticRegressionTrainingData(context, feature.AsComputable(), target.AsComputable(), theta.AsComputable());
            return new Trainer<LogisticRegression, LogisticRegressionTrainingData>(data, _GradientDescent, _Evaluate, _GetModel);
        }

        static LogisticRegression _GetModel(LogisticRegressionTrainingData data) => data.CreateModel();

        static float _GradientDescent(ITrainer<LogisticRegression, LogisticRegressionTrainingData> trainer, ITrainingContext context)
        {
            var theta = trainer.Data.Theta;
            var data = trainer.Data;
            var lambda = context.Lambda;

            using (var d = data.Derivative(theta, lambda)) {
                d.MultiplyInPlace(context.LearningRate);
                var theta2 = theta.Subtract(d);
                theta.Dispose();
                theta = theta2;
            }

            return data.Cost(theta, context.Lambda);
        }

        static IReadOnlyList<(float Output, float Target)> _Evaluate(ITrainer<LogisticRegression, LogisticRegressionTrainingData> trainer)
        {
            var feature = trainer.Data.Feature;
            var theta = trainer.Data.Theta;
            var target = trainer.Data.Target;

            using var h0 = feature.Multiply(theta);
            using var h1 = h0.Column(0);
            using var h = h1.Sigmoid();

            return h.GetInternalArray().Zip(target.GetInternalArray(), (o, t) => (o, t)).ToList();
        }
    }
}
