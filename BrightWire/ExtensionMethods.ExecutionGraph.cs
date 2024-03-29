﻿using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData;
using BrightData.Helper;
using BrightWire.ExecutionGraph.Node;
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
        /// <param name="onImprovement">Optional callback for when the test data score has improved against the error metric</param>
        /// <param name="testCadence">Determines how many epochs elapse before the test data is evaluated</param>
        public static GraphModel? Train(this IGraphTrainingEngine engine, uint numIterations, IDataSource testData, Action<GraphModel>? onImprovement = null, int testCadence = 1)
        {
            var executionContext = new ExecutionContext(engine.Context, engine.LinearAlgebraProvider, engine);
            var userNotifications = engine.LinearAlgebraProvider.Context.UserNotifications;
            // ReSharper disable once AccessToModifiedClosure
            engine.Test(testData, 128, percentage => userNotifications?.OnOperationProgress(percentage));

            var count = 0;
            GraphModel? ret = null;
            for (var i = 0; i < numIterations; i++) {
                userNotifications?.OnStartOperation();
                engine.Train(executionContext, percentage => userNotifications?.OnOperationProgress(percentage));
                if (++count == testCadence) {
                    userNotifications?.OnStartOperation();
                    if (engine.Test(testData, 128, percentage => userNotifications?.OnOperationProgress(percentage)) && onImprovement != null) {
                        ret = new GraphModel {
                            Graph = engine.Graph
                        };
                        onImprovement(ret);
                    }
                    count = 0;
                }
                userNotifications?.OnCompleteOperation();
            }

            return ret;
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
        public static ExecutionGraphModel GetGraph(this NodeBase input, string? name = null)
        {
            var connectedTo = new List<ExecutionGraphModel.Node>();
            var wireList = new HashSet<ExecutionGraphModel.Wire>();
            var existing = new HashSet<NodeBase>();
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
        public static NodeBase CreateFrom(this GraphFactory factory, ExecutionGraphModel graph)
        {
            // create the input node
            var nodeTable = new Dictionary<string, NodeBase>();
            var ret = factory.Create(graph.InputNode);
            nodeTable.Add(ret.Id, ret);

            // create the other nodes
            foreach (var node in graph.OtherNodes) {
                var n = factory.Create(node);
                if (!nodeTable.ContainsKey(n.Id))
                    nodeTable.Add(n.Id, n);
            }

            // let each node know it has been deserialised and can access the entire graph
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
	    public static float[][][] OrderSequentialOutput(this IEnumerable<ExecutionResult> results)
	    {
		    var ret = new Dictionary<(uint RowIndex, uint SequenceIndex), float[]>();
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

        /// <summary>
        /// Finds the graph sequence contexts that have been executed in this mini batch
        /// </summary>
        /// <param name="miniBatch"></param>
        /// <returns></returns>
        public static IEnumerable<IGraphSequenceContext> GetGraphContexts(this IMiniBatch miniBatch)
        {
            for (uint i = 0, len = miniBatch.SequenceCount; i < len; i++) {
                var context = miniBatch.GetSequenceAtIndex(i).GraphContext;
                if (context != null)
                    yield return context;
            }
        }
    }
}
