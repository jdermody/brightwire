using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData.ConstraintValidation;
using BrightData.Helper;
using BrightData.Types;

namespace BrightData
{
    public partial class ExtensionMethods
    {
        /// <summary>
        /// Creates a data field specification for a data type
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="name"></param>
        /// <param name="canRepeat"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IDataTypeSpecification AsDataFieldSpecification(this BrightDataType dataType, string? name, bool canRepeat = false) => dataType switch 
        {
            BrightDataType.BinaryData        => new FieldSpecification<BinaryData>(name, canRepeat),
            BrightDataType.Boolean           => new FieldSpecification<bool>(name, canRepeat),
            BrightDataType.SByte             => new FieldSpecification<byte>(name, canRepeat),
            BrightDataType.Short             => new FieldSpecification<short>(name, canRepeat),
            BrightDataType.Int               => new FieldSpecification<int>(name, canRepeat),
            BrightDataType.Long              => new FieldSpecification<long>(name, canRepeat),
            BrightDataType.Float             => new FieldSpecification<float>(name, canRepeat),
            BrightDataType.Double            => new FieldSpecification<double>(name, canRepeat),
            BrightDataType.Decimal           => new FieldSpecification<decimal>(name, canRepeat),
            BrightDataType.String            => new FieldSpecification<string>(name, canRepeat),
            BrightDataType.Date              => new FieldSpecification<DateTime>(name, canRepeat),
            BrightDataType.IndexList         => new FieldSpecification<IndexList>(name, canRepeat),
            BrightDataType.WeightedIndexList => new FieldSpecification<WeightedIndexList>(name, canRepeat),
            BrightDataType.Vector            => new FieldSpecification<IReadOnlyVector>(name, canRepeat),
            BrightDataType.Matrix            => new FieldSpecification<IReadOnlyMatrix>(name, canRepeat),
            BrightDataType.Tensor3D          => new FieldSpecification<IReadOnlyTensor3D>(name, canRepeat),
            BrightDataType.Tensor4D          => new FieldSpecification<IReadOnlyTensor4D>(name, canRepeat),
            _                                => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };

        /// <summary>
        /// Creates a type specification for a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IDataTypeSpecification GetTypeSpecification(this IDataTable dataTable) => new DataTableSpecification(dataTable);

        class ColumnFilter<T> : IAcceptBlock<T> where T: notnull
        {
            readonly IDataTypeSpecification<T> _filter;
            readonly HashSet<uint> _nonConformingRowIndices;
            uint _index = 0;

            public ColumnFilter(uint columnIndex, BrightDataType columnType, IDataTypeSpecification<T> filter, HashSet<uint> nonConformingRowIndices)
            {
                _filter = filter;
                _nonConformingRowIndices = nonConformingRowIndices;
                ColumnIndex = columnIndex;
                ColumnType = columnType;
            }

            public uint ColumnIndex { get; }
            public BrightDataType ColumnType { get; }

            public void Add(ReadOnlySpan<T> inputBlock)
            {
                foreach (var item in inputBlock) {
                    if (!_filter.IsValid(item))
                        _nonConformingRowIndices.Add(_index);
                    ++_index;
                }
            }
            public Type BlockType => typeof(T);
        }

        static IAcceptBlock GetColumnReader(uint columnIndex, BrightDataType columnType, HashSet<uint> nonConformingRowIndices, IDataTypeSpecification typeSpecification)
        {
            return GenericActivator.Create<IAcceptBlock>(typeof(ColumnFilter<>).MakeGenericType(typeSpecification.UnderlyingType), columnIndex, columnType, typeSpecification, nonConformingRowIndices);
        }

        /// <summary>
        /// Finds the row indices of any row that does not conform to the type specification
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<HashSet<uint>> FindNonConformingRows(this IDataTypeSpecification typeInfo, IDataTable dataTable)
        {
            if (typeInfo.UnderlyingType != typeof(IDataTable))
                throw new ArgumentException("Expected data table specification");
            if(typeInfo.Children?.Length != dataTable.ColumnCount)
                throw new ArgumentException("Expected data table and type info column count to match");

            var ret = new HashSet<uint>();
            var ops = dataTable.CopyTo(typeInfo.Children.Select((ts, ci) => GetColumnReader((uint)ci, dataTable.ColumnTypes[ci], ret, ts)).ToArray());
            await ops.Process();
            return ret;
        }
    }
}
