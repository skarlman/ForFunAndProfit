using System.Collections.Generic;
using Encog;
using Encog.MathUtil.Error;
using Encog.ML;
using Encog.ML.Data.Versatile;
using Encog.ML.Factory;
using Encog.ML.Model;
using SharedUtils;
using SharedUtils.DTO;
using TheTrainer.Helpers;

namespace TheTrainer.PredictionMachines
{
    class PredictionMachineFactory
    {
        private readonly PredictionConfig _predictionConfig;

        public PredictionMachineFactory(PredictionConfig predictionConfig, List<Ticker> trainingTickers)
        {
            _predictionConfig = predictionConfig;
            CsvFileHelper.CreateCsvFile(trainingTickers, GlobalConfig.CsvTickersFilename);
        }

        public PredictionMachine CreateAndTrainMachine()
        {

            VersatileMLDataSet trainingDataSet = CsvFileHelper.LoadDataSetFromCsv(GlobalConfig.CsvTickersFilename);

            #region Configure Dataset

            trainingDataSet.ConfigureDatacolumnsToUse(
                inputColumns: _predictionConfig.InputColumnNames, 
                outputColumnName: _predictionConfig.PredictedColumnName
                );

            //Number of Input nodes
            trainingDataSet.LagWindowSize = _predictionConfig.NumberOfPreviousTickers;

            //Number of output nodes
            trainingDataSet.LeadWindowSize = 1;

            trainingDataSet.Analyze(); //Encog does some magic here....

            #endregion

            
            #region Train new neural net

            var trainer = new EncogModel(trainingDataSet);
            //Choose network structure
            trainer.SelectMethod(trainingDataSet, MLMethodFactory.TypeFeedforward);

            trainingDataSet.Normalize();
            
            trainer.HoldBackValidation(
                validationPercent: 0.3, 
                shuffle: false, 
                seed: TOTALLY_RANDOM_NUMBER);


            trainer.SelectTrainingType(trainingDataSet); //More magic by encog

            trainer.Report = new ConsoleStatusReportable();

            //Create and train the network (this is where the magic happens)
            var trainedNeuralNet = (IMLRegression) trainer
                                    .Crossvalidate(k: 5, shuffle: false);

            #endregion

            //Package into a prediction machine
            var predictionMachine = new PredictionMachine(
                neuralNet: trainedNeuralNet,
                normalizationHelper: trainingDataSet.NormHelper,
                config: _predictionConfig);

            return predictionMachine;
        }

       

        private const int TOTALLY_RANDOM_NUMBER = 1001;

        private CsvFileHelper CsvFileHelper => new CsvFileHelper();
    }
}