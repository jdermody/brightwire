using System;
using System.Collections.Generic;
using System.Linq;
using BrightData.DataTypeSpecification;
using BrightData.Helper;
using BrightData.LinearAlgebra;

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
        public static IDataTypeSpecification AsDataFieldSpecification(this BrightDataType dataType, string? name, bool canRepeat = false) => dataType switch {
            BrightDataType.BinaryData => new FieldSpecification<BinaryData>(name, canRepeat),
            BrightDataType.Boolean => new FieldSpecification<bool>(name, canRepeat),
            BrightDataType.SByte => new FieldSpecification<byte>(name, canRepeat),
            BrightDataType.Short => new FieldSpecification<short>(name, canRepeat),
            BrightDataType.Int => new FieldSpecification<int>(name, canRepeat),
            BrightDataType.Long => new FieldSpecification<long>(name, canRepeat),
            BrightDataType.Float => new FieldSpecification<float>(name, canRepeat),
            BrightDataType.Double => new FieldSpecification<double>(name, canRepeat),
            BrightDataType.Decimal => new FieldSpecification<decimal>(name, canRepeat),
            BrightDataType.String => new FieldSpecification<string>(name, canRepeat),
            BrightDataType.Date => new FieldSpecification<DateTime>(name, canRepeat),
            BrightDataType.IndexList => new FieldSpecification<IndexList>(name, canRepeat),
            BrightDataType.WeightedIndexList => new FieldSpecification<WeightedIndexList>(name, canRepeat),
            BrightDataType.FloatVector => new FieldSpecification<System.Numerics.Vector<float>>(name, canRepeat),
            BrightDataType.FloatMatrix => new FieldSpecification<Matrix<float>>(name, canRepeat),
            BrightDataType.FloatTensor3D => new FieldSpecification<Tensor3D<float>>(name, canRepeat),
            BrightDataType.FloatTensor4D => new FieldSpecification<Tensor4D<float>>(name, canRepeat),
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };

        /// <summary>
        /// Creates a type specification for a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IDataTypeSpecification GetTypeSpecification(this IDataTable dataTable) => new DataTableSpecification(dataTable);

        class ColumnFilter<T> : IConsumeColumnData<T> where T: notnull
        {
            readonly IDataTypeSpecification<T> _filter;
            readonly HashSet<uint> _nonConformingRowIndices;

            public ColumnFilter(uint columnIndex, BrightDataType columnType, IDataTypeSpecification<T> filter, HashSet<uint> nonConformingRowIndices)
            {
                _filter = filter;
                _nonConformingRowIndices = nonConformingRowIndices;
                ColumnIndex = columnIndex;
                ColumnType = columnType;
            }

            public uint ColumnIndex { get; }
            public BrightDataType ColumnType { get; }
            public void Add(T value, uint index)
            {
                if(!_filter.IsValid(value))
                    _nonConformingRowIndices.Add(index);
            }

            public void Append(Span<T> data)
            {
                throw new NotImplementedException();
            }
        }

        static IConsumeColumnData GetColumnReader(uint columnIndex, BrightDataType columnType, HashSet<uint> nonConformingRowIndices, IDataTypeSpecification typeSpecification)
        {
            return GenericActivator.Create<IConsumeColumnData>(typeof(ColumnFilter<>).MakeGenericType(typeSpecification.UnderlyingType), columnIndex, columnType, typeSpecification, nonConformingRowIndices);
        }

        /// <summary>
        /// Finds the row indices of any row that does not conform to the type specification
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static HashSet<uint> FindNonConformingRows(this IDataTypeSpecification typeInfo, IDataTable dataTable)
        {
            if (typeInfo.UnderlyingType != typeof(IDataTable))
                throw new ArgumentException("Expected data table specification");
            if(typeInfo.Children?.Length != dataTable.ColumnCount)
                throw new ArgumentException("Expected data table and type info column count to match");

            var ret = new HashSet<uint>();
            var readers = dataTable.ColumnTypes
                .Select((ct, i) => (ColumnType:ct, Index:(uint)i))
                .Zip(typeInfo.Children!, (ct, ts) => GetColumnReader(ct.Index, ct.ColumnType, ret, ts));
            dataTable.ReadTyped(readers);
            return ret;
        }
    }
}
