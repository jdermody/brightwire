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
            public IMatrix<float> Feature { get; }
            public IVector<float> Target { get; }

            public LogisticRegressionTrainingData(IBrightDataContext context, IMatrix<float> feature, IVector<float> target)
            {
                _context = context;
                Feature = feature;
                Target = target;
            }

            public void Dispose()
            {
                Feature.Dispose();
                Target.Dispose();
            }

            public float Cost(IVector<float> theta, float lambda)
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
                    ret += theta.AsIndexable().Values.Skip(1).Select(v => v * v).Sum() * lambda / (2 * Feature.RowCount);
                return ret;
            }

            public IVector<float> Derivative(IVector<float> theta, float lambda)
            {
                using var p0 = Feature.Multiply(theta);
                using var p1 = p0.Column(0);
                using var p = p1.Sigmoid();
                using var e0 = p.Subtract(Target);
                using var e = e0.Reshape(1, e0.Size);
                using var e2 = e.Multiply(Feature);

                e2.MultiplyInPlace(1f / Feature.RowCount);
                var ret = e2.Row(0);
                if (FloatMath.IsNotZero(lambda)) {
                    var size = theta.Size;
                    var reg = new float[size];
                    using var thi = theta.AsIndexable();
                    var term = lambda / Feature.RowCount;
                    for (var i = 1; i < size; i++) {
                        reg[i] = thi[i] * term;
                    }

                    using var regVector = _context.CreateVector(reg);
                    ret.AddInPlace(regVector);
                }
                return ret;
            }
        }

        public static ITrainer<LogisticRegression, LogisticRegressionTrainingData> GetTrainer(IDataTable dataTable)
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
                column.CopyTo(feature.Column(columnIndex++));
            feature.Column(0).Initialize(1f);

            // copy the target vector
            var target = context.CreateVector<float>(dataTable.RowCount);
            classificationTarget.CopyTo(target.Data);

            var data = new LogisticRegressionTrainingData(context, feature, target);
            var model = new LogisticRegression(context.CreateVector<float>(feature.ColumnCount));
            return new Trainer<LogisticRegression, LogisticRegressionTrainingData>(model, data, _GradientDescent, _Evaluate);
        }

        static float _GradientDescent(ITrainer<LogisticRegression, LogisticRegressionTrainingData> trainer, ITrainingContext context)
        {
            var theta = trainer.Model.Theta;
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
            var theta = trainer.Model.Theta;
            var target = trainer.Data.Target.AsIndexable();

            using var h0 = feature.Multiply(theta);
            using var h1 = h0.Column(0);
            using var h = h1.Sigmoid();
            using var h2 = h.AsIndexable();

            return h2.Values.Zip(target.Values, (o, t) => (o, t)).ToList();
        }
    }
}
