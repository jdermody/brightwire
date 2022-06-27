using BrightData;
using BrightData.DataTable2;
using BrightWire;

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
                Table.CreateSequentialWindow(windowSize, 0, 1, 2, 3, 4)
            );
        }
    }
}
