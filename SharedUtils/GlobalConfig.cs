using System.Net;

namespace SharedUtils
{
    public static class GlobalConfig
    {
        //TODO: Replace with config file or something
        public static IPEndPoint EventstoreEndpoint => new IPEndPoint(IPAddress.Loopback, 1113);
        public static string TickerStreamName => "BitFinexTickers";

        public static string CsvTickersFilename => "tempTickerData.csv";
        public static string PredictionMachinesStreamName => "PredictionMachines";
    }
}
