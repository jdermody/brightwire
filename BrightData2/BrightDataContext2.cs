using System.Collections.Concurrent;
using BrightData;
using BrightData.Helper;

namespace BrightData2
{
    public class BrightDataContext2
    {
        public BrightDataContext2(int? randomSeed = null)
        {
            IsStochastic = !randomSeed.HasValue;
            Random = randomSeed.HasValue
                ? new Random(randomSeed.Value)
                : new Random()
            ;
            NewComputationUnit = () => new ComputationUnit(this);
        }

        public bool IsStochastic { get; }
        public Random Random { get; private set; }
        public void ResetRandom(int? seed) => Random = seed.HasValue ? new Random(seed.Value) : new Random();
        public INotifyUser? UserNotifications { get; set; } = new ConsoleProgressNotification();
        public CancellationToken CancellationToken { get; set; } = default;
        public Func<ComputationUnit> NewComputationUnit { get; set; }
    }
}