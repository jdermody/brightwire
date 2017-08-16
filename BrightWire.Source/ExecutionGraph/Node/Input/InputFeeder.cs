using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightWire.ExecutionGraph.Node.Input
{
    class InputFeeder : NodeBase
    {
        int _index;

        public InputFeeder(int index, string name = null) : base(name)
        {
            _index = index;
        }

        public override void ExecuteForward(IContext context)
        {
            var input = context.BatchSequence.Input[_index];
            _AddNextGraphAction(context, input, null);
        }

        protected override (string Description, byte[] Data) _GetInfo()
        {
            return ("INPUT", _WriteData(WriteTo));
        }

        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(_index);
        }

        public override void ReadFrom(GraphFactory factory, BinaryReader reader)
        {
            _index = reader.ReadInt32();
        }
    }
}
