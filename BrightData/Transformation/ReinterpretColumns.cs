using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightData.LinearAlgebra;

namespace BrightData.Transformation
{
    /// <summary>
    /// Parameters that define how to reinterpret the columns in a data table
    /// </summary>
    internal class ReinterpretColumns : IReinterpretColumnsParam
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
            if (ColumnIndices.Length >= 1)
            {
                // convert many columns to single index list
                if (NewType == ColumnType.IndexList)
                    yield return new ManyToOne<IndexList>(context, tempStreams, Name, initialColumnIndex, columns, ToIndexList);
                else if(NewType == ColumnType.WeightedIndexList)
                    yield return new ManyToOne<WeightedIndexList>(context, tempStreams, Name, initialColumnIndex, columns, ToWeightedIndexList);
                else if (NewType == ColumnType.Vector)
                    yield return new ManyToOne<Vector<float>>(context, tempStreams, Name, initialColumnIndex, columns, ToVector);
                else if (NewType == ColumnType.String)
                    yield return new ManyToOne<string>(context, tempStreams, Name, initialColumnIndex, columns, ToString);
            }
            else
                throw new NotImplementedException("Currently not supported");
        }

        static IndexList ToIndexList(IBrightDataContext context, object[] vals)
        {
            var indices = vals
                .Select(Convert.ToSingle)
                .Select((v, i) => (Value: v, Index: i))
                .Where(d => d.Value != 0)
                .Select(d => (uint)d.Index);
            return context.CreateIndexList(indices);
        }

        static WeightedIndexList ToWeightedIndexList(IBrightDataContext context, object[] vals)
        {
            var indices = vals
                .Select(Convert.ToSingle)
                .Select((v, i) => (Value: v, Index: i))
                .Where(d => d.Value != 0)
                .Select(d => ((uint)d.Index, d.Value))
                .ToArray();
            return context.CreateWeightedIndexList(indices);
        }

        static Vector<float> ToVector(IBrightDataContext context, object[] values)
        {
            var data = values.Select(Convert.ToSingle).ToArray();
            return context.CreateVector(data);
        }

        static string ToString(IBrightDataContext context, object[] vals) => String.Join('|', vals.Select(Convert.ToString));

        class ManyToOne<T> : ISingleTypeTableSegment where T: notnull
        {
            readonly IHybridBuffer<T> _buffer;

            public ManyToOne(
                IBrightDataContext context, 
                IProvideTempStreams tempStreams, 
                string name, 
                uint newColumnIndex, 
                IReadOnlyCollection<(IColumnInfo Info, ISingleTypeTableSegment Segment)> sourceColumns, 
                Func<IBrightDataContext, object[], T> converter
            ) {
                Index = newColumnIndex;
                Size = sourceColumns.First().Segment.Size;
                MetaData.SetType(ColumnType.IndexList);
                MetaData.Set(Consts.Index, newColumnIndex);
                MetaData.Set(Consts.Name, name);
                _buffer = (IHybridBuffer<T>)MetaData.GetGrowableSegment(SingleType, context, tempStreams);

                // fill the buffer
                var len = sourceColumns.Count;
                var buffer = new object[len];
                var enumerators = sourceColumns.Select(c => c.Segment.Enumerate().GetEnumerator()).ToList();
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
