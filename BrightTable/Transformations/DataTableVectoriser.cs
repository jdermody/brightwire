using BrightData;
using BrightData.Converters;
using BrightData.Helper;
using BrightTable.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightTable.Transformations
{
    public class DataTableVectoriser : IHaveDataContext
    {
        interface IColumnVectoriser : IDisposable
        {
            IEnumerable<float> GetNext();
            IEnumerable<float> Convert(object obj);
            uint Size { get; }
        }
        abstract class VectoriserBase<T> : IColumnVectoriser
        {
            readonly IEnumerator<T> _enumerator;

            public VectoriserBase(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public abstract uint Size { get; }

            public IEnumerable<float> Convert(object obj) => _Convert((T)obj);

            public IEnumerable<float> GetNext()
            {
                if (_enumerator.MoveNext()) {
                    foreach (var item in _Convert(_enumerator.Current))
                        yield return item;
                }
            }

            protected abstract IEnumerable<float> _Convert(T obj);
        }
        class NumericVectoriser<T> : VectoriserBase<T>
            where T : struct
        {
            readonly ConvertToFloat<T> _converter = new ConvertToFloat<T>();

            public NumericVectoriser(ISingleTypeTableSegment column)
                : base(((IDataTableSegment<T>)column).EnumerateTyped().GetEnumerator())
            {
            }

            public override uint Size => 1;

            protected override IEnumerable<float> _Convert(T obj)
            {
                yield return _converter.Convert(obj);
            }
        }
        class WeightedIndexListVectoriser : VectoriserBase<WeightedIndexList>
        {
            readonly uint _maxSize;

            public WeightedIndexListVectoriser(uint maxSize, ISingleTypeTableSegment column)
                : base(((IDataTableSegment<WeightedIndexList>)column).EnumerateTyped().GetEnumerator())
            {
                _maxSize = maxSize;
            }

            public override uint Size => _maxSize;

            protected override IEnumerable<float> _Convert(WeightedIndexList obj)
            {
                var indexTable = obj.Indices.ToDictionary(d => d.Index, d => d.Weight);
                for (uint i = 0; i < _maxSize; i++)
                    yield return indexTable.TryGetValue(i, out var val) ? val : 0f;
            }
        }
        class IndexListVectoriser : VectoriserBase<IndexList>
        {
            readonly uint _maxSize;

            public IndexListVectoriser(uint maxSize, ISingleTypeTableSegment column)
                : base(((IDataTableSegment<IndexList>)column).EnumerateTyped().GetEnumerator())
            {
                _maxSize = maxSize;
            }

            public override uint Size => _maxSize;

            protected override IEnumerable<float> _Convert(IndexList obj)
            {
                var indexSet = new HashSet<uint>(obj.Indices);
                for (uint i = 0; i < _maxSize; i++)
                    yield return indexSet.Contains(i) ? 1f : 0f;
            }
        }
        class TensorVectoriser : VectoriserBase<BrightData.TensorBase<float, ITensor<float>>>
        {
            public TensorVectoriser(uint size, ISingleTypeTableSegment column)
                : base(((IDataTableSegment<BrightData.TensorBase<float, ITensor<float>>>)column).EnumerateTyped().GetEnumerator())
            {
                Size = size;
            }

            public override uint Size { get; }

            protected override IEnumerable<float> _Convert(TensorBase<float, ITensor<float>> obj)
            {
                return obj.Data.Values;
            }
        }
        class OneHotEncodeVectorised : VectoriserBase<object>
        {
            readonly Dictionary<string, uint> _stringIndex = new Dictionary<string, uint>();
            readonly float[] _buffer;
            uint _nextIndex = 0;

            public OneHotEncodeVectorised(uint size, ISingleTypeTableSegment column)
                : base(column.Enumerate().GetEnumerator())
            {
                Size = size;
                _buffer = new float[size];
            }

            public override uint Size { get; }

            protected override IEnumerable<float> _Convert(object obj)
            {
                var str = obj.ToString();
                if(_stringIndex.TryGetValue(str, out var index))
                    _stringIndex.Add(str, index = _nextIndex++);
                Array.Clear(_buffer, 0, _buffer.Length);
                _buffer[index] = 1f;
                return _buffer;
            }
        }

        public float[] GetInput(object[] r)
        {
            return r.Zip(_input, (o, c) => c.Convert(o)).SelectMany(d => d).ToArray();
        }

        public float[] GetOutput(object r)
        {
            throw new NotImplementedException();
        }

        readonly List<IColumnVectoriser> _input = new List<IColumnVectoriser>();
        readonly IColumnVectoriser _target = null;

        public uint InputSize => (uint)_input.Sum(c => c.Size);
        public uint OutputSize => _target?.Size ?? 0;
        public uint RowCount { get; }

        public IBrightDataContext Context { get; }

        public DataTableVectoriser(IDataTable dataTable)
        {
            var targetColumn = dataTable.GetTargetColumn();
            RowCount = dataTable.RowCount;
            Context = dataTable.Context;

            var inputColumnIndices = dataTable.ColumnIndices().ToList();
            var targetColumnIndices = new List<uint>();
            if (targetColumn.HasValue) {
                inputColumnIndices.Remove(targetColumn.Value);
                _target = dataTable.Columns(new[] { targetColumn.Value })
                    .Select(_GetColumnVectoriser)
                    .Single()
                ;
            }

            _input.AddRange(dataTable.Columns(inputColumnIndices.ToArray())
                .Select(_GetColumnVectoriser)
            );
        }

        public IRowOrientedDataTable Vectorize(string filePath = null)
        {
            var builder = new RowOrientedTableBuilder(RowCount, filePath);
            builder.AddColumn(ColumnType.Vector, "Input");
            if (_target != null)
                builder.AddColumn(ColumnType.Vector, "Target").SetTargetColumn(true);
            var inputSize = InputSize;
            var outputSize = OutputSize;

            var inputBuffer = new float[inputSize];
            var outputBuffer = new float[outputSize];
            for (uint i = 0; i < RowCount; i++) {
                var index = 0;
                foreach (var column in _input) {
                    foreach (var value in column.GetNext())
                        inputBuffer[index++] = value;
                }
                var input = Context.CreateVector(inputBuffer);
                if (_target != null) {
                    index = 0;
                    foreach (var value in _target.GetNext())
                        outputBuffer[index++] = value;
                    builder.AddRow(input, Context.CreateVector(outputBuffer));
                } else
                    builder.AddRow(input);
            }

            return builder.Build(Context);
        }

        static IColumnVectoriser _GetColumnVectoriser(ISingleTypeTableSegment column)
        {
            var type = column.SingleType;
            if (type.IsNumeric()) {
                return (IColumnVectoriser)Activator.CreateInstance(
                    typeof(NumericVectoriser<>).MakeGenericType(type.GetColumnType()),
                    column
                );
            }

            var metaData = column.Analyse();
            if (type == ColumnType.WeightedIndexList)
                return new WeightedIndexListVectoriser(metaData.Get<uint>(Consts.MaxIndex), column);
            else if (type == ColumnType.IndexList)
                return new IndexListVectoriser(metaData.Get<uint>(Consts.MaxIndex), column);
            else if (type.IsTensor())
                return new TensorVectoriser(metaData.Get<uint>(Consts.Size), column);

            return null;
        }
    }
}
