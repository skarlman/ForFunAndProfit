using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Encog.ML.Data.Versatile;
using Encog.ML.Data.Versatile.Sources;
using Encog.Util.CSV;
using SharedUtils.DTO;

namespace TheTrainer.Helpers
{
    class CsvFileHelper
    {
        public void CreateCsvFile(List<Ticker> tickers, string dataFileName)
        {
            var csvData = tickers.Select(t => string.Join("\t", t.Values.Select(v => v.ToString(CultureInfo.InvariantCulture)))).ToList();

            var headers = string.Join("\t", tickers.First().Keys);
            csvData.Insert(0, headers);

            File.WriteAllLines(dataFileName, csvData);
        }

        public VersatileMLDataSet LoadDataSetFromCsv(string filename)
        {
            var csvFormatSpec = new CSVFormat('.', '\t');
            var dataSource = new CSVDataSource(filename, headers: true, format: csvFormatSpec);

            var dataSet = new VersatileMLDataSet(dataSource)
            {
                NormHelper = {Format = csvFormatSpec}
            };

            return dataSet;
        }
    }
}