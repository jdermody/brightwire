using BrightData;
using BrightTable;
using System;
using System.Linq;
using BrightData.Helper;

namespace BrightWire.Learning
{
    public static class LogisticRegression
    {
        public static void TrainLogisticRegression(this IDataTable dataTable)
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
            var target = context.CreateVector<float>(dataTable.ColumnCount);
            classificationTarget.CopyTo(target.Data);
        }

        static IVector<float> _Derivative(IBrightDataContext context, IMatrix<float> feature, IVector<float> th, IVector<float> target, float lambda)
        {
            using var p0 = feature.Multiply(th);
            using var p1 = p0.Column(0);
            using var p = p1.Sigmoid();
            using var e0 = p.Subtract(target);
            using var e = e0.Reshape(1, e0.Size);
            using var e2 = e.Multiply(feature);

            e2.MultiplyInPlace(1f / feature.RowCount);
            var ret = e2.Row(0);
            if (FloatMath.IsNotZero(lambda)) {
                var reg = new float[th.Size];
                using var thi = th.AsIndexable();
                var term = lambda / feature.RowCount;
                for (var i = 1; i < th.Size; i++) {
                    reg[i] = thi[i] * term;
                }

                using var regVector = context.CreateVector(reg);
                ret.AddInPlace(regVector);
            }
            return ret;
        }

        static float ComputeCost(IMatrix<float> feature, IVector<float> th, IVector<float> target, float lambda)
        {
            using var h0 = feature.Multiply(th);
            using var h1 = h0.Column(0);
            using var h = h1.Sigmoid();
            using var hLog = h.Log();
            using var t = target.Clone();
            var a = target.DotProduct(hLog);

            t.MultiplyInPlace(-1f);
            t.AddInPlace(1f);

            h.MultiplyInPlace(-1f);
            h.AddInPlace(1f);

            var b = t.DotProduct(hLog);
            var ret = -(a + b) / feature.RowCount;
            if (FloatMath.IsNotZero(lambda))
                ret += th.AsIndexable().Values.Skip(1).Select(v => v * v).Sum() * lambda / (2 * feature.RowCount);
            return ret;
        }
    }
}
