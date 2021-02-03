using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.Converter;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.Transformation
{
    internal class DataTableVectoriser : IHaveDataContext, IDataTableVectoriser
    {
        interface IColumnVectoriser : IDisposable
        {
            uint ColumnIndex { get; }
            IEnumerable<float> GetNext();
            IEnumerable<float> Convert(object obj);
            uint Size { get; }
        }
        abstract class VectoriserBase<T> : IColumnVectoriser
        {
            readonly IEnumerator<T> _enumerator;

            protected VectoriserBase(uint columnIndex, IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
                ColumnIndex = columnIndex;
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public abstract uint Size { get; }
            public uint ColumnIndex { get; }

            public IEnumerable<float> Convert(object obj) => Vectorize((T)obj);

            public IEnumerable<float> GetNext()
            {
                if (_enumerator.MoveNext()) {
                    foreach (var item in Vectorize(_enumerator.Current))
                        yield return item;
                }
            }

            protected abstract IEnumerable<float> Vectorize(T obj);
        }
        class NumericVectoriser<T> : VectoriserBase<T>
            where T : struct
        {
            readonly ICanConvert<T, float> _converter = StaticConverters.ConvertToFloat<T>();

            public NumericVectoriser(ISingleTypeTableSegment column)
                : base(column.MetaData.GetIndex(), ((IDataTableSegment<T>)column).EnumerateTyped().GetEnumerator())
            {
            }

            public override uint Size => 1;

            protected override IEnumerable<float> Vectorize(T obj)
            {
                yield return _converter.Convert(obj);
            }
        }
        class WeightedIndexListVectoriser : VectoriserBase<WeightedIndexList>
        {
            readonly uint _maxSize;

            public WeightedIndexListVectoriser(uint maxSize, ISingleTypeTableSegment column)
                : base(column.MetaData.GetIndex(), ((IDataTableSegment<WeightedIndexList>)column).EnumerateTyped().GetEnumerator())
            {
                _maxSize = maxSize;
            }

            public override uint Size => _maxSize;

            protected override IEnumerable<float> Vectorize(WeightedIndexList obj)
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
                : base(column.MetaData.GetIndex(), ((IDataTableSegment<IndexList>)column).EnumerateTyped().GetEnumerator())
            {
                _maxSize = maxSize;
            }

            public override uint Size => _maxSize;

            protected override IEnumerable<float> Vectorize(IndexList obj)
            {
                var indexSet = new HashSet<uint>(obj.Indices);
                for (uint i = 0; i < _maxSize; i++)
                    yield return indexSet.Contains(i) ? 1f : 0f;
            }
        }
        class TensorVectoriser : VectoriserBase<ITensor<float>>
        {
            public TensorVectoriser(uint size, ISingleTypeTableSegment column)
                : base(column.MetaData.GetIndex(), ((IDataTableSegment<ITensor<float>>)column).EnumerateTyped().GetEnumerator())
            {
                Size = size;
            }

            public override uint Size { get; }

            protected override IEnumerable<float> Vectorize(ITensor<float> obj)
            {
                return obj.Segment.Values;
            }
        }

        class OneHotEncodeVectorised : VectoriserBase<object>
        {
            readonly Dictionary<string, uint> _stringIndex = new Dictionary<string, uint>();
            readonly float[] _buffer;
            uint _nextIndex = 0;

            public OneHotEncodeVectorised(uint size, ISingleTypeTableSegment column)
                : base(column.MetaData.GetIndex(), column.Enumerate().GetEnumerator())
            {
                Size = size;
                _buffer = new float[size];
            }

            public override uint Size { get; }

            protected override IEnumerable<float> Vectorize(object obj)
            {
                var str = obj.ToString();
                Array.Clear(_buffer, 0, _buffer.Length);
                if (str != null) {
                    if (!_stringIndex.TryGetValue(str, out var index))
                        _stringIndex.Add(str, index = _nextIndex++);
                    _buffer[index] = 1f;
                }
                return _buffer;
            }

            public string GetOutputLabel(uint index)
            {
                var ret = _stringIndex
                    .Where(kv => kv.Value == index)
                    .Select(kv => kv.Key)
                    .SingleOrDefault();
                if (ret == null)
                    throw new ArgumentException($"Index {index} not found");
                return ret;
            }
        }

        class OneHotEncode : VectoriserBase<object>
        {
            readonly Dictionary<string, uint> _stringIndex = new Dictionary<string, uint>();
            uint _nextIndex = 0;

            public OneHotEncode(ISingleTypeTableSegment column)
                : base(column.MetaData.GetIndex(), column.Enumerate().GetEnumerator())
            {
                Size = 1;
            }

            public override uint Size { get; }

            protected override IEnumerable<float> Vectorize(object obj)
            {
                var str = obj.ToString();
                if (str != null) {
                    if (!_stringIndex.TryGetValue(str, out var index))
                        _stringIndex.Add(str, index = _nextIndex++);
                    yield return index;
                }
            }

            public string GetOutputLabel(uint index)
            {
                var ret = _stringIndex
                    .Where(kv => kv.Value == index)
                    .Select(kv => kv.Key)
                    .SingleOrDefault();
                if (ret == null)
                    throw new ArgumentException($"Index {index} not found");
                return ret;
            }
        }

        readonly List<IColumnVectoriser> _input = new List<IColumnVectoriser>();

        public uint OutputSize => (uint)_input.Sum(c => c.Size);
        public uint RowCount { get; }
        public IBrightDataContext Context { get; }

        public float[] Vectorise(object[] row)
        {
            return _input.SelectMany(i => i.Convert(row[i.ColumnIndex])).ToArray();
        }

        public DataTableVectoriser(IDataTable dataTable, params uint[] columnIndices)
        {
            if (columnIndices.Length == 0)
                columnIndices = dataTable.ColumnIndices().ToArray();

            RowCount = dataTable.RowCount;
            Context = dataTable.Context;

            _input.AddRange(dataTable.Columns(columnIndices)
                .Select(GetColumnVectoriser)
            );
        }

        public IEnumerable<Vector<float>> Enumerate()
        {
            var ret = new float[OutputSize];

            for (uint i = 0; i < RowCount; i++) {
                var index = 0;
                foreach (var column in _input) {
                    foreach (var value in column.GetNext())
                        ret[index++] = value;
                }
                var input = Context.CreateVector(ret);
                yield return input;
            }
        }

        public string GetOutputLabel(uint columnIndex, uint vectorIndex)
        {
            if(columnIndex >= _input.Count)
                throw new ArgumentException($"Column index should be less than {_input.Count}");
            if(!(_input[(int) columnIndex] is OneHotEncodeVectorised column))
                throw new ArgumentException($"Column {columnIndex} is not a one hot encoded column");
            return column.GetOutputLabel(vectorIndex);
        }

        static IColumnVectoriser GetColumnVectoriser(ISingleTypeTableSegment column)
        {
            var type = column.SingleType;
            var columnClass = ColumnTypeClassifier.GetClass(type, column.MetaData);
            var metaData = column.Analyse();

            if ((columnClass & ColumnClass.Categorical) != 0)
                return new OneHotEncodeVectorised(metaData.GetNumDistinct(), column);

            if ((columnClass & ColumnClass.Numeric) != 0) {
                return GenericActivator.Create<IColumnVectoriser>(
                    typeof(NumericVectoriser<>).MakeGenericType(type.GetDataType()),
                    column
                );
            }

            if ((columnClass & ColumnClass.Tensor) != 0)
                return new TensorVectoriser(metaData.Get<uint>(Consts.Size), column);

            if (type == ColumnType.WeightedIndexList)
                return new WeightedIndexListVectoriser(metaData.Get<uint>(Consts.MaxIndex), column);

            if (type == ColumnType.IndexList)
                return new IndexListVectoriser(metaData.Get<uint>(Consts.MaxIndex), column);

            throw new NotImplementedException();
        }
    }
}
