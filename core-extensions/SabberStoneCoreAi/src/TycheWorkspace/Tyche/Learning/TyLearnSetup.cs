using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.Tyche.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Learning
{
    class TyLearnSetup
    {
		/// <summary> Higher values make fitness more accurate. </summary>
		private const int NUM_TRAININGS = 2;
		private const float MIN_WEIGHT = 0.0f;
		private const float MAX_WEIGHT = 10.0f;
		
		public float MutationDeviation = 2.0f;
		public int PopulationSize = 10;
		public int MatingPoolSize = 4;
		public int OffspringSize = 4;

		public int Rounds = 100;
		public int MatchesPerRound = 1;
		public string FileName = "result";

		private System.Random _random;

		private List<TyWeightsLearner> _currentPopulation;
		private List<string> _globalFileLog;
		private int _individualId = 0;

		private CsvLog _csvLog;
		private CsvHelper _csvHelper;

		public TyLearnSetup()
		{
			Clear();
		}

		public void Clear()
		{
			_individualId = 0;
			_globalFileLog = new List<string>();

			_csvLog = new CsvLog();
			_csvHelper = new CsvHelper();
			_csvLog.AddCsvEntry(CsvHelper.GetCsvHeader());

			_random = new Random();
			_currentPopulation = new List<TyWeightsLearner>();

			for (int i = 0; i < PopulationSize; i++)
			{
				var learner = new TyWeightsLearner(_random, MIN_WEIGHT, MAX_WEIGHT, 0, _individualId);
				_individualId++;
				_currentPopulation.Add(learner);
			}
		}

		public void Run(int numGenerations, List<TyDeckHeroPair> myDeck, List<TyDeckHeroPair> enemyDeck, AbstractAgent enemyAgent)
		{
			var myDeckName = TyDeckHeroPair.GetDeckListPrint(myDeck);
			var enemyDeckName = TyDeckHeroPair.GetDeckListPrint(enemyDeck);
			FileName = myDeckName + "Vs" + enemyDeckName +"_"+ enemyAgent.GetType().Name;

			while(File.Exists(FileName + ".txt"))
				FileName += "0";

			WriteGlobalToFile();

			TyDebug.LogInfo(FileName);
			TyDebug.LogInfo("Generations: " + numGenerations);

			for (int step = 0; step < numGenerations; step++)
			{
				var startDate = DateTime.Now;
				
				Train(_currentPopulation, myDeck, enemyDeck, enemyAgent);
					
				var children = GiveBirth(SelectFittest(_currentPopulation), _random, step + 1);

				Train(children, myDeck, enemyDeck, enemyAgent);

				_currentPopulation = MixPopulations(_currentPopulation, children);

				Log("Generation " + (step));
				LogPopulation(_currentPopulation);
				var diff = DateTime.Now.Subtract(startDate);
				Log("Generation took " + diff.Minutes + " min, " + diff.Seconds + " s");
				WriteCurrentToFile(FileName + ".txt");
				_csvLog.WriteToFiles(FileName);
			}
		}

		private void Train(List<TyWeightsLearner> learners, List<TyDeckHeroPair> myDeck, List<TyDeckHeroPair> enemyDeck, AbstractAgent enemyAgent)
		{
			for (int i = 0; i < learners.Count; i++)
			{
				var childLearner = learners[i];
				childLearner.Weights.Clamp(MIN_WEIGHT, MAX_WEIGHT);

				for (int j = 0; j < NUM_TRAININGS; j++)
					ComputeFitness(childLearner, myDeck, enemyDeck, enemyAgent);
			}
		}

		private void LogPopulation(List<TyWeightsLearner> population)
		{
			for (int i = 0; i < _currentPopulation.Count; i++)
			{
				var curLearner = _currentPopulation[i];

				CsvLog(curLearner);

				Log("Id: " + curLearner.Id + " (born: " + curLearner.GenerationBorn + ", cur: " + curLearner.CurWinPercent + " (avg: " + curLearner.AverageWinPercent + "))");
				Log("Weights: " + curLearner.Weights.ToString());
			}
		}

		private void CsvLog(TyWeightsLearner curLearner)
		{	
			_csvHelper.SetColumn(CsvHelper.Column.Id, curLearner.Id);
			_csvHelper.SetColumn(CsvHelper.Column.Generation, curLearner.GenerationBorn);
			_csvHelper.SetColumn(CsvHelper.Column.NumPlays, curLearner.NumPlays);
			_csvHelper.SetColumn(CsvHelper.Column.Current, curLearner.CurWinPercent);
			_csvHelper.SetColumn(CsvHelper.Column.Average, curLearner.AverageWinPercent);
			
			for (int i = 0; i < (int)TyStateWeights.WeightType.Count; i++)
			{
				var curWeightType = (TyStateWeights.WeightType)i;
				var obj = Enum.Parse(typeof(CsvHelper.Column), curWeightType.ToString());
				var targetEnum = (CsvHelper.Column)obj;
				_csvHelper.SetColumn(targetEnum, curLearner.Weights.GetWeight(curWeightType));
			}

			_csvLog.AddCsvEntry(_csvHelper.GetLine());
		}

		private void Log(string s)
		{
			_globalFileLog.Add(s);
			//TyDebug.LogInfo(s);
		}

		public void WriteGlobalToFile()
		{
			File.WriteAllLines(FileName + ".txt", _globalFileLog);
		}

		private void WriteCurrentToFile(string fileName)
		{
			File.WriteAllLines(fileName, _globalFileLog);
		}

		private List<TyWeightsLearner> MixPopulations(List<TyWeightsLearner> oldPopulation, List<TyWeightsLearner> offspring)
		{
			List<TyWeightsLearner> tmpPopulation = new List<TyWeightsLearner>();
			tmpPopulation.AddRange(oldPopulation);
			tmpPopulation.AddRange(offspring);
			tmpPopulation.Sort(FittestSort);

			List<TyWeightsLearner> newPopulaton = new List<TyWeightsLearner>();

			for (int i = 0; i < PopulationSize; i++)
				newPopulaton.Add(tmpPopulation[i]);

			return newPopulaton;
		}

		/// <summary> Sort from biggest to smallest fitness </summary>
		private int FittestSort(TyWeightsLearner x, TyWeightsLearner y)
		{
			return y.Fitness.CompareTo(x.Fitness);
		}

		private List<TyWeightsLearner> GiveBirth(List<TyWeightsLearner> choosen, System.Random random, int generation)
		{
			List<TyWeightsLearner> children = new List<TyWeightsLearner>();

			for (int i = 0; i < OffspringSize; i++)
			{
				List<TyWeightsLearner> parentsToChoose = new List<TyWeightsLearner>(choosen);

				int firstId = random.Next(parentsToChoose.Count);
				var first = parentsToChoose[firstId];
				parentsToChoose.RemoveAt(firstId);

				int secondId = random.Next(parentsToChoose.Count);
				var second = parentsToChoose[secondId];

				var childWeights = TyWeightsLearner.GetCrossedWeights(first, second, random);
				childWeights = TyWeightsLearner.GetMutatedWeights(childWeights, random, MutationDeviation);

				children.Add(new TyWeightsLearner(childWeights, generation, _individualId));
				_individualId++;
			}

			return children;
		}

		private List<TyWeightsLearner> SelectFittest(List<TyWeightsLearner> learners)
		{
			List<TyWeightsLearner> copyLearners = new List<TyWeightsLearner>(learners);

			//sort by fitness (aka win percent)
			copyLearners.Sort(FittestSort);

			List<TyWeightsLearner> fittest = new List<TyWeightsLearner>();

			for (int i = 0; i < MatingPoolSize; i++)
				fittest.Add(copyLearners[i]);

			return fittest;
		}

		private void ComputeFitness(TyWeightsLearner learner, List<TyDeckHeroPair> myDeck, List<TyDeckHeroPair> enemyDeck, AbstractAgent enemyAgent)
		{
			learner.BeforeLearn();

			TyMatchSetup training = new TyMatchSetup(TycheAgent.GetLearningAgent(learner.Weights), enemyAgent);
			training.RunRounds(myDeck, enemyDeck, Rounds, MatchesPerRound);

			learner.AfterLearn(training.TotalPlays, training.Agent0Wins);
		}
    }
}
