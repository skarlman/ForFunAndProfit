using System;
using System.Collections.Generic;

namespace SharedUtils.DTO
{
    [Serializable]
    public class PredictionConfig
    {
        public int NumberOfPreviousTickers { get; set; }
        public string PredictedColumnName { get; set; }
        public List<string> InputColumnNames { get; set; }
    }
}