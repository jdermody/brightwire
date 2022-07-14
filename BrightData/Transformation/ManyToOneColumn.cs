using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.DataTable.Operations;

namespace BrightData.Transformation
{
    /// <summary>
    /// Parameters that define how to reinterpret the columns in a data table
    /// </summary>
    internal class ManyToOneColumn : IReinterpretColumns
    {
        readonly uint _outputColumnIndex;

        public ManyToOneColumn(BrightDataType newType, string? name, uint outputColumnIndex, params uint[] sourceColumnIndices)
        {
            _outputColumnIndex = outputColumnIndex;
            NewType = newType;
            Name = name;
            SourceColumnIndices = sourceColumnIndices;
            OutputColumnIndices = new[] { _outputColumnIndex };
        }

        public BrightDataType NewType { get; }
        public string? Name { get; }
        public uint[] SourceColumnIndices { get; }
        public uint[] OutputColumnIndices { get; }

        public IEnumerable<IOperation<ISingleTypeTableSegment?>> GetNewColumnOperations(BrightDataContext context, IProvideTempStreams tempStreams, uint rowCount, ICanEnumerateDisposable[] sourceColumns)
        {
            if (SourceColumnIndices.Length >= 1) {
                var metaData = new MetaData();
                if (!String.IsNullOrWhiteSpace(Name))
                    metaData.SetName(Name);
                metaData.SetType(NewType);
                metaData.Set(Consts.ColumnIndex, _outputColumnIndex);
                var outputBuffer = NewType.GetHybridBufferWithMetaData(metaData, context, tempStreams);

                if (NewType == BrightDataType.IndexList)
                    yield return new ManyToOneColumnOperation<IndexList>(
                        rowCount,
                        sourceColumns, 
                        obj => ToIndexList(context , obj),
                        (IHybridBuffer<IndexList>)outputBuffer
                    );
                else if (NewType == BrightDataType.WeightedIndexList)
                    yield return new ManyToOneColumnOperation<WeightedIndexList>(
                        rowCount,
                        sourceColumns, 
                        obj => ToWeightedIndexList(context , obj),
                        (IHybridBuffer<WeightedIndexList>)outputBuffer
                    );
                else if (NewType == BrightDataType.Vector)
                    yield return new ManyToOneColumnOperation<IVector>(
                        rowCount,
                        sourceColumns, 
                        obj => ToVector(context , obj),
                        (IHybridBuffer<IVector>)outputBuffer
                    );
                else if (NewType == BrightDataType.String)
                    yield return new ManyToOneColumnOperation<string>(
                        rowCount,
                        sourceColumns,
                        ToString,
                        (IHybridBuffer<string>)outputBuffer
                    );
                else
                    throw new NotImplementedException();
            }
            else
                throw new NotImplementedException("Currently not supported");
        }

        static IndexList ToIndexList(BrightDataContext context, object[] vals)
        {
            var indices = vals
                .Select(Convert.ToSingle)
                .Select((v, i) => (Value: v, Index: i))
                .Where(d => d.Value != 0)
                .Select(d => (uint)d.Index);
            return context.CreateIndexList(indices);
        }

        static WeightedIndexList ToWeightedIndexList(BrightDataContext context, object[] vals)
        {
            var indices = vals
                .Select(Convert.ToSingle)
                .Select((v, i) => (Value: v, Index: i))
                .Where(d => d.Value != 0)
                .Select(d => ((uint)d.Index, d.Value))
                .ToArray();
            return context.CreateWeightedIndexList(indices);
        }

        static IVector ToVector(BrightDataContext context, object[] values)
        {
            var data = values.Select(Convert.ToSingle).ToArray();
            return context.LinearAlgebraProvider.CreateVector(data);
        }

        static string ToString(object[] vals) => String.Join('|', vals.Select(Convert.ToString));

        //class ManyToOne<T> : ISingleTypeTableSegment where T : notnull
        //{
        //    readonly IHybridBuffer<T> _buffer;

        //    public ManyToOne(
        //        BrightDataContext context,
        //        IProvideTempStreams tempStreams,
        //        string name,
        //        uint newColumnIndex,
        //        ISingleTypeTableSegment[] sourceColumns,
        //        Func<BrightDataContext, object[], T> converter
        //    )
        //    {
        //        Index = newColumnIndex;
        //        Size = sourceColumns.First().Size;
        //        MetaData.SetType(BrightDataType.IndexList);
        //        MetaData.Set(Consts.Index, newColumnIndex);
        //        MetaData.Set(Consts.Name, name);
        //        _buffer = (IHybridBuffer<T>)MetaData.GetGrowableSegment(SingleType, context, tempStreams);

        //        // fill the buffer
        //        var len = sourceColumns.Length;
        //        var buffer = new object[len];
        //        var enumerators = sourceColumns.Select(c => c.Enumerate().GetEnumerator()).ToList();
        //        var ct = context.CancellationToken;
        //        while (enumerators.All(e => e.MoveNext()) && !ct.IsCancellationRequested) {
        //            for (var i = 0; i < len; i++)
        //                buffer[i] = enumerators[i].Current;
        //            _buffer.Add(converter(context, buffer));
        //        }
        //    }

        //    public MetaData MetaData { get; } = new MetaData();
        //    public uint Index { get; }
        //    public void WriteTo(BinaryWriter writer) => _buffer.CopyTo(writer.BaseStream);

        //    public void Dispose()
        //    {
        //        // nop
        //    }

        //    public BrightDataType SingleType { get; } = BrightDataType.IndexList;
        //    public IEnumerable<object> Enumerate() => _buffer.Enumerate();
        //    public uint Size { get; }
        //}
    }
}
