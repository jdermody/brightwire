using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

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
        /// <param name="testCadence">Determines how many epochs elapse before the test data is evaluated</param>
        public static void Train(this IGraphTrainingEngine engine, int numIterations, IDataSource testData, IErrorMetric errorMetric, Action<GraphModel> onImprovement = null, int testCadence = 1)
        {
            var executionContext = new ExecutionContext(engine.LinearAlgebraProvider);
            engine.Test(testData, errorMetric, 128, percentage => Console.Write("\rTesting... ({0:P})    ", percentage));
            int count = 0;
            for (var i = 0; i < numIterations; i++) {
                engine.Train(executionContext, percentage => Console.Write("\rTraining... ({0:P})    ", percentage));
                if (++count == testCadence) {
                    if (engine.Test(testData, errorMetric, 128, percentage => Console.Write("\rTesting... ({0:P})    ", percentage)) && onImprovement != null) {
                        var bestModel = new GraphModel {
                            Graph = engine.Graph
                        };
                        if (engine.DataSource is IAdaptiveDataSource adaptiveDataSource)
                            bestModel.DataSource = adaptiveDataSource.GetModel();
                        onImprovement(bestModel);
                    }
                    count = 0;
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
            var wireList = new HashSet<Models.ExecutionGraph.Wire>();
            var existing = new HashSet<INode>();
            var data = input.SerialiseTo(existing, connectedTo, wireList);

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
            writer.Write(optimisation.GetType().AssemblyQualifiedName);
            optimisation.WriteTo(writer);
        }

        internal static IGradientDescentOptimisation CreateGradientDescentOptimisation(this GraphFactory factory, BinaryReader reader)
        {
            var updaterType = Type.GetType(reader.ReadString());
            var ret = (IGradientDescentOptimisation)FormatterServices.GetUninitializedObject(updaterType);
            ret.ReadFrom(factory, reader);
            return ret;
        }

		/// <summary>
		/// Aligns the output of sequential graph execution into an ordered list of results
		/// </summary>
		/// <param name="results">Output from sequential graph execution</param>
	    public static IReadOnlyList<FloatVector[]> OrderSequentialOutput(this IReadOnlyList<ExecutionResult> results)
	    {
		    var ret = new Dictionary<(int RowIndex, int SequenceIndex), FloatVector>();
		    foreach (var result in results) {
			    var sequenceIndex = result.MiniBatchSequence.SequenceIndex;
			    var rows = result.MiniBatchSequence.MiniBatch.Rows;
			    for (var i = 0; i < result.Output.Count; i++) {
				    var rowIndex = rows[i];
				    ret.Add((rowIndex, sequenceIndex), result.Output[i]);
			    }
		    }
		    return ret.GroupBy(d => d.Key.RowIndex)
				.Select(g => (g.Key, g.OrderBy(d => d.Key.SequenceIndex).Select(d => d.Value).ToArray()))
				.OrderBy(d => d.Item1)
				.Select(d => d.Item2)
				.ToList()
			;
	    }
	}
}
