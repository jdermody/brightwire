using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using BrightData.Helper;

namespace BrightData
{
    public static class ExtensionMethods
    {
        public static Type ToType(this TypeCode code)
        {
            switch (code) {
                case TypeCode.Boolean:
                    return typeof(bool);

                case TypeCode.Byte:
                    return typeof(byte);

                case TypeCode.Char:
                    return typeof(char);

                case TypeCode.DateTime:
                    return typeof(DateTime);

                case TypeCode.DBNull:
                    return typeof(DBNull);

                case TypeCode.Decimal:
                    return typeof(decimal);

                case TypeCode.Double:
                    return typeof(double);

                case TypeCode.Empty:
                    return null;

                case TypeCode.Int16:
                    return typeof(short);

                case TypeCode.Int32:
                    return typeof(int);

                case TypeCode.Int64:
                    return typeof(long);

                case TypeCode.Object:
                    return typeof(object);

                case TypeCode.SByte:
                    return typeof(sbyte);

                case TypeCode.Single:
                    return typeof(Single);

                case TypeCode.String:
                    return typeof(string);

                case TypeCode.UInt16:
                    return typeof(UInt16);

                case TypeCode.UInt32:
                    return typeof(UInt32);

                case TypeCode.UInt64:
                    return typeof(UInt64);
            }

            return null;
        }

        public static IndexList CreateIndexList(this IBrightDataContext context, params uint[] indices) => IndexList.Create(context, indices);
        public static WeightedIndexList CreateWeightedIndexList(this IBrightDataContext context, params (uint Index, float Weight)[] indexList) => WeightedIndexList.Create(context, indexList);

        public static Vector<T> CreateVector<T>(this IBrightDataContext context, uint size, Func<uint, T> initializer = null) where T: struct
        {
            var data = context.TensorPool.Get<T>(size);
            var segment = data.GetSegment();
            if (initializer != null)
                segment.Initialize(initializer);
            return new Vector<T>(context, segment);
        }

        public static Vector<T> CreateVector<T>(this IBrightDataContext context, uint size, T initializer = default(T)) where T: struct
        {
            var data = context.TensorPool.Get<T>(size);
            var segment = data.GetSegment();
            segment.Initialize(initializer);
            return new Vector<T>(context, segment);
        }

        public static Vector<T> CreateVector<T>(this IBrightDataContext context, params T[] data) where T: struct
        {
            return CreateVector(context, (uint)data.Length, i => data[i]);
        }

        public static Matrix<T> CreateMatrix<T>(this IBrightDataContext context, uint rows, uint columns, Func<uint, uint, T> initializer = null) where T: struct
        {
            var data = context.TensorPool.Get<T>(rows * columns);
            var segment = data.GetSegment();
            if (initializer != null)
                segment.Initialize(i => initializer(i / columns, i % columns));
            return new Matrix<T>(context, segment, rows, columns);
        }

        public static Matrix<T> CreateMatrixFromRows<T>(this IBrightDataContext context, params Vector<T>[] rows) where T: struct
        {
            var columns = rows.First().Size;
            return CreateMatrix(context, (uint) rows.Length, columns, (j, i) => rows[j][i]);
        }

        public static Matrix<T> CreateMatrixFromColumns<T>(this IBrightDataContext context, params Vector<T>[] columns) where T: struct
        {
            var rows = columns.First().Size;
            return CreateMatrix(context, rows, (uint) columns.Length, (j, i) => columns[i][j]);
        }

        public static Tensor3D<T> CreateTensor3D<T>(this IBrightDataContext context, uint depth, uint rows, uint columns) where T : struct
        {
            var data = context.TensorPool.Get<T>(depth * rows * columns);
            var segment = data.GetSegment();
            return new Tensor3D<T>(context, segment, depth, rows, columns);
        }

        public static Tensor4D<T> CreateTensor4D<T>(this IBrightDataContext context, uint count, uint depth, uint rows, uint columns) where T : struct
        {
            var data = context.TensorPool.Get<T>(count * depth * rows * columns);
            var segment = data.GetSegment();
            return new Tensor4D<T>(context, segment, count, depth, rows, columns);
        }

        public static uint GetSize(this ITensor tensor)
        {
            uint ret = 1;
            foreach(var item in tensor.Shape)
                ret *= item;
            return ret;
        }

        public static uint GetRank(this ITensor tensor) => (uint)tensor.Shape.Length;

        public static uint GetColumnCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 1 ? tensor.Shape[^1] : 0;
        }

        public static uint GetRowCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 1 ? tensor.Shape[^2] : 0;
        }

        public static uint GetDepth<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 2 ? tensor.Shape[^3] : 0;
        }

        public static uint GetCount<T>(this ITensor<T> tensor) where T : struct
        {
            return tensor.Shape.Length > 3 ? tensor.Shape[^4] : 0;
        }

        public static WeightedIndexList ToSparse(this ITensorSegment<float> segment)
        {
            return WeightedIndexList.Create(segment.Context, segment.Values
                .Select((v, i) => new WeightedIndexList.Item((uint)i, v))
                .Where(d => FloatMath.IsNotZero(d.Weight))
            );
        }

        public static bool WriteIfNotNull<T>(this IMetaData metadata, string name, T? value)
            where T : struct, IConvertible
        {
            if (value.HasValue) {
                metadata.Set(name, value.Value);
                return true;
            }
            return false;
        }

        public static bool WriteIfNotNull<T>(this IMetaData metadata, string name, T value)
            where T : class, IConvertible
        {
            if (value != null) {
                metadata.Set(name, value);
                return true;
            }
            return false;
        }

        public static bool HasConversionOperator(this Type from, Type to)
        {
            Func<Expression, UnaryExpression> bodyFunction = body => Expression.Convert(body, to);
            var inp = Expression.Parameter(from, "inp");
            try {
                // If this succeeds then we can cast 'from' type to 'to' type using implicit coercion
                Expression.Lambda(bodyFunction(inp), inp).Compile();
                return true;
            }
            catch (InvalidOperationException) {
                return false;
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return Shuffle(seq, rnd);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, Random rnd)
        {
            return seq.OrderBy(e => rnd.Next()).ToList();
        }

        public static (IReadOnlyList<T> Training, IReadOnlyList<T> Test) Split<T>(this IReadOnlyList<T> seq, double trainPercentage = 0.8)
        {
            var input = Enumerable.Range(0, seq.Count).ToList();
            int trainingCount = Convert.ToInt32(seq.Count * trainPercentage);
            return (
                input.Take(trainingCount).Select(i => seq[i]).ToArray(),
                input.Skip(trainingCount).Select(i => seq[i]).ToArray()
            );
        }

        public static T[] Bag<T>(this IReadOnlyList<T> list, uint count, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return Enumerable.Range(0, (int)count)
                .Select(i => list[rnd.Next(0, list.Count)])
                .ToArray()
            ;
        }

        public static string Name(this IMetaData metadata) => metadata.Get<string>(Consts.Name);
        public static uint Index(this IMetaData metadata) => metadata.Get<uint>(Consts.Index);
        public static bool IsNumeric(this IMetaData metadata) => metadata.Get<bool>(Consts.IsNumeric);
        public static bool IsTarget(this IMetaData metadata) => metadata.Get<bool>(Consts.IsTarget);

        public static float CosineDistance(this Vector<float> vector, Vector<float> other)
        {
            return BrightData.Distance.CosineDistance.Calculate(vector.ToArray(), other.ToArray());
        }

        public static float EuclideanDistance(this Vector<float> vector, Vector<float> other)
        {
            return BrightData.Distance.EuclideanDistance.Calculate(vector.ToArray(), other.ToArray());
        }

        public static float ManhattanDistance(this Vector<float> vector, Vector<float> other)
        {
            return BrightData.Distance.ManhattanDistance.Calculate(vector.ToArray(), other.ToArray());
        }
    }
}
