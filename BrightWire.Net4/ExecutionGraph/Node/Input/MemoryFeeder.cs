using BrightWire.ExecutionGraph.Helper;
using BrightWire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BrightWire.ExecutionGraph.Action;

namespace BrightWire.ExecutionGraph.Node.Input
{
    internal class MemoryFeeder : NodeBase
    {
        class Backpropagation : BackpropagationBase<MemoryFeeder>
        {
            public Backpropagation(MemoryFeeder source) : base(source)
            {
            }

            public override void _Backward(INode fromNode, IGraphData errorSignal, IContext context, IReadOnlyList<INode> parents)
            {
                if (context.BatchSequence.Type == MiniBatchType.SequenceStart) {
                    var es = errorSignal.GetMatrix();

                    context.LearningContext.Log(writer => {
                        writer.WriteStartElement("memory-backpropagation");
                        if (context.LearningContext.LogMatrixValues)
                            writer.WriteRaw(es.AsIndexable().AsXml);
                        writer.WriteEndElement();
                    });

                    using (var columnSums = es.ColumnSums()) {
                        var initialDelta = columnSums.AsIndexable();
                        for (var j = 0; j < _source._data.Length; j++)
                            _source._data[j] += initialDelta[j] * context.LearningContext.LearningRate;
                    }
                }
            }
        }

        readonly float[] _data;
        SetMemory _setMemory;

        public MemoryFeeder(float[] data, string name = null, string id = null) : base(name, id)
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
            if (context.BatchSequence.SequenceIndex == 0)
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

        public IMatrix GetMemory(ILinearAlgebraProvider lap, int batchSize) => lap.CreateMatrix(batchSize, _data.Length, (x, y) => _data[y]);

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
