using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrightData.DataTable.ConstraintValidation;
using BrightData.Helper;
using BrightData.LinearAlgebra.ReadOnly;
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
            BrightDataType.Vector            => new FieldSpecification<ReadOnlyVector>(name, canRepeat),
            BrightDataType.Matrix            => new FieldSpecification<ReadOnlyMatrix>(name, canRepeat),
            BrightDataType.Tensor3D          => new FieldSpecification<ReadOnlyTensor3D>(name, canRepeat),
            BrightDataType.Tensor4D          => new FieldSpecification<ReadOnlyTensor4D>(name, canRepeat),
            _                                => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };

        /// <summary>
        /// Creates a type specification for a data table
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IDataTypeSpecification GetTypeSpecification(this IDataTable dataTable) => new DataTableSpecification(dataTable);

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
            var ops = dataTable.CopyTo(typeInfo.Children.Select((ts, ci) => GenericTypeMapping.ColumnFilter(ts.UnderlyingType, (uint)ci, dataTable.ColumnTypes[ci], ret, ts)).ToArray());
            await ops.ExecuteAllAsOne();
            return ret;
        }
    }
}
