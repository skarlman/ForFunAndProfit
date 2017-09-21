
// More info at http://www.sortingbits.net/

using System;
using System.Net;
using System.Threading.Tasks;
using SharedUtils;
using SharedUtils.EventStore;
using SharedUtils.OutputWriters;

namespace TheScraper
{
    class Program
    {
        private static EventStoreHandler _eventstoreHandler;

        static void Main(string[] args)
        {
            Console.Title = "BitFinex Ticker scraper";

            ConnectToEventstore();

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {

                    #region Download ticker

                    var jsonTicker = await DownloadBitFinexTicker();
                    OutputWriter.WriteLine(jsonTicker);

                    #endregion

                    #region Store ticker in Eventstore

                    await WriteTickerToEventstore(jsonTicker, GlobalConfig.TickerStreamName);

                    #endregion

                    OutputWriter.WriteLine();
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }
            );

            Console.ReadLine();
        }
        

        private static async Task WriteTickerToEventstore(string jsonTicker, string tickerStreamName)
        {
            #region Discard empty data

            if (string.IsNullOrEmpty(jsonTicker))
            {
                OutputWriter.WriteLine(ConsoleColor.DarkRed, "Receieved null ticker, nothing to store");
                return;
            }

            #endregion

            try
            {

               await _eventstoreHandler.WriteJson(tickerStreamName, jsonTicker, "BitFinexTicker", metadata: null);

               OutputWriter.WriteLine(ConsoleColor.Cyan, $"Wrote ticker to Eventstore stream {tickerStreamName}");

            }
            catch (Exception ex)
            {
               OutputWriter.WriteLine(ConsoleColor.Red, $"Failed to write ticker to Eventstore: {ex.Message}");
            }
        }

        private static async Task<string> DownloadBitFinexTicker()
        {
            var tickerUrl = "https://api.bitfinex.com/v1/pubticker/BTCUSD";
            
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var result = await wc.DownloadStringTaskAsync(tickerUrl);

                    OutputWriter.WriteLine(ConsoleColor.Green, "Ticker retrieved {0} ***", DateTime.Now);

                    return result;
                }
            }
            catch (Exception ex)
            {
                OutputWriter.WriteLine(ConsoleColor.Red, "!! Outer exception: {0}", ex.Message);
            }

            return null;
        }

        private static void ConnectToEventstore()
        {
            OutputWriter.WriteLine(ConsoleColor.Yellow, "Connecting to eventstore...");

            _eventstoreHandler = new EventStoreHandler(GlobalConfig.EventstoreEndpoint);

            _eventstoreHandler.Connect();

            OutputWriter.WriteLine(ConsoleColor.Green, "Yay, I am connected!");
        }

    }
}
