using BrightData;
using BrightTable;
using BrightTable.Builders;

namespace BrightWire.TrainingData.Helper
{
    public static class DataTableBuilder
    {
        public static TableBuilder CreateTwoColumnMatrixTableBuilder(this IBrightDataContext context)
        {
            var ret = new TableBuilder(context);
            ret.AddColumn(ColumnType.Matrix, "Input");
            ret.AddColumn(ColumnType.Matrix, "Target").SetTargetColumn(true);
            return ret;
        }

        public static TableBuilder CreateTwoColumnVectorTableBuilder(this IBrightDataContext context)
        {
            var ret = new TableBuilder(context);
            ret.AddColumn(ColumnType.Vector, "Input");
            ret.AddColumn(ColumnType.Vector, "Target").SetTargetColumn(true);
            return ret;
        }

        public static TableBuilder Create3DTensorToVectorTableBuilder(this IBrightDataContext context)
        {
            var ret = new TableBuilder(context);
            ret.AddColumn(ColumnType.Tensor3D, "Input");
            ret.AddColumn(ColumnType.Vector, "Target").SetTargetColumn(true);
            return ret;
        }
    }
}
