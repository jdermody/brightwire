using BrightData;
using BrightWire;
using BrightDataTable = BrightData.DataTable.BrightDataTable;

namespace ExampleCode.DataTableTrainers
{
    internal class StockDataTrainer : DataTableTrainer
    {
        public StockDataTrainer(BrightDataTable table) : base(table)
        {
        }

        public SequentialWindowStockDataTrainer GetSequentialWindow(uint windowSize = 14)
        {
            return new(
                Table.Value.CreateSequentialWindow(windowSize, 0, 1, 2, 3, 4)
            );
        }
    }
}
