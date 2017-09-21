using System.Collections.Generic;
using System.Linq;
using Encog.ML.Data.Versatile;
using Encog.ML.Data.Versatile.Columns;

namespace TheTrainer.Helpers
{
    public static class DataSetExtensions
    {
        public static void ConfigureDatacolumnsToUse(this VersatileMLDataSet data, List<string> inputColumns, string outputColumnName)
        {
            var columnDefinitions = inputColumns.Select(c => data.DefineSourceColumn(c, ColumnType.Continuous)).ToList();

            columnDefinitions.ForEach(data.DefineInput);

            data.DefineOutput(columnDefinitions.Single(c => c.Name == outputColumnName));
        }
    }
}
