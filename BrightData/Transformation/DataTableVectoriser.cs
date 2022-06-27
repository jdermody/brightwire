using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.Converter;
using BrightData.DataTable2;
using BrightData.Helper;
using BrightData.LinearAlgebra;

namespace BrightData.Transformation
{
    internal class DataTableVectoriser : IHaveDataContext, IDataTableVectoriser
    {
        interface IColumnVectoriser : IDisposable, ICanWriteToBinaryWriter
        {
            uint ColumnIndex { get; }
            IEnumerable<float> GetNext();
            IEnumerable<float> Convert(object obj);
            uint Size { get; }
        }

        interface IColumnVectoriser<in T> : IColumnVectoriser where T : notnull
        {
            IEnumerable<float> Convert(T obj);
        }

        enum VectorisationType : byte
        {
            Numeric,
            WeightedIndexList,
            IndexList,
            Tensor,
            OneHotEncodeToVector,
            OneHotEncode
        }
        abstract class VectoriserBase<T> : IColumnVectoriser<T> where T : notnull
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
            public abstract VectorisationType VectorisationType { get; }

            public IEnumerable<float> Convert(T obj) => Vectorize(obj);
            public IEnumerable<float> Convert(object obj) => Vectorize((T)obj);

            public IEnumerable<float> GetNext()
            {
                if (_enumerator.MoveNext()) {
                    foreach (var item in Vectorize(_enumerator.Current))
                        yield return item;
                }
            }

            protected abstract IEnumerable<float> Vectorize(T obj);

            public virtual void WriteTo(BinaryWriter writer)
            {
                writer.Write((byte)VectorisationType);
                writer.Write(ColumnIndex);
            }
        }
        class NumericVectoriser<T> : VectoriserBase<T>
            where T : struct
        {
            readonly ICanConvert<T, float> _converter = StaticConverters.GetConverterToFloat<T>();

            public NumericVectoriser(ISingleTypeTableSegment column)
                : base(column.MetaData.GetIndex(), ((IDataTableSegment<T>)column).EnumerateTyped().GetEnumerator())
            {
            }

            public override uint Size => 1;
            public override VectorisationType VectorisationType => VectorisationType.Numeric;

            protected override IEnumerable<float> Vectorize(T obj)
            {
                yield return _converter.Convert(obj);
            }
        }
        class WeightedIndexListVectoriser : VectoriserBase<WeightedIndexList>
        {
            readonly uint _maxSize;

            WeightedIndexListVectoriser(ISingleTypeTableSegment column) : base(column.MetaData.GetIndex(), ((IDataTableSegment<WeightedIndexList>)column).EnumerateTyped().GetEnumerator()) { }
            public WeightedIndexListVectoriser(uint maxSize, ISingleTypeTableSegment column) : this(column)
            {
                _maxSize = maxSize;
            }
            public WeightedIndexListVectoriser(ISingleTypeTableSegment column, BinaryReader reader) : this(column)
            {
                _maxSize = reader.ReadUInt32();
            }

            public override void WriteTo(BinaryWriter writer)
            {
                base.WriteTo(writer);
                writer.Write(_maxSize);
            }

            public override uint Size => _maxSize + 1;
            public override VectorisationType VectorisationType => VectorisationType.WeightedIndexList;

            protected override IEnumerable<float> Vectorize(WeightedIndexList obj)
            {
                var indexTable = obj.Indices.ToDictionary(d => d.Index, d => d.Weight);
                for (uint i = 0; i <= _maxSize; i++)
                    yield return indexTable.TryGetValue(i, out var val) ? val : 0f;
            }
        }
        class IndexListVectoriser : VectoriserBase<IndexList>
        {
            readonly uint _maxSize;

            IndexListVectoriser(ISingleTypeTableSegment column) : base(column.MetaData.GetIndex(), ((IDataTableSegment<IndexList>)column).EnumerateTyped().GetEnumerator()) { }
            public IndexListVectoriser(uint maxSize, ISingleTypeTableSegment column) : this(column)
            {
                _maxSize = maxSize;
            }
            public IndexListVectoriser(ISingleTypeTableSegment column, BinaryReader reader) : this(column)
            {
                _maxSize = reader.ReadUInt32();
            }

            public override uint Size => _maxSize + 1;
            public override VectorisationType VectorisationType => VectorisationType.IndexList;

            protected override IEnumerable<float> Vectorize(IndexList obj)
            {
                var indexSet = new HashSet<uint>(obj.Indices);
                for (uint i = 0; i <= _maxSize; i++)
                    yield return indexSet.Contains(i) ? 1f : 0f;
            }

            public override void WriteTo(BinaryWriter writer)
            {
                base.WriteTo(writer);
                writer.Write(_maxSize);
            }
        }
        class TensorVectoriser : VectoriserBase<ITensor<float>>
        {
            TensorVectoriser(ISingleTypeTableSegment column) : base(column.MetaData.GetIndex(), ((IDataTableSegment<ITensor<float>>)column).EnumerateTyped().GetEnumerator()) {}
            public TensorVectoriser(uint size, ISingleTypeTableSegment column) : this(column)
            {
                Size = size;
            }
            public TensorVectoriser(ISingleTypeTableSegment column, BinaryReader reader) : this(column)
            {
                Size = reader.ReadUInt32();
            }

            public override void WriteTo(BinaryWriter writer)
            {
                base.WriteTo(writer);
                writer.Write(Size);
            }

            public override uint Size { get; }
            public override VectorisationType VectorisationType => VectorisationType.Tensor;

            protected override IEnumerable<float> Vectorize(ITensor<float> obj)
            {
                return obj.Segment.Values;
            }
        }

        interface IHaveOutputLabel
        {
            string GetOutputLabel(uint index);
        }

        class OneHotEncodeVectorised : VectoriserBase<object>, IHaveOutputLabel
        {
            readonly Dictionary<string, uint> _stringIndex = new();
            readonly float[] _buffer;
            uint _nextIndex = 0;

#pragma warning disable 8618
            OneHotEncodeVectorised(ISingleTypeTableSegment column) : base(column.MetaData.GetIndex(), column.Enumerate().GetEnumerator()) { }
#pragma warning restore 8618
            public OneHotEncodeVectorised(uint size, ISingleTypeTableSegment column) : this(column)
            {
                _buffer = new float[Size = size];
            }
            public OneHotEncodeVectorised(ISingleTypeTableSegment column, BinaryReader reader) : this(column)
            {
                _nextIndex = reader.ReadUInt32();
                var size = reader.ReadUInt32();
                _buffer = new float[Size = size];
                var len = reader.ReadInt32();
                for(uint i = 0; i < len; i++)
                    _stringIndex.Add(reader.ReadString(), i);
            }

            public override void WriteTo(BinaryWriter writer)
            {
                base.WriteTo(writer);
                writer.Write(_nextIndex);
                writer.Write(Size);
                writer.Write(_stringIndex.Count);
                foreach(var item in _stringIndex.OrderBy(d => d.Value))
                    writer.Write(item.Key);
            }

            public override uint Size { get; }
            public override VectorisationType VectorisationType => VectorisationType.OneHotEncodeToVector;

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

        class OneHotEncode : VectoriserBase<object>, IHaveOutputLabel
        {
            readonly Dictionary<string, uint> _stringIndex = new();
            uint _nextIndex = 0;

            public OneHotEncode(ISingleTypeTableSegment column)
                : base(column.MetaData.GetIndex(), column.Enumerate().GetEnumerator())
            {
                Size = 1;
            }
            public OneHotEncode(ISingleTypeTableSegment column, BinaryReader reader)
                : this(column)
            {
                _nextIndex = reader.ReadUInt32();
                var len = reader.ReadInt32();
                for(uint i = 0; i < len; i++)
                    _stringIndex.Add(reader.ReadString(), i);
            }

            public override void WriteTo(BinaryWriter writer)
            {
                base.WriteTo(writer);
                writer.Write(_nextIndex);
                writer.Write(_stringIndex.Count);
                foreach(var item in _stringIndex.OrderBy(d => d.Value))
                    writer.Write(item.Key);
            }

            public override uint Size { get; }
            public override VectorisationType VectorisationType => VectorisationType.OneHotEncode;

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

        readonly List<IColumnVectoriser> _input = new();

        public uint OutputSize => (uint)_input.Sum(c => c.Size);
        public uint RowCount { get; }
        public IBrightDataContext Context { get; }

        public float[] Vectorise(object[] row)
        {
            return _input.SelectMany(i => i.Convert(row[i.ColumnIndex])).ToArray();
        }

        public float[] Vectorise(IDataTableSegment segment)
        {
            return _input.SelectMany(i => i.Convert(segment[i.ColumnIndex])).ToArray();
        }

        public DataTableVectoriser(BrightDataTable dataTable, bool oneHotEncodeToMultipleColumns, params uint[] columnIndices)
        {
            columnIndices = dataTable.AllOrSpecifiedColumnIndices(columnIndices).ToArray();

            RowCount = dataTable.RowCount;
            Context = dataTable.Context;

            var analysis = dataTable.GetColumnAnalysis(columnIndices).ToDictionary(d => d.ColumnIndex, d => d.MetaData);
            _input.AddRange(columnIndices
                .Select(ci => GetColumnVectoriser(dataTable.GetColumn(ci), analysis[ci], oneHotEncodeToMultipleColumns))
            );
        }

        public DataTableVectoriser(BrightDataTable dataTable, BinaryReader reader)
        {
            RowCount = dataTable.RowCount;
            Context = dataTable.Context;

            var len = reader.ReadInt32();
            for(var i = 0; i < len; i++)
                _input.Add(GetColumnVectoriser(dataTable, reader));
        }

        public void Dispose()
        {
            foreach(var item in _input)
                item.Dispose();
        }

        static IColumnVectoriser GetColumnVectoriser(BrightDataTable dataTable, BinaryReader reader)
        {
            var type = (VectorisationType) reader.ReadByte();
            var column = dataTable.GetColumn(reader.ReadUInt32());

            return type switch {
                VectorisationType.WeightedIndexList => new WeightedIndexListVectoriser(column, reader),
                VectorisationType.Numeric => GenericActivator.Create<IColumnVectoriser>(
                    typeof(NumericVectoriser<>).MakeGenericType(column.SingleType.GetDataType()),
                    column
                ),
                VectorisationType.IndexList => new IndexListVectoriser(column, reader),
                VectorisationType.Tensor => new TensorVectoriser(column, reader),
                VectorisationType.OneHotEncodeToVector => new OneHotEncodeVectorised(column, reader),
                VectorisationType.OneHotEncode => new OneHotEncode(column, reader),
                _ => throw new NotImplementedException()
            };
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(_input.Count);
            foreach(var item in _input)
                item.WriteTo(writer);
        }

        public IEnumerable<IVector> Enumerate()
        {
            var ret = new float[OutputSize];

            for (uint i = 0; i < RowCount; i++) {
                var index = 0;
                foreach (var column in _input) {
                    foreach (var value in column.GetNext())
                        ret[index++] = value;
                }
                var input = Context.LinearAlgebraProvider2.CreateVector(ret);
                yield return input;
            }
        }

        public string GetOutputLabel(uint vectorIndex, uint columnIndex)
        {
            if(columnIndex >= _input.Count)
                throw new ArgumentException($"Column index should be less than {_input.Count}");
            if(_input[(int) columnIndex] is not IHaveOutputLabel column)
                throw new ArgumentException($"Column {columnIndex} is not a one hot encoded column");
            return column.GetOutputLabel(vectorIndex);
        }

        static IColumnVectoriser GetColumnVectoriser(ISingleTypeTableSegment column, IMetaData analysedMetaData, bool oneHotEncodeToMultipleColumns)
        {
            var type = column.SingleType;
            var columnClass = ColumnTypeClassifier.GetClass(type, column.MetaData);

            if ((columnClass & ColumnClass.Categorical) != 0) {
                return oneHotEncodeToMultipleColumns
                    ? new OneHotEncodeVectorised(analysedMetaData.GetNumDistinct(), column)
                    : new OneHotEncode(column)
                ;
            }

            if ((columnClass & ColumnClass.Numeric) != 0) {
                return GenericActivator.Create<IColumnVectoriser>(
                    typeof(NumericVectoriser<>).MakeGenericType(type.GetDataType()),
                    column
                );
            }

            if ((columnClass & ColumnClass.Tensor) != 0)
                return new TensorVectoriser(analysedMetaData.Get<uint>(Consts.Size), column);

            if (type == BrightDataType.WeightedIndexList)
                return new WeightedIndexListVectoriser(analysedMetaData.Get<uint>(Consts.MaxIndex), column);

            if (type == BrightDataType.IndexList)
                return new IndexListVectoriser(analysedMetaData.Get<uint>(Consts.MaxIndex), column);

            throw new NotImplementedException();
        }
    }
}
