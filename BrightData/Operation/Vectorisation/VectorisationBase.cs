using System;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace BrightData.Operation.Vectorisation
{
    internal abstract class VectorisationBase<T> : ICanVectorise where T: notnull
    {
        readonly float[] _output;

        protected VectorisationBase(uint outputSize)
        {
            _output = new float[outputSize];
        }

        public uint OutputSize => (uint)_output.Length;

        protected abstract void Vectorise(in T item, Span<float> buffer);
        public abstract VectorisationType Type { get; }

        public async Task WriteBlock(IReadOnlyBuffer buffer, uint blockIndex, uint offset, float[,] output)
        {
            if (buffer is not IReadOnlyBuffer<T> typedBuffer)
                throw new Exception($"Expected read only buffer of {typeof(T)}");
            var block = await typedBuffer.GetTypedBlock(blockIndex);
            WriteBlock(block.Span, offset, output.AsSpan2D());
        }

        public void Vectorise(object obj, Span<float> output) => Vectorise((T)obj, output);

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
            var type = (VectorisationType)metaData.Get<byte>(Consts.VectorisationType, (byte)VectorisationType.Unknown);
            if (type != VectorisationType.Unknown && type != Type)
                throw new Exception($"Previously created vectorisation differs from current vectorisation type (previous: {type}, current: {Type})");
        }

        public virtual void WriteTo(MetaData metaData)
        {
            metaData.Set(Consts.VectorisationType, (byte)Type);
            metaData.Set(Consts.VectorisationSize, OutputSize);
        }
    }
}
