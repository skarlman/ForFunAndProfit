
// More info at http://www.sortingbits.net/

using SharedUtils.EventStore;
using System;
using System.Text;
using SharedUtils;
using SharedUtils.OutputWriters;

using Newtonsoft.Json;
using SharedUtils.DTO;
using SharedUtils.Interfaces;
using SharedUtils.Traders;
using TheTrainer.Helpers;

namespace TheMoneyMaker
{
    class Program
    {
        private static Simulator _simulator = new Simulator();

        static void Main(string[] args)
        {
            
            EventstoreHandler.Connect();

            //Subscribe to new prediction machines
            EventstoreHandler.SubscribeToCatchupStream(GlobalConfig.PredictionMachinesStreamName, HandleNewPredictionMachine);

            //Subscribe to new tickers
            EventstoreHandler.SubscribeToStream(GlobalConfig.TickerStreamName, HandleNewTicker);


            OutputWriter.WriteLine("Awaiting tickers...");
            Console.ReadLine();

        }

        private static void HandleNewPredictionMachine(EventstoreEvent incomingEvent)
        {
            try
            {
                IPredictionMachine newPredictionMachine = SerializationHelper.DeserializeBot(incomingEvent.Data);
                _simulator.UpdatePredictionMachine(newPredictionMachine);

                OutputWriter.WriteLine(ConsoleColor.Cyan, "Got new PredictionMachine, updated!");

                PreloadPreviousTickersToWarmUp(EventstoreHandler);
            }
            catch(Exception ex)
            {
                OutputWriter.WriteLine(ConsoleColor.Red, $"Could not update prediction machine :(  {ex.Message}");
            }
        }

        private static void HandleNewTicker(EventstoreEvent incomingEvent)
        {
            Ticker ticker = ExtractTickerFromEvent(incomingEvent);

            _simulator.NewTickerArrived(ticker);
        }

        private static void PreloadPreviousTickersToWarmUp(EventStoreHandler eventstoreHandler, int tickersToLoad=9)
        {
            OutputWriter.WriteLine($"Preloading {tickersToLoad} last tickers to warm up");

            var previousTickers = eventstoreHandler.ReadLastEvents(GlobalConfig.TickerStreamName, tickersToLoad);
            previousTickers.ForEach(HandleNewTicker);
        }

        private static Ticker ExtractTickerFromEvent(EventstoreEvent incomingEvent)
        {
            var json = Encoding.UTF8.GetString(incomingEvent.Data);

            var ticker = JsonConvert.DeserializeObject<Ticker>(json);

            return ticker;
        }

        static readonly EventStoreHandler EventstoreHandler = new EventStoreHandler(GlobalConfig.EventstoreEndpoint);
    }
}
