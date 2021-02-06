using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightWire.Helper;

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
        public static void Train(this IGraphTrainingEngine engine, uint numIterations, IDataSource testData, IErrorMetric errorMetric, Action<GraphModel>? onImprovement = null, int testCadence = 1)
        {
            var executionContext = new ExecutionContext(engine.LinearAlgebraProvider);
            var progress = -1;
            var sw = Stopwatch.StartNew();
            // ReSharper disable once AccessToModifiedClosure
            engine.Test(testData, errorMetric, 128, percentage => percentage.WriteProgressPercentage(ref progress, sw));

            var count = 0;
            for (var i = 0; i < numIterations; i++) {
                progress = -1;
                sw.Restart();
                engine.Train(executionContext, percentage => percentage.WriteProgressPercentage(ref progress, sw));
                if (++count == testCadence) {
                    progress = -1;
                    sw.Restart();
                    if (engine.Test(testData, errorMetric, 128, percentage => percentage.WriteProgressPercentage(ref progress, sw)) && onImprovement != null) {
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

        static void WriteProgressPercentage(this float progress, ref int previousPercentage, Stopwatch sw)
        {
            var curr = Convert.ToInt32(progress * 100);
            if (curr > previousPercentage) {
                var sb = new StringBuilder();
                sb.Append($"\r");
                var i = 0;
                for (; i < curr; i++)
                    sb.Append('█');
                var fore = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(sb.ToString());

                sb.Clear();
                for (; i < 100; i++)
                    sb.Append('█');
                sb.Append($" {progress:P0}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(sb.ToString());

                Console.ForegroundColor = fore;
                Console.Write($" {sw.Elapsed.Minutes:00}:{sw.Elapsed.Seconds:00}:{sw.Elapsed.Milliseconds:0000}");

                previousPercentage = curr;
            }
        }

        /// <summary>
        /// Classifies each row of the data table
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="dataTable"></param>
        /// <returns>A list of rows with their corresponding classifications</returns>
        public static IEnumerable<(IConvertibleRow Row, (string Label, float Weight)[] Classification)> Classifiy(this IRowClassifier classifier, IRowOrientedDataTable dataTable)
        {
            return dataTable.Classify(classifier);
        }

        /// <summary>
        /// Serialises the node and any other connected nodes to an execution graph
        /// </summary>
        /// <param name="input"></param>
        /// <param name="name">Name of the graph (optional)</param>
        /// <returns></returns>
        public static ExecutionGraphModel GetGraph(this INode input, string? name = null)
        {
            var connectedTo = new List<ExecutionGraphModel.Node>();
            var wireList = new HashSet<ExecutionGraphModel.Wire>();
            var existing = new HashSet<INode>();
            var data = input.SerialiseTo(existing, connectedTo, wireList);

            return new ExecutionGraphModel {
                Name = name!,
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
        public static INode CreateFrom(this GraphFactory factory, ExecutionGraphModel graph)
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
            writer.Write(TypeLoader.GetTypeName(optimisation));
            optimisation.WriteTo(writer);
        }

        internal static IGradientDescentOptimisation CreateGradientDescentOptimisation(this GraphFactory factory, BinaryReader reader)
        {
            var updaterType = TypeLoader.LoadType(reader.ReadString());
            var ret = GenericActivator.CreateUninitialized<IGradientDescentOptimisation>(updaterType);
            ret.ReadFrom(factory, reader);
            return ret;
        }

		/// <summary>
		/// Aligns the output of sequential graph execution into an ordered list of results
		/// </summary>
		/// <param name="results">Output from sequential graph execution</param>
	    public static Vector<float>[][] OrderSequentialOutput(this IEnumerable<ExecutionResult> results)
	    {
		    var ret = new Dictionary<(uint RowIndex, uint SequenceIndex), Vector<float>>();
		    foreach (var result in results) {
			    var sequenceIndex = result.MiniBatchSequence.SequenceIndex;
			    var rows = result.MiniBatchSequence.MiniBatch.Rows;
			    for (var i = 0; i < result.Output.Length; i++) {
				    var rowIndex = rows[i];
				    ret.Add((rowIndex, sequenceIndex), result.Output[i]);
			    }
		    }
		    return ret.GroupBy(d => d.Key.RowIndex)
				.Select(g => (g.Key, g.OrderBy(d => d.Key.SequenceIndex).Select(d => d.Value).ToArray()))
				.OrderBy(d => d.Key)
				.Select(d => d.Item2)
				.ToArray()
			;
	    }

        /// <summary>
        /// Converts the matrix to a generic IGraphData
        /// </summary>
        /// <param name="matrix">Matrix to convert</param>
        public static IGraphData AsGraphData(this IFloatMatrix matrix)
        {
            return new MatrixGraphData(matrix);
        }

        /// <summary>
        /// Converts the 3D tensor to a generic IGraphData
        /// </summary>
        /// <param name="tensor">Tensor to convert</param>
        public static IGraphData AsGraphData(this I3DFloatTensor tensor)
        {
            return new Tensor3DGraphData(tensor);
        }

        /// <summary>
        /// Converts the 4D tensor to a generic IGraphData
        /// </summary>
        /// <param name="tensor">Tensor to convert</param>
        public static IGraphData AsGraphData(this I4DFloatTensor tensor)
        {
            return new Tensor4DGraphData(tensor);
        }
    }
}
