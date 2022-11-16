/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface BagTableRequest {
  /** @format int32 */
  rowCount: number;
}

export enum BrightDataType {
  Unknown = "Unknown",
  Boolean = "Boolean",
  SByte = "SByte",
  Short = "Short",
  Int = "Int",
  Long = "Long",
  Float = "Float",
  Double = "Double",
  Decimal = "Decimal",
  String = "String",
  Date = "Date",
  IndexList = "IndexList",
  WeightedIndexList = "WeightedIndexList",
  Vector = "Vector",
  Matrix = "Matrix",
  Tensor3D = "Tensor3D",
  Tensor4D = "Tensor4D",
  BinaryData = "BinaryData",
  TimeOnly = "TimeOnly",
  DateOnly = "DateOnly",
}

export enum ColumnConversionType {
  Unchanged = "Unchanged",
  ToBoolean = "ToBoolean",
  ToDate = "ToDate",
  ToNumeric = "ToNumeric",
  ToString = "ToString",
  ToIndexList = "ToIndexList",
  ToWeightedIndexList = "ToWeightedIndexList",
  ToVector = "ToVector",
  ToCategoricalIndex = "ToCategoricalIndex",
  ToByte = "ToByte",
  ToShort = "ToShort",
  ToInt = "ToInt",
  ToLong = "ToLong",
  ToFloat = "ToFloat",
  ToDouble = "ToDouble",
  ToDecimal = "ToDecimal",
}

export interface ConvertDataTableColumnsRequest {
  columnIndices: number[];
  columnConversions: ColumnConversionType[];
}

export interface DataTableColumnModel {
  name: string;
  columnType: BrightDataType;
  isTarget?: boolean;
  metadata?: NameValueModel[] | null;
}

export interface DataTableColumnsRequest {
  columnIndices: number[];
}

export interface DataTableCsvPreviewRequest {
  hasHeader: boolean;
  delimiter: string;
  lines: string[];
}

export interface DataTableCsvRequest {
  hasHeader: boolean;
  delimiter: string;
  lines: string[];
  fileName: string;
  columnNames: string;

  /** @format int32 */
  targetIndex?: number | null;
}

export interface DataTableInfoModel {
  id: string;
  name: string;

  /** @format int32 */
  rowCount: number;

  /** @format date-time */
  dateCreated: string;
  metadata: NameValueModel[];
  columns: DataTableColumnModel[];
}

export interface DataTableListItemModel {
  id: string;
  name: string;

  /** @format int32 */
  rowCount: number;

  /** @format date-time */
  dateCreated: string;
}

export interface DataTablePreviewModel {
  columns?: DataTableColumnModel[] | null;
  previewRows?: string[][] | null;
}

export interface DataTableRowsRequest {
  rowRanges: RangeModel[];
}

export type EmptyTableRequest = object;

export interface NamedItemModel {
  id: string;
  name: string;
}

export interface NameValueModel {
  name: string;
  value: string;
}

export interface NewColumnFromExistingColumnsModel {
  columnIndices: number[];
  newType: BrightDataType;
  name: string;
}

export enum NormalizationType {
  None = "None",
  Standard = "Standard",
  Euclidean = "Euclidean",
  Manhattan = "Manhattan",
  FeatureScale = "FeatureScale",
}

export interface NormalizeDataTableColumnsRequest {
  columnIndices: number[];
  columns: NormalizationType[];
}

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;

  /** @format int32 */
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
}

export interface RangeModel {
  /** @format int32 */
  firstInclusiveRowIndex: number;

  /** @format int32 */
  lastExclusiveRowIndex: number;
}

export interface ReinterpretDataTableColumnsRequest {
  columns: NewColumnFromExistingColumnsModel[];
}

export interface RenameTableColumnsRequest {
  columnIndices: number[];
  columnsNames?: string[] | null;
}

export interface SetColumnTargetRequest {
  /** @format int32 */
  targetColumn?: number | null;
}

export interface SplitTableRequest {
  /** @format double */
  trainingPercentage: number;
}

export interface VectoriseDataTableColumnsRequest {
  columnIndices: number[];
  oneHotEncodeToMultipleColumns: boolean;
}
