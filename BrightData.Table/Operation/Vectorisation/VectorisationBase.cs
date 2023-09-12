using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Table.Operation.Vectorisation
{
    internal abstract class VectorisationBase<T> : ICanVectorise where T: notnull
    {
        readonly IReadOnlyBuffer<T> _buffer;
        readonly float[] _output;

        protected VectorisationBase(IReadOnlyBuffer<T> buffer, uint outputSize)
        {
            _buffer = buffer;
            _output = new float[outputSize];
        }

        public uint OutputSize => (uint)_output.Length;

        protected abstract void Vectorise(in T item, Span<float> buffer);

        public async Task WriteBlock(uint blockIndex, uint offset, float[,] output)
        {
            var block = await _buffer.GetTypedBlock(blockIndex);
            WriteBlock(block.Span, offset, output.AsSpan2D());
        }

        void WriteBlock(ReadOnlySpan<T> block, uint offset, Span2D<float> output)
        {
            var rowIndex = 0;
            foreach (ref readonly var item in block)
                Vectorise(item).CopyTo(output.GetRowSpan(rowIndex++)[(int)offset..]);
        }

        public ReadOnlySpan<float> Vectorise(in T item)
        {
            var span = _output.AsSpan();
            span.Clear();
            Vectorise(item, span);
            return _output;
        }

        public virtual void ReadFrom(MetaData metaData)
        {
            // default is nop
        }

        public virtual void WriteTo(MetaData metaData)
        {
            // default is nop
        }
    }
}
