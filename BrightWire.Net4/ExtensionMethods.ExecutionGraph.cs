using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Trains a graph for a fixed number of iterations
        /// </summary>
        /// <param name="engine">The graph training engine</param>
        /// <param name="numIterations">The number of iterations to train for</param>
        /// <param name="testData">The test data source to use</param>
        /// <param name="errorMetric">The error metric to evaluate the test data against</param>
        /// <param name="onImprovement">Optional callback for when the test data score has improved against the error metric</param>
        public static void Train(this IGraphTrainingEngine engine, int numIterations, IDataSource testData, IErrorMetric errorMetric, Action<GraphModel> onImprovement = null)
        {
            engine.Test(testData, errorMetric);
            for (var i = 0; i < numIterations; i++) {
                engine.Train(percentage => Console.Write("\r({0:P}) ", percentage));
                if (engine.Test(testData, errorMetric) && onImprovement != null) {
                    var bestModel = new GraphModel {
                        Graph = engine.Graph
                    };
                    var adaptiveDataSource = engine.DataSource as IAdaptiveDataSource;
                    if (adaptiveDataSource != null)
                        bestModel.DataSource = adaptiveDataSource.GetModel();
                    onImprovement(bestModel);
                }
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
