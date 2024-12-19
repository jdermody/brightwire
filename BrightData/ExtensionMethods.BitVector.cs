using BrightData.Types;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using BrightData.LinearAlgebra.ReadOnly;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Returns all contiguous ranges of set bits
        /// </summary>
        public static IEnumerable<Range> GetContiguousRanges<T>(this T vector) where T: IBitVector
        {
            var inRange = false;
            var start = -1;
            for (var i = 0; i < vector.Size; i++) {
                if (vector[i]) {
                    if(inRange)
                        continue;
                    inRange = true;
                    start = i;
                }else if (inRange) {
                    yield return new Range(start, i);
                    inRange = false;
                    start = -1;
                }
            }
            if (start >= 0)
                yield return new Range(start, (int)vector.Size);
        }

        /// <summary>
        /// Counts the bits that are true
        /// </summary>
        /// <returns></returns>
        public static unsafe uint CountOfSetBits<T>(this T vector) where T: IBitVector, allows ref struct
        {
            var ret = 0;
            var span = vector.AsSpan();
            fixed (ulong* ptr = span) {
                var p = ptr;
                for(int i = 0, len = span.Length; i < len; i++)
                    ret += BitOperations.PopCount(*p++);
            }
            return (uint)ret;
        }

        /// <summary>
        /// Creates a copy of the bit vector
        /// </summary>
        /// <param name="vector"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static BitVector Clone<T>(T vector) where T : IBitVector, allows ref struct => new(vector.AsSpan(), vector.Size);

        /// <summary>
        /// Returns a new vector that has been XORed with this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector XorWith<T>(this T vector, T other) where T : IBitVector, allows ref struct => XorWith(vector, other.AsSpan());

        /// <summary>
        /// Returns a new vector that has been XORed with this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector XorWith<T>(this T vector, ReadOnlySpan<ulong> other) where T : IBitVector, allows ref struct
        {
            var ret = Clone(vector);
            Xor(ret.Data.Span, other);
            return ret;
        }

        /// <summary>
        /// Returns a new vector that is the union of this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector UnionWith<T>(this T vector, T other) where T : IBitVector, allows ref struct => UnionWith(vector, other.AsSpan());

        /// <summary>
        /// Returns a new vector that is the union of this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector UnionWith<T>(this T vector, ReadOnlySpan<ulong> other) where T : IBitVector, allows ref struct
        {
            var ret = Clone(vector);
            Or<ulong>(ret.Data.Span, other);
            return ret;
        }

        /// <summary>
        /// Returns a new vector that is the intersection of this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector IntersectionWith<T>(this T vector, T other) where T : IBitVector, allows ref struct => IntersectionWith(vector, other.AsSpan());

        /// <summary>
        /// Returns a new vector that is the intersection of this and another vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector IntersectionWith<T>(this T vector, ReadOnlySpan<ulong> other) where T : IBitVector, allows ref struct
        {
            var ret = Clone(vector);
            And<ulong>(ret.Data.Span, other);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector Except<T>(this T vector, T other) where T : IBitVector, allows ref struct => Except(vector, other.AsSpan());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BitVector Except<T>(this T vector, ReadOnlySpan<ulong> other) where T : IBitVector, allows ref struct
        {
            var ret = Clone(vector);
            Xor(ret.Data.Span, other);
            And<ulong>(ret.Data.Span, other);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static uint HammingDistance<T>(this T vector, BitVector other) where T : IBitVector, allows ref struct => HammingDistance(vector, other.AsSpan());

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vector"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static uint HammingDistance<T>(this T vector, ReadOnlySpan<ulong> other) where T : IBitVector, allows ref struct
        {
            var copy = Clone(vector);
            Xor(copy.Data.Span, other);
            return copy.CountOfSetBits();
        }

        /// <summary>
        /// Builds the vectors in the storage into a table of vectors
        /// </summary>
        /// <param name="context"></param>
        /// <param name="vectorStorage"></param>
        /// <param name="columnName">Column name of the vector column</param>
        /// <returns></returns>
        public static IBuildDataTables ToDataTableBuilder(this IStoreVectors<float> vectorStorage, BrightDataContext context, string? columnName = "vectors")
        {
            var builder = context.CreateTableBuilder();
            var column = builder.CreateFixedSizeVectorColumn(vectorStorage.VectorSize, columnName);
            vectorStorage.ForEach(x => column.Append(new ReadOnlyVector<float>(x.ToArray())));
            return builder;
        }
    }
}
