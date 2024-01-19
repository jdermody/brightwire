namespace BrightData.DataTable.Columns
{
    /// <summary>
    /// Column type that maps to a range of indexed data
    /// </summary>
    /// <param name="StartIndex"></param>
    /// <param name="Size"></param>
    public record struct DataRangeColumnType(uint StartIndex, uint Size) : IHaveSize;

    /// <summary>
    /// Column type that maps to matrices
    /// </summary>
    /// <param name="StartIndex"></param>
    /// <param name="RowCount"></param>
    /// <param name="ColumnCount"></param>
    public record struct MatrixColumnType(uint StartIndex, uint RowCount, uint ColumnCount) : IHaveSize
    {
        /// <inheritdoc />
        public readonly uint Size => RowCount * ColumnCount;
    }

    /// <summary>
    /// Column type that maps to 3D tensors
    /// </summary>
    /// <param name="StartIndex"></param>
    /// <param name="Depth"></param>
    /// <param name="RowCount"></param>
    /// <param name="ColumnCount"></param>
    public record struct Tensor3DColumnType(uint StartIndex, uint Depth, uint RowCount, uint ColumnCount) : IHaveSize
    {
        /// <inheritdoc />
        public readonly uint Size => Depth * RowCount * ColumnCount;
    }

    /// <summary>
    /// Column type that maps to 4D tensors
    /// </summary>
    /// <param name="StartIndex"></param>
    /// <param name="Count"></param>
    /// <param name="Depth"></param>
    /// <param name="RowCount"></param>
    /// <param name="ColumnCount"></param>
    public record struct Tensor4DColumnType(uint StartIndex, uint Count, uint Depth, uint RowCount, uint ColumnCount) : IHaveSize
    {
        /// <inheritdoc />
        public readonly uint Size => Count * Depth * RowCount * ColumnCount;
    }
}
