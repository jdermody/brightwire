using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BrightWire
{
    /// <summary>
    /// Static extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Shuffles the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq">The sequence to shuffle</param>
        /// <param name="randomSeed">The random seed to use or null for a random shuffle</param>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return Shuffle(seq, rnd);
        }

        /// <summary>
        /// Shuffles the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq">The sequence to shuffle</param>
        /// <param name="rnd">The random number generator to use</param>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> seq, Random rnd)
        {
            return seq.OrderBy(e => rnd.Next()).ToList();
        }

        /// <summary>
        /// Splits a sequence into training and test splits
        /// </summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="seq">The sequence to split</param>
        /// <param name="trainPercentage">The percentage of the sequence to add to the training set</param>
        public static (IReadOnlyList<T> Training, IReadOnlyList<T> Test) Split<T>(this IReadOnlyList<T> seq, double trainPercentage = 0.8)
        {
            var input = Enumerable.Range(0, seq.Count).ToList();
            int trainingCount = Convert.ToInt32(seq.Count * trainPercentage);
            return (
                input.Take(trainingCount).Select(i => seq[i]).ToArray(),
                input.Skip(trainingCount).Select(i => seq[i]).ToArray()
            );
        }

        /// <summary>
        /// Bags (select with replacement) the input sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The input sequence</param>
        /// <param name="count">The size of the output sequence</param>
        /// <param name="randomSeed">The random seed or null initialise randomlu</param>
        /// <returns></returns>
        public static IReadOnlyList<T> Bag<T>(this IReadOnlyList<T> list, int count, int? randomSeed = null)
        {
            var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
            return Enumerable.Range(0, count)
                .Select(i => list[rnd.Next(0, list.Count)])
                .ToList()
            ;
        }

        /// <summary>
        /// Returns the underlying .net type associated with the column type
        /// </summary>
        /// <param name="type">The column type</param>
        public static Type GetColumnType(this ColumnType type)
        {
            switch (type) {
                case ColumnType.Boolean:
                    return typeof(bool);

                case ColumnType.Byte:
                    return typeof(byte);

                case ColumnType.Date:
                    return typeof(DateTime);

                case ColumnType.Double:
                    return typeof(double);

                case ColumnType.Float:
                    return typeof(float);

                case ColumnType.Int:
                    return typeof(int);

                case ColumnType.Long:
                    return typeof(long);

                case ColumnType.Null:
                    return null;

                case ColumnType.String:
                    return typeof(string);

                case ColumnType.IndexList:
                    return typeof(IndexList);

                case ColumnType.WeightedIndexList:
                    return typeof(WeightedIndexList);

                case ColumnType.Vector:
                    return typeof(FloatVector);

                case ColumnType.Matrix:
                    return typeof(FloatMatrix);

                case ColumnType.Tensor:
                    return typeof(FloatTensor);

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Random projections allow you to reduce the dimensions of a matrix while still preserving significant information
        /// </summary>
        /// <param name="lap">Linear algebra provider</param>
        /// <param name="fixedSize">The vector size to reduce from</param>
        /// <param name="reducedSize">The vector size to reduce to</param>
        /// <param name="s"></param>
        public static IRandomProjection CreateRandomProjection(this ILinearAlgebraProvider lap, int fixedSize, int reducedSize, int s = 3)
        {
            return new RandomProjection(lap, fixedSize, reducedSize, s);
        }

        //public static IMatrix CreateMatrix(this ILinearAlgebraProvider lap, IReadOnlyList<float> vector, int width)
        //{
        //    var height = vector.Count / width;
        //    return lap.Create(width, height, (i, j) => vector[(i * width) + j]);
        //}

        public static I3DTensor ConvertToTensor(this IVector vector, ILinearAlgebraProvider lap, int rows, int columns, int depth)
        {
            if (depth > 1) {
                var matrixList = new List<IMatrix>();
                var slice = vector.Split(depth);
                foreach (var part in slice)
                    matrixList.Add(part.ConvertInPlaceToMatrix(rows, columns));
                var ret = lap.CreateTensor(matrixList);
                foreach (var item in matrixList)
                    item.Dispose();
                return ret;
            } else {
                var matrix = vector.ConvertInPlaceToMatrix(rows, columns);
                return lap.CreateTensor(new[] { matrix });
            }
        }

        public static IGraphData ToGraphData(this IMatrix matrix)
        {
            return new MatrixGraphData(matrix);
        }

        public static IGraphData ToGraphData(this I3DTensor tensor)
        {
            return new TensorGraphData(tensor);
        }

        public static IGraphData ToGraphData(this IReadOnlyList<IMatrix> matrixList, ILinearAlgebraProvider lap)
        {
            if (matrixList.Count == 1)
                return matrixList[0].ToGraphData();
            else if (matrixList.Count > 1)
                return lap.CreateTensor(matrixList).ToGraphData();
            return null;
        }

        public static IReadOnlyList<IMatrix> Decompose(this IGraphData data)
        {
            var ret = new List<IMatrix>();
            if (data.DataType == GraphDataType.Matrix)
                ret.Add(data.GetMatrix());
            else {
                var tensor = data.GetTensor();
                for (var i = 0; i < tensor.Depth; i++)
                    ret.Add(tensor.GetDepthSlice(i));
            }
            return ret;
        }

        public static IGraphData ToGraphData(this IContext context, IEnumerable<IMatrix> matrixList)
        {
            return matrixList.ToList().ToGraphData(context.LinearAlgebraProvider);
        }

        public static Models.ExecutionGraph GetGraph(this INode input, string name = null)
        {
            var connectedTo = new List<Models.ExecutionGraph.Node>();
            var wireList = new List<Models.ExecutionGraph.Wire>();
            var data = input.SerialiseTo(connectedTo, wireList);

            return new Models.ExecutionGraph {
                Name = name,
                InputNode = data,
                OtherNodes = connectedTo.ToArray(),
                Wires = wireList.ToArray()
            };
        }

        public static INode CreateFrom(this GraphFactory factory, Models.ExecutionGraph graph)
        {
            // create the input node
            var nodeTable = new Dictionary<string, INode>();
            var ret = factory.Create(graph.InputNode);
            nodeTable.Add(ret.Id, ret);

            // create the other nodes
            foreach (var node in graph.OtherNodes) {
                var n = factory.Create(node);
                if (!nodeTable.ContainsKey(n.Id))
                    nodeTable.Add(n.Id, n);
            }

            // create the wires between nodes
            foreach (var wire in graph.Wires) {
                var from = nodeTable[wire.FromId];
                var to = nodeTable[wire.ToId];
                from.Output.Add(new WireToNode(to, wire.InputChannel));
            }
            return ret;
        }

        internal static void Write(this BinaryWriter writer, IGradientDescentOptimisation optimisation)
        {
            writer.Write(optimisation.GetType().FullName);
            optimisation.WriteTo(writer);
        }

        internal static IGradientDescentOptimisation CreateGradientDescentOptimisation(this GraphFactory factory, BinaryReader reader)
        {
            var updaterType = Type.GetType(reader.ReadString());
            var ret = (IGradientDescentOptimisation)FormatterServices.GetUninitializedObject(updaterType);
            ret.ReadFrom(factory, reader);
            return ret;
        }
    }
}
