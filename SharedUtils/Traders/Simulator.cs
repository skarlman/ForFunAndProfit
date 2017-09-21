using System;
using System.Threading;
using SharedUtils.DTO;
using SharedUtils.Interfaces;
using SharedUtils.OutputWriters;

namespace SharedUtils.Traders
{
    public class Simulator
    {
        //TODO: Replace this with trading API integration

        private IPredictionMachine _predictionMachine;
        private readonly object _predictionMachineUpdateLock = new object();

        public Simulator()
        {
            
        }
        public Simulator(IPredictionMachine predictionMachine)
        {
            lock (_predictionMachineUpdateLock)
            {
                _predictionMachine = predictionMachine;
            }
        }

        public void UpdatePredictionMachine(IPredictionMachine predictionMachine)
        {
            lock (_predictionMachineUpdateLock)
            {
                _predictionMachine = predictionMachine;
            }
        }

        public void NewTickerArrived(Ticker newTicker)
        {
            double? nextPricePrediction;
            double currentPrice;

            //Make prediction
            lock (_predictionMachineUpdateLock)
            {
                currentPrice = newTicker[_predictionMachine.Config.PredictedColumnName];

                nextPricePrediction = _predictionMachine.GetPredicionOfNextPrice(newTicker);
            }
            

            //Are all input nodes filled?
            if (!nextPricePrediction.HasValue)
            {
                OutputWriter.WriteLine(ConsoleColor.DarkGray, $"Ticker received {currentPrice}. Not enough tickers yet to make predictions..");
                return;
            }
            

            //See if we were correct
            SeeIfPredictionWasCorrect(currentPrice);

            _previousPrediction = nextPricePrediction.Value;
            _previousPrice = currentPrice;

        }

        private void SeeIfPredictionWasCorrect(double currentPrice)
        {
            var predictedThatPriceWillGoUp = _previousPrediction > _previousPrice;
            var priceHasGoneUp = currentPrice > _previousPrice;


            bool predictedCorrectly = (predictedThatPriceWillGoUp && priceHasGoneUp) ||
                                      (!predictedThatPriceWillGoUp && !priceHasGoneUp);


            if (IsFirstRun())
            {
                return;
            }

            Interlocked.Increment(ref _totalPredictions);

            if (predictedCorrectly)
            {
                Interlocked.Increment(ref _correctPredictions);

                OutputWriter.WriteLine(ConsoleColor.Green,
                    $"Ticker received! Price {currentPrice}. (Predicted price: {_previousPrediction}) Predicted trend CORRECT");
            }
            else
            {
                OutputWriter.WriteLine(ConsoleColor.Red,
                    $"Ticker received! Price {currentPrice}. (Predicted price: {_previousPrediction}) Predicted trend INCORRECT");
            }

            OutputWriter.WriteLine($"Correct predictions: {(_correctPredictions / (double)_totalPredictions).ToString("P")}");
        }

        private bool IsFirstRun()
        {
            return Math.Abs(_previousPrediction - int.MinValue) < 1;
        }

        private int _totalPredictions;
        private double _previousPrediction = int.MinValue;
        private int _correctPredictions;
        private double _previousPrice;
    }
}