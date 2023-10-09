using BrightData;
using BrightWire;

namespace ExampleCode.DataTableTrainers
{
    internal class StockDataTrainer : DataTableTrainer
    {
        public StockDataTrainer(IDataTable table) : base(table)
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
