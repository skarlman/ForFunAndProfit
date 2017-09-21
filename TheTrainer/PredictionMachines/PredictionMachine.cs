using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Encog.ML;
using Encog.ML.Data.Basic;
using Encog.ML.Data.Versatile;
using Encog.Util.Arrayutil;
using SharedUtils.DTO;
using SharedUtils.Interfaces;

namespace TheTrainer.PredictionMachines
{
    [Serializable]
    public class PredictionMachine : IPredictionMachine
    {
        public IMLRegression NeuralNet { get; set; }
        public NormalizationHelper _normalizationHelper { get; set; }

        public PredictionConfig Config { get; set; }


        public PredictionMachine(IMLRegression neuralNet, NormalizationHelper normalizationHelper, PredictionConfig config)
        {
            NeuralNet = neuralNet;
            _normalizationHelper = normalizationHelper;
            Config = config;
        }


        public double? GetPredicionOfNextPrice(Ticker incomingTicker)
        {
            var inputValues = ExtractOnlyValuesFromTicker(Config.InputColumnNames, incomingTicker);

            //Normalize
            var normalizedInputValues = new double[Config.InputColumnNames.Count];
            _normalizationHelper.NormalizeInputVector(
                                                        line: inputValues,
                                                        data: normalizedInputValues,
                                                        originalOrder: true
                                                      );

            //Add to input window
            _previousTickersWindow.Add(normalizedInputValues);

            //Do we have enough input tickers?
            if (!_previousTickersWindow.IsReady())
            {
                return null;
            }

            //Feed input nodes with the ticker values
            var inputNodesData = _normalizationHelper.AllocateInputVector(Config.NumberOfPreviousTickers);
            _previousTickersWindow.CopyWindow(((BasicMLData) inputNodesData).Data, 0);


            //make prediction
            var normalizedPrediction = NeuralNet.Compute(inputNodesData);


            //Denormalize
            var denormalizedPrediction = _normalizationHelper
                .DenormalizeOutputVectorToString(normalizedPrediction)
                .Select(double.Parse)
                .Single();


            return denormalizedPrediction;
        }

        private static string[] ExtractOnlyValuesFromTicker(List<string> inputColumnNames, Ticker inputTicker)
        {
            return inputColumnNames.Select(v => inputTicker[v].ToString(CultureInfo.InvariantCulture)).ToArray();
        }

        private VectorWindow _previousTickersWindow
        {
            get
            {
                if (_inputDataHistory == null)
                {
                    _inputDataHistory = new VectorWindow(Config.NumberOfPreviousTickers);
                }
                return _inputDataHistory;
            }
        }

        [NonSerialized]
        private VectorWindow _inputDataHistory;
    }
}