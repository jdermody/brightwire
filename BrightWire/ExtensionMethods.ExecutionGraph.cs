using BrightWire.ExecutionGraph;
using BrightWire.ExecutionGraph.Engine.Helper;
using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrightData;
using BrightData.Helper;
using BrightData.LinearAlgebra;
using BrightWire.ExecutionGraph.Node;
using BrightWire.Helper;
using BrightData.DataTable.Rows;

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
        public static async Task<GraphModel?> Train(this IGraphTrainingEngine engine, uint numIterations, IDataSource testData, Action<GraphModel>? onImprovement = null, int testCadence = 1)
        {
            var executionContext = new GraphExecutionContext(engine);
            var userNotifications = engine.Context.UserNotifications;

#if !DEBUG
            var testId = Guid.NewGuid();
            userNotifications?.OnStartOperation(testId);
            await engine.Test(testData, 128, percentage => userNotifications?.OnOperationProgress(testId, percentage));
            userNotifications?.OnCompleteOperation(testId, false);
#endif

            var count = 0;
            GraphModel? ret = null;
            for (var i = 0; i < numIterations; i++) {
                var id = Guid.NewGuid();
                userNotifications?.OnStartOperation(id);
                await engine.Train(executionContext, percentage => userNotifications?.OnOperationProgress(id, percentage));
                if (++count == testCadence) {
                    userNotifications?.OnStartOperation(id);
                    if (await engine.Test(testData, 128, percentage => userNotifications?.OnOperationProgress(id, percentage)) && onImprovement != null) {
                        ret = new GraphModel {
                            Graph = engine.Graph
                        };
                        onImprovement(ret);
                    }
                    count = 0;
                }
                userNotifications?.OnCompleteOperation(id, false);
            }

            return ret;
        }

        /// <summary>
        /// Classifies each row of the data table
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="dataTable"></param>
        /// <returns>A list of rows with their corresponding classifications</returns>
        public static IEnumerable<(GenericTableRow Row, (string Label, float Weight)[] Classification)> Classify(this IRowClassifier classifier, IDataTable dataTable)
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
                OtherNodes = [.. connectedTo],
                Wires = [.. wireList]
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
                nodeTable.TryAdd(n.Id, n);
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
	    public static async Task<IReadOnlyVector<float>[][]> OrderSequentialOutput(this IAsyncEnumerable<ExecutionResult> results)
	    {
		    var ret = new Dictionary<(uint RowIndex, uint SequenceIndex), IReadOnlyVector<float>>();
		    await foreach (var result in results) {
			    var sequenceIndex = result.MiniBatchSequence.SequenceIndex;
			    var rows = result.MiniBatchSequence.MiniBatch.Rows;
                var outputRows = result.Output;
			    for (var i = 0; i < outputRows.Length; i++) {
				    var rowIndex = rows[i];
				    ret.Add((rowIndex, sequenceIndex), outputRows[i]);
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
        /// Aligns the output of sequential graph execution into an ordered list of results
        /// </summary>
        /// <param name="results">Output from sequential graph execution</param>
        public static IReadOnlyVector<float>[][] OrderSequentialOutput(this IEnumerable<ExecutionResult> results)
        {
            var ret = new Dictionary<(uint RowIndex, uint SequenceIndex), IReadOnlyVector<float>>();
            foreach (var result in results) {
                var sequenceIndex = result.MiniBatchSequence.SequenceIndex;
                var rows = result.MiniBatchSequence.MiniBatch.Rows;
                var outputRows = result.Output;
                for (var i = 0; i < outputRows.Length; i++) {
                    var rowIndex = rows[i];
                    ret.Add((rowIndex, sequenceIndex), outputRows[i]);
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
        public static IGraphData AsGraphData(this IMatrix<float> matrix)
        {
            return new MatrixGraphData(matrix);
        }

        /// <summary>
        /// Converts the 3D tensor to a generic IGraphData
        /// </summary>
        /// <param name="tensor">Tensor to convert</param>
        public static IGraphData AsGraphData(this ITensor3D<float> tensor)
        {
            return new Tensor3DGraphData(tensor);
        }

        /// <summary>
        /// Converts the 4D tensor to a generic IGraphData
        /// </summary>
        /// <param name="tensor">Tensor to convert</param>
        public static IGraphData AsGraphData(this ITensor4D<float> tensor)
        {
            return new Tensor4DGraphData(tensor);
        }

        /// <summary>
        /// Finds the graph sequence contexts that have been executed in this mini batch
        /// </summary>
        /// <param name="miniBatch"></param>
        /// <returns></returns>
        public static IEnumerable<IGraphContext> GetGraphContexts(this MiniBatch miniBatch)
        {
            for (uint i = 0, len = miniBatch.SequenceCount; i < len; i++) {
                var context = miniBatch.GetSequenceAtIndex(i).GraphContext;
                if (context != null)
                    yield return context;
            }
        }

        /// <summary>
        /// Returns the linear algebra provider associated from this graph context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LinearAlgebraProvider<float> GetLinearAlgebraProvider(this IGraphContext context) => context.ExecutionContext.LinearAlgebraProvider;
    }
}
