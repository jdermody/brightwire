using BrightWire.ExecutionGraph.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Input
{
    internal class MemoryFeeder
    {
        class Backpropagation : BackpropagationBase
        {
            readonly MemoryFeeder _feeder;

            public Backpropagation(MemoryFeeder feeder)
            {
                _feeder = feeder;
            }

            public override void Backward(IMatrix errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                context.LearningContext.Log(writer => {
                    writer.WriteStartElement("memory-backpropagation");
                    if (context.LearningContext.LogMatrixValues)
                        writer.WriteRaw(errorSignal.AsIndexable().AsXml);
                    writer.WriteEndElement();
                });

                using (var columnSums = errorSignal.ColumnSums()) {
                    var initialDelta = columnSums.AsIndexable();
                    for (var j = 0; j < _feeder._data.Length; j++)
                        _feeder._data[j] += initialDelta[j] * context.LearningContext.LearningRate;
                }
            }
        }
        class SetMemory : IAction
        {
            readonly MemoryFeeder _feeder;

            public SetMemory(MemoryFeeder feeder)
            {
                _feeder = feeder;
            }

            public void Execute(IMatrix input, IContext context)
            {
                context.ExecutionContext.SetMemory(_feeder._id, input);
            }
        }

        readonly float[] _data;
        readonly ILinearAlgebraProvider _lap;
        readonly string _id;
        readonly SetMemory _setMemory;
        readonly FlowThrough _memoryInput;

        public MemoryFeeder(string id, ILinearAlgebraProvider lap, float[] data)
        {
            _lap = lap;
            _data = data;
            _id = id;
            _setMemory = new SetMemory(this);
            _memoryInput = new FlowThrough();
        }

        public INode MemoryInput => _memoryInput;
        public IAction SetMemoryAction => _setMemory;

        public void OnStart(IContext context)
        {
            var memory = GetMemory(context.BatchSequence.MiniBatch.BatchSize);

            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("init-memory");
                writer.WriteRaw(memory.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            context.Forward(new GraphAction(_memoryInput, new MatrixGraphData(memory)), () => new Backpropagation(this));
        }

        public void OnNext(IContext context)
        {
            var memory = context.ExecutionContext.GetMemory(_id);

            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("read-memory");
                writer.WriteRaw(memory.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            context.Forward(new GraphAction(_memoryInput, new MatrixGraphData(memory)), null);
        }

        public IMatrix GetMemory(int batchSize) => _lap.Create(batchSize, _data.Length, (x, y) => _data[y]);
    }
}
