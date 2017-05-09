using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BrightWire.ExecutionGraph.Node.Input
{
    internal class MemoryFeeder : NodeBase
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
                context.ExecutionContext.SetMemory(_feeder.Id, input);
            }
        }

        readonly float[] _data;
        readonly SetMemory _setMemory;
        readonly FlowThrough _memoryInput;

        public MemoryFeeder(float[] data, string name = null) : base(name)
        {
            _data = data;
            _setMemory = new SetMemory(this);
            _memoryInput = new FlowThrough();
        }

        public INode MemoryInput => _memoryInput;
        public IAction SetMemoryAction => _setMemory;
        public FloatArray Data
        {
            get { return new FloatArray { Data = _data }; }
            set { value.Data.CopyTo(_data, 0); }
        }

        public override void ExecuteForward(IContext context)
        {
            if (context.BatchSequence.SequenceIndex == 1)
                _OnStart(context);
            else
                _OnNext(context);
        }

        void _OnStart(IContext context)
        {
            var memory = GetMemory(context.LinearAlgebraProvider, context.BatchSequence.MiniBatch.BatchSize);

            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("init-memory");
                writer.WriteRaw(memory.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            context.Forward(new GraphAction(_memoryInput, new MatrixGraphData(memory)), () => new Backpropagation(this));
        }

        void _OnNext(IContext context)
        {
            var memory = context.ExecutionContext.GetMemory(Id);

            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("read-memory");
                writer.WriteRaw(memory.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            context.Forward(new GraphAction(_memoryInput, new MatrixGraphData(memory)), null);
        }

        public IMatrix GetMemory(ILinearAlgebraProvider lap, int batchSize) => lap.Create(batchSize, _data.Length, (x, y) => _data[y]);

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("MF", _WriteData(WriteTo));
        }

        protected override void _Initalise(byte[] data)
        {
            _ReadFrom(data, ReadFrom);
        }

        public override void WriteTo(BinaryWriter writer)
        {
            Data.WriteTo(writer);
        }

        public override void ReadFrom(BinaryReader reader)
        {
            Data = FloatArray.ReadFrom(reader);
        }
    }
}
