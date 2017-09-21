using SharedUtils.DTO;

namespace SharedUtils.Interfaces
{
    public interface IPredictionMachine
    {
        double? GetPredicionOfNextPrice(Ticker incomingTicker);

        PredictionConfig Config { get; set; }
    }
}