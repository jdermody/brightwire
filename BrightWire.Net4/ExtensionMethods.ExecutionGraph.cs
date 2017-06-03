using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Engine;
using BrightWire.ExecutionGraph.Engine.Helper;
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
            var executionContext = new ExecutionContext(engine.LinearAlgebraProvider);
            engine.Test(testData, errorMetric);
            Console.Write("\r({0:P}) ", 0f);
            for (var i = 0; i < numIterations; i++) {
                engine.Train(executionContext, percentage => Console.Write("\r({0:P}) ", percentage));
                if (engine.Test(testData, errorMetric) && onImprovement != null) {
                    var bestModel = new GraphModel {
                        Graph = engine.Graph
                    };
                    if (engine.DataSource is IAdaptiveDataSource adaptiveDataSource)
                        bestModel.DataSource = adaptiveDataSource.GetModel();
                    onImprovement(bestModel);
                }
            }
        }

        /// <summary>
        /// Classifies each row of the data table
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="dataTable"></param>
        /// <returns>A list of rows with their corresponding classifications</returns>
        public static IReadOnlyList<(IRow Row, string Classification)> Classifiy(this IRowClassifier classifier, IDataTable dataTable)
        {
            return dataTable.Classify(classifier, percentage => Console.Write("\r({0:P}) ", percentage));
        }

        /// <summary>
        /// Serialises the node and any other connected nodes to an execution graph
        /// </summary>
        /// <param name="input"></param>
        /// <param name="name">Name of the graph (optional)</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a node and any other connected nodes from a serialised execution graph
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="graph">Serialised graph</param>
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

            // let each node know it has been deserialised and access to the entire graph
            foreach(var item in nodeTable)
                item.Value.OnDeserialise(nodeTable);

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
