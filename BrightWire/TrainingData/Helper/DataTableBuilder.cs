using BrightData;
using BrightData.DataTable.Builders;

namespace BrightWire.TrainingData.Helper
{
    public static class DataTableBuilder
    {
        public static InMemoryTableBuilder CreateTwoColumnMatrixTableBuilder(this IBrightDataContext context)
        {
            var ret = context.BuildTable();
            ret.AddColumn(ColumnType.Matrix, "Input");
            ret.AddColumn(ColumnType.Matrix, "Target").SetTarget(true);
            return ret;
        }

        public static InMemoryTableBuilder CreateTwoColumnVectorTableBuilder(this IBrightDataContext context)
        {
            var ret = context.BuildTable();
            ret.AddColumn(ColumnType.Vector, "Input");
            ret.AddColumn(ColumnType.Vector, "Target").SetTarget(true);
            return ret;
        }

        public static InMemoryTableBuilder Create3DTensorToVectorTableBuilder(this IBrightDataContext context)
        {
            var ret = context.BuildTable();
            ret.AddColumn(ColumnType.Tensor3D, "Input");
            ret.AddColumn(ColumnType.Vector, "Target").SetTarget(true);
            return ret;
        }
    }
}
