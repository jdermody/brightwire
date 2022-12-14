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
        public ManyToOneColumn(BrightDataType newType, string? name, params uint[] sourceColumnIndices)
        {
            NewType = newType;
            Name = name;
            SourceColumnIndices = sourceColumnIndices;
        }

        public BrightDataType NewType { get; }
        public string? Name { get; }
        public uint[] SourceColumnIndices { get; }

        public IEnumerable<IOperation<ITypedSegment?>> GetNewColumnOperations(BrightDataContext context, IProvideTempStreams tempStreams, uint rowCount, ICanEnumerateDisposable[] sourceColumns)
        {
            if (SourceColumnIndices.Length >= 1) {
                var metaData = new MetaData();
                if (!String.IsNullOrWhiteSpace(Name))
                    metaData.SetName(Name);
                metaData.SetType(NewType);
                var outputBuffer = NewType.GetCompositeBufferWithMetaData(metaData, context, tempStreams);

                if (NewType == BrightDataType.IndexList)
                    yield return new ManyToOneColumnOperation<IndexList>(
                        rowCount,
                        sourceColumns, 
                        obj => ToIndexList(context , obj),
                        (ICompositeBuffer<IndexList>)outputBuffer
                    );
                else if (NewType == BrightDataType.WeightedIndexList)
                    yield return new ManyToOneColumnOperation<WeightedIndexList>(
                        rowCount,
                        sourceColumns, 
                        obj => ToWeightedIndexList(context , obj),
                        (ICompositeBuffer<WeightedIndexList>)outputBuffer
                    );
                else if (NewType == BrightDataType.Vector)
                    yield return new ManyToOneColumnOperation<IReadOnlyVector>(
                        rowCount,
                        sourceColumns, 
                        obj => ToVector(context , obj),
                        (ICompositeBuffer<IReadOnlyVector>)outputBuffer
                    );
                else if (NewType == BrightDataType.String)
                    yield return new ManyToOneColumnOperation<string>(
                        rowCount,
                        sourceColumns,
                        ToString,
                        (ICompositeBuffer<string>)outputBuffer
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
    }
}
