
// More info at http://www.sortingbits.net/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SharedUtils;
using SharedUtils.DTO;
using SharedUtils.EventStore;
using SharedUtils.OutputWriters;
using SharedUtils.Traders;
using TheTrainer.Helpers;
using TheTrainer.PredictionMachines;

namespace TheTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Ticker> trainingTickers = GetLastTickersFromEventstore(tickersToFetch: 4000);

            #region Train prediction network

            var predictionConfig = new PredictionConfig()
            {
                NumberOfPreviousTickers = 10, //input nodes
                PredictedColumnName = "last_price", //output node
                InputColumnNames = trainingTickers.First().Keys.ToList() //input columns
            };

            var factory = new PredictionMachineFactory(predictionConfig, trainingTickers);

            var predictionMachine = factory.CreateAndTrainMachine();

            #endregion
            
            TestPredictionsInSimulator(predictionMachine, trainingTickers);
            
            #region Save predictionMachine in Eventstore

            var serializedPredictionMachine = SerializationHelper.SerializeBot(predictionMachine);

            using (var eventStoreHandler = new EventStoreHandler(GlobalConfig.EventstoreEndpoint))
            {
                eventStoreHandler.Connect();
                eventStoreHandler.WriteBytes(
                    streamId: GlobalConfig.PredictionMachinesStreamName,
                    data: serializedPredictionMachine, datatype: predictionMachine.GetType().FullName,
                    metadata: Encoding.UTF8.GetBytes(""),
                    isJson: false
                    ).Wait();
            }

            #endregion

            OutputWriter.WriteLine();
            OutputWriter.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void TestPredictionsInSimulator(PredictionMachine predictionMachine, List<Ticker> trainingTickers,
            int numberOfTries = 100)
        {
            var simulator = new Simulator(predictionMachine);

            for (int i = 0; i < numberOfTries; i++)
            {
                #region Stop if end of tickers is reached

                if (i >= trainingTickers.Count)
                {
                    break;
                }

                #endregion

                simulator.NewTickerArrived(trainingTickers[i]);
            }
        }


        private static List<Ticker> GetLastTickersFromEventstore(int tickersToFetch)
        {
            using (var eventStoreHandler = new EventStoreHandler(GlobalConfig.EventstoreEndpoint))
            {
                eventStoreHandler.Connect();

                var eventstoreEvents = eventStoreHandler.ReadLastEvents(GlobalConfig.TickerStreamName, tickersToFetch);
                var tickers = eventstoreEvents.Select(GetTickerFromEvent).ToList();

                return tickers;
            }
        }


        private static Ticker GetTickerFromEvent(EventstoreEvent rawTicker)
        {
            string json = Encoding.UTF8.GetString(rawTicker.Data);

            var ticker = JsonConvert.DeserializeObject<Ticker>(json);
            ticker.Remove("timestamp");

            return ticker;
        }
    }

}
