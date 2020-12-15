using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrightData;
using BrightTable.Segments;

namespace BrightTable.Transformations
{
    public class ReinterpretColumns
    {
        public ReinterpretColumns(ColumnType newType, string name, params uint[] columnIndices)
        {
            NewType = newType;
            Name = name;
            ColumnIndices = columnIndices;
        }

        public ColumnType NewType { get; }
        public string Name { get; }
        public uint[] ColumnIndices { get; }

        public IEnumerable<ISingleTypeTableSegment> GetNewColumns(IBrightDataContext context, IProvideTempStreams tempStreams, uint initialColumnIndex, (IColumnInfo Info, ISingleTypeTableSegment Segment)[] columns)
        {
            if (ColumnIndices.Length > 1)
            {
                // convert many columns to single index list
                if (NewType == ColumnType.IndexList)
                    yield return new ManyToOne<IndexList>(context, tempStreams, Name, initialColumnIndex, columns, _ToIndexList);
                else if(NewType == ColumnType.WeightedIndexList)
                    yield return new ManyToOne<WeightedIndexList>(context, tempStreams, Name, initialColumnIndex, columns, _ToWeightedIndexList);
                else if (NewType == ColumnType.Vector)
                    yield return new ManyToOne<Vector<float>>(context, tempStreams, Name, initialColumnIndex, columns, _ToVector);
                else if (NewType == ColumnType.String)
                    yield return new ManyToOne<string>(context, tempStreams, Name, initialColumnIndex, columns, _ToString);
            }
            else
                throw new NotImplementedException("Currently not supported");
        }

        static IndexList _ToIndexList(IBrightDataContext context, object[] vals)
        {
            var indices = vals
                .Select(Convert.ToSingle)
                .Select((v, i) => (Value: v, Index: i))
                .Where(d => d.Value != 0)
                .Select(d => (uint)d.Index);
            return IndexList.Create(context, indices);
        }

        static WeightedIndexList _ToWeightedIndexList(IBrightDataContext context, object[] vals)
        {
            var indices = vals
                .Select(Convert.ToSingle)
                .Select((v, i) => (Value: v, Index: i))
                .Where(d => d.Value != 0)
                .Select(d => new WeightedIndexList.Item((uint)d.Index, d.Value));
            return WeightedIndexList.Create(context, indices);
        }

        static Vector<float> _ToVector(IBrightDataContext context, object[] vals)
        {
            var data = vals.Select(Convert.ToSingle).ToArray();
            return context.CreateVector(data);
        }

        static string _ToString(IBrightDataContext context, object[] vals) => String.Join('|', vals.Select(Convert.ToString));

        public class ManyToOne<T> : ISingleTypeTableSegment
        {
            private readonly (IColumnInfo Info, ISingleTypeTableSegment Segment)[] _sourceColumns;
            private readonly IHybridBuffer<T> _buffer;

            public ManyToOne(IBrightDataContext context, IProvideTempStreams tempStreams, string name, uint newColumnIndex, (IColumnInfo Info, ISingleTypeTableSegment Segment)[] sourceColumns, Func<IBrightDataContext, object[], T> converter)
            {
                Index = newColumnIndex;
                _sourceColumns = sourceColumns;
                Size = _sourceColumns.First().Segment.Size;
                MetaData.SetType(ColumnType.IndexList);
                MetaData.Set(Consts.Index, newColumnIndex);
                MetaData.Set(Consts.Name, name);
                _buffer = (IHybridBuffer<T>)this.GetHybridBuffer(SingleType, context, tempStreams);

                // fill the buffer
                var len = sourceColumns.Length;
                var buffer = new object[len];
                var enumerators = sourceColumns.Select(c => c.Segment!.Enumerate().GetEnumerator()).ToList();
                while (enumerators.All(e => e.MoveNext()))
                {
                    for (var i = 0; i < len; i++)
                        buffer[i] = enumerators[i].Current;
                    _buffer.Add(converter(context, buffer));
                }
            }

            public IMetaData MetaData { get; } = new MetaData();
            public uint Index { get; }
            public void WriteTo(BinaryWriter writer) => _buffer.CopyTo(writer.BaseStream);

            public void Dispose()
            {
                // nop
            }

            public ColumnType SingleType { get; } = ColumnType.IndexList;
            public IEnumerable<object> Enumerate() => _buffer.Enumerate();
            public uint Size { get; }
        }
    }
}
