using System;
using System.Collections.Generic;
using System.Text;
using BrightData;
using BrightTable;
using BrightTable.Builders;

namespace BrightWire.TrainingData.Helper
{
    public static class DataTableBuilder
    {
        public static TableBuilder CreateTwoColumnMatrix(IBrightDataContext context)
        {
            var ret = new TableBuilder(context);
            ret.AddColumn(ColumnType.Matrix, "Input");
            ret.AddColumn(ColumnType.Matrix, "Target");
            return ret;
        }
    }
}
