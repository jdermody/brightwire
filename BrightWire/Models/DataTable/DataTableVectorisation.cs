using System;
using BrightWire.Models;
using System.Collections.Generic;
using System.Linq;
using BrightWire.LinearAlgebra.Helper;
using BrightTable;

namespace BrightWire.Models.DataTable
{
    /// <summary>
    /// A data table vectorisation model - maps rows in a data table to vectors
    /// </summary>
  //  public class DataTableVectorisation
  //  {
  //      // lazy initialised variables
  //      Dictionary<int, Dictionary<int, string>> _reverseColumnMap = null;
  //      Dictionary<int, Dictionary<string, int>> _columnMap = null;

  //      /// <summary>
  //      /// A categorical column value
  //      /// </summary>
  //      public class CategoricalIndex
  //      {
  //          /// <summary>
  //          /// The classification label
  //          /// </summary>
  //          public string Category { get; set; }

  //          /// <summary>
  //          /// The label's index
  //          /// </summary>
  //          public int Index { get; set; }
  //      }

  //      /// <summary>
  //      /// Column information
  //      /// </summary>
  //      public class Column
  //      {
  //          /// <summary>
  //          /// The column index
  //          /// </summary>
  //          public uint ColumnIndex { get; set; }

  //          /// <summary>
  //          /// Column name
  //          /// </summary>
  //          public string Name { get; set; }

  //          /// <summary>
  //          /// True if the column is the classification target
  //          /// </summary>
  //          public bool IsTargetColumn { get; set; }

  //          /// <summary>
  //          /// True if the column has a continuous value
  //          /// </summary>
  //          public bool IsContinuous { get; set; }

  //          /// <summary>
  //          /// The number of slots this column will fill in the output vector
  //          /// </summary>
  //          public int Size { get; set; }

  //          /// <summary>
  //          /// An array of categorial values
  //          /// </summary>
  //          public CategoricalIndex[] Values { get; set; }

	 //       /// <summary>
	 //       /// True if the column has one of two possible values
	 //       /// </summary>
	 //       public bool IsBinary { get; set; }
  //      }

  //      /// <summary>
  //      /// The columns in the table
  //      /// </summary>
  //      public Column[] Columns { get; set; }

  //      /// <summary>
  //      /// The size of each input vector that will be created
  //      /// </summary>
  //      public int InputSize { get; set; }

  //      /// <summary>
  //      /// The size of each output vector that will be created
  //      /// </summary>
  //      public int OutputSize { get; set; }

  //      /// <summary>
  //      /// True if the vectoriser has a classification target column
  //      /// </summary>
  //      public bool HasTarget { get; set; }

  //      /// <summary>
  //      /// True if the classification target column is continuous
  //      /// </summary>
  //      public bool IsTargetContinuous { get; set; }

  //      /// <summary>
  //      /// The column index of the classifiation target column
  //      /// </summary>
  //      public int ClassColumnIndex { get; set; }

	 //   /// <summary>
	 //   /// True if the classification target column has one of two possible values
	 //   /// </summary>
	 //   public bool IsTargetBinary { get; set; }

  //      /// <summary>
  //      /// A dictionary of column to categorical value tables
  //      /// </summary>
  //      public Dictionary<int, Dictionary<string, int>> ColumnMap
  //      {
  //          get
  //          {
  //              if(_columnMap == null) {
  //                  lock(this) {
  //                      if(_columnMap == null) {
  //                          _columnMap = Columns
  //                              .Where(c => c.Values != null)
  //                              .Select(c => (c.ColumnIndex, c.Values.ToDictionary(cv => cv.Category, cv => cv.Index)))
  //                              .ToDictionary(d => d.Item1, d => d.Item2)
  //                          ;
  //                      }
  //                  }
  //              }
  //              return _columnMap;
  //          }
  //      }

  //      /// <summary>
  //      /// A dictionary of column to reversed categorical value tables
  //      /// </summary>
  //      public Dictionary<int, Dictionary<int, string>> ReverseColumnMap
  //      {
  //          get
  //          {
  //              if (_reverseColumnMap == null) {
  //                  lock (this) {
  //                      if (_reverseColumnMap == null) {
  //                          _reverseColumnMap = Columns
  //                              .Where(c => c.Values != null)
  //                              .Select(c => (c.ColumnIndex, c.Values.ToDictionary(cv => cv.Index, cv => cv.Category)))
  //                              .ToDictionary(d => d.Item1, d => d.Item2)
  //                          ;
  //                      }
  //                  }
  //              }
  //              return _reverseColumnMap;
  //          }
  //      }

  //      /// <summary>
  //      /// Returns the classification label for the corresponding column/vector indices
  //      /// </summary>
  //      /// <param name="columnIndex">The data table column index</param>
  //      /// <param name="vectorIndex">The one hot vector index</param>
  //      /// <returns></returns>
  //      public string GetOutputLabel(int columnIndex, int vectorIndex)
  //      {
  //          if (HasTarget) {
  //              if (ReverseColumnMap.TryGetValue(columnIndex, out Dictionary<int, string> map)) {
  //                  if (map.TryGetValue(vectorIndex, out string ret))
  //                      return ret;
  //              }
  //          }
  //          return null;
  //      }

  //      /// <summary>
  //      /// Vectorises the input columns of the specified row
  //      /// </summary>
  //      /// <param name="row">>The row to vectorise</param>
  //      public FloatVector GetInput(IConvertibleRow row)
  //      {
  //          var ret = new float[InputSize];
  //          var index = 0;

  //          for (int i = 0, len = Columns.Length; i < len; i++) {
  //              var column = Columns[i];
  //              if (i == ClassColumnIndex)
  //                  continue;

	 //           var columnIndex = column.ColumnIndex;
	 //           if (column.IsBinary)
		//            ret[index++] = row.GetField<bool>(columnIndex) ? 1f : 0f;
  //              else if (column.IsContinuous)
  //                  ret[index++] = row.GetField<float>(columnIndex);
  //              else {
	 //               _WriteNonContinuous(columnIndex, row, index, ret);
	 //               index += column.Size;
  //              }
  //          }
  //          return new FloatVector {
  //              Data = ret
  //          };
  //      }

  //      /// <summary>
  //      /// Vectorises the output column of the specified row
  //      /// </summary>
  //      /// <param name="row">The row to vectorise</param>
  //      public FloatVector GetOutput(IRow row)
  //      {
  //          if (HasTarget) {
  //              var ret = new float[OutputSize];

	 //           if (IsTargetBinary)
		//            ret[0] = row.GetField<bool>(ClassColumnIndex) ? 1f : 0f;
	 //           else if (IsTargetContinuous)
  //                  ret[0] = row.GetField<float>(ClassColumnIndex);
  //              else
	 //               _WriteNonContinuous(ClassColumnIndex, row, 0, ret);

  //              return new FloatVector {
  //                  Data = ret
  //              };
  //          }
  //          return null;
  //      }

		///// <summary>
		///// Converts a float output vector to its original column type
		///// </summary>
		///// <param name="vector">Vector to convert</param>
		///// <param name="targetColumnType">Target column in the original data table</param>
	 //   public object ReverseOutput(FloatVector vector, ColumnType targetColumnType)
	 //   {
		//    if (HasTarget)
		//	    return _Reverse(vector, ClassColumnIndex, targetColumnType, IsTargetBinary, IsTargetContinuous);
		//	return vector;
	 //   }

	 //   object _Reverse(FloatVector vector, int columnIndex, ColumnType columnType, bool isBinary, bool isContinuous)
	 //   {
		//    if (isBinary)
		//	    return vector.Data[0] > 0.5f;

		//	if (isContinuous)
		//	    return vector.Data[0];

		//    if (columnType == ColumnType.IndexList) {
		//	    return new IndexList {
		//		    Index = vector.Data
		//			    .Select((v, i) => (v, (uint) i))
		//			    .Where(d => BoundMath.IsNotZero(d.Item1))
		//			    .Select(d => d.Item2)
		//			    .ToArray()
		//	    };
		//    }

		//    if (columnType == ColumnType.WeightedIndexList) {
		//	    return new WeightedIndexList {
		//		    IndexList = vector.Data
		//			    .Select((v, i) => (v, (uint) i))
		//			    .Where(d => BoundMath.IsNotZero(d.Item1))
		//			    .Select(d => new WeightedIndexList.WeightedIndex {
		//					Index = d.Item2,
		//					Weight = d.Item1
		//			    }).ToArray()
		//	    };
		//    }

		//    if (columnType == ColumnType.Vector)
		//	    return vector;

		//    if (ReverseColumnMap.TryGetValue(columnIndex, out var valueMap)) {
		//	    var index = vector.MaximumIndex();
		//		if(valueMap.TryGetValue(index, out var str))
		//			return str;
		//    }

		//    throw new NotImplementedException();
	 //   }

	 //   void _WriteNonContinuous(uint columnIndex, IConvertibleRow row, int offset, float[] buffer)
	 //   {
		//    var columnType = row.Table.Columns[columnIndex].Type;
		//    if (columnType == ColumnType.IndexList) {
		//	    var indexList = row.GetField<IndexList>(columnIndex);
		//	    foreach (var index in indexList.Index)
		//		    buffer[offset + index] = 1f;
		//    }else if (columnType == ColumnType.WeightedIndexList) {
		//		var weightedIndexList = row.GetField<WeightedIndexList>(columnIndex);
		//	    foreach (var index in weightedIndexList.IndexList)
		//		    buffer[offset + index.Index] = index.Weight;
		//    }else if (columnType == ColumnType.Vector) {
		//		var vector = row.GetField<FloatVector>(columnIndex);
		//	    Array.Copy(vector.Data, 0, buffer, offset, vector.Count);
		//    }else if (columnType == ColumnType.Matrix) {
		//	    var matrix = row.GetField<FloatMatrix>(columnIndex);
		//	    foreach (var vector in matrix.Row) {
		//		    Array.Copy(vector.Data, 0, buffer, offset, vector.Count);
		//		    offset += vector.Count;
		//	    }
		//    }else if (columnType == ColumnType.Tensor) {
		//	    var tensor = row.GetField<FloatTensor>(columnIndex);
		//	    foreach (var matrix in tensor.Matrix) {
		//		    foreach (var vector in matrix.Row) {
		//			    Array.Copy(vector.Data, 0, buffer, offset, vector.Count);
		//			    offset += vector.Count;
		//		    }
		//	    }
		//    }
		//    else {
		//		// one hot encode the output
		//	    var str = row.GetField<string>(columnIndex);
		//	    offset += ColumnMap[columnIndex][str];
		//	    buffer[offset] = 1f;
		//    }
	 //   }
  //  }1
}
