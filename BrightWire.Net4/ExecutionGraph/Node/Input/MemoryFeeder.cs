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
            string _id;

            public SetMemory(string id)
            {
                _id = id;
            }

            public void Initialise(string data)
            {
                _id = data;
            }

            public string Serialise()
            {
                return _id;
            }

            public void Execute(IMatrix input, IContext context)
            {
                context.ExecutionContext.SetMemory(_id, input);
            }
        }

        readonly float[] _data;
        SetMemory _setMemory;

        public MemoryFeeder(float[] data, string name = null) : base(name)
        {
            _data = data;
            _setMemory = new SetMemory(Id);
        }

        public IAction SetMemoryAction => _setMemory;
        public FloatVector Data
        {
            get { return new FloatVector { Data = _data }; }
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

            _AddNextGraphAction(context, new MatrixGraphData(memory), () => new Backpropagation(this));
        }

        void _OnNext(IContext context)
        {
            var memory = context.ExecutionContext.GetMemory(Id);

            context.LearningContext?.Log(writer => {
                writer.WriteStartElement("read-memory");
                writer.WriteRaw(memory.AsIndexable().AsXml);
                writer.WriteEndElement();
            });

            _AddNextGraphAction(context, new MatrixGraphData(memory), null);
        }

        public IMatrix GetMemory(ILinearAlgebraProvider lap, int batchSize) => lap.Create(batchSize, _data.Length, (x, y) => _data[y]);

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("MF", _WriteData(WriteTo));
        }

        protected override void _Initalise(GraphFactory factory, string description, byte[] data)
        {
            _ReadFrom(data, reader => ReadFrom(factory, reader));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            Data.WriteTo(writer);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            Data = FloatVector.ReadFrom(reader);
        }
    }
}
