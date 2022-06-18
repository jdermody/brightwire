using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrightData.DataTable2
{
    public struct DataRangeColumnType
    {
        public uint StartIndex { get; set; }
        public uint Count { get; set; }
    }

    public struct MatrixColumnType
    {
        public uint StartIndex { get; set; }
        public uint RowCount { get; set; }
        public uint ColumnCount { get; set; }
        public uint Size => RowCount * ColumnCount;
    }

    public struct Tensor3DColumnType
    {
        public uint StartIndex { get; set; }
        public uint Depth { get; set; }
        public uint RowCount { get; set; }
        public uint ColumnCount { get; set; }
        public uint Size => Depth * RowCount * ColumnCount;
    }

    public struct Tensor4DColumnType
    {
        public uint StartIndex { get; set; }
        public uint Count { get; set; }
        public uint Depth { get; set; }
        public uint RowCount { get; set; }
        public uint ColumnCount { get; set; }
        public uint Size => Count * Depth * RowCount * ColumnCount;
    }

    public static class ColumnTypeHelper
    {
        static (Type, uint) GetTypeAndSize<T>() => (typeof(T), (uint)Unsafe.SizeOf<T>());

        public static (Type, uint) GetColumnType(this BrightDataType dataType) => dataType switch {
            BrightDataType.BinaryData                                          => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Boolean                                             => GetTypeAndSize<bool>(),
            BrightDataType.SByte                                               => GetTypeAndSize<sbyte>(),
            BrightDataType.Short                                               => GetTypeAndSize<short>(),
            BrightDataType.Int                                                 => GetTypeAndSize<int>(),
            BrightDataType.Long                                                => GetTypeAndSize<long>(),
            BrightDataType.Float                                               => GetTypeAndSize<float>(),
            BrightDataType.Double                                              => GetTypeAndSize<double>(),
            BrightDataType.Decimal                                             => GetTypeAndSize<decimal>(),
            BrightDataType.String                                              => GetTypeAndSize<uint>(),
            BrightDataType.Date                                                => GetTypeAndSize<DateTime>(),
            BrightDataType.IndexList                                           => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.WeightedIndexList                                   => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Vector                                              => GetTypeAndSize<DataRangeColumnType>(),
            BrightDataType.Matrix                                              => GetTypeAndSize<MatrixColumnType>(),
            BrightDataType.Tensor3D                                            => GetTypeAndSize<Tensor3DColumnType>(),
            BrightDataType.Tensor4D                                            => GetTypeAndSize<Tensor4DColumnType>(),
            BrightDataType.TimeOnly                                            => GetTypeAndSize<TimeOnly>(),
            BrightDataType.DateOnly                                            => GetTypeAndSize<DateOnly>(),
            _                                                                  => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };
    }
}
