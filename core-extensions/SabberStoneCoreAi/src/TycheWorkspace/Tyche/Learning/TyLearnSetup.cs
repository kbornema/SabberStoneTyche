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
		private const float MIN_WEIGHT = 0.0f;
		private const float MAX_WEIGHT = 10.0f;
		
		public float MutationDeviation = 2.0f;
		public int PopulationSize = 10;
		public int MatingPoolSize = 4;
		public int OffspringSize = 4;

		public int Rounds = 10;
		public int MatchesPerRound = 5;
		public string FileName = "result";

		private System.Random _random;

		private List<TyWeightsLearner> _currentPopulation;

		private List<string> _globalFileLog;
		private int _individualId = 0;

		public TyLearnSetup()
		{
			Clear();
		}

		public void Clear()
		{
			_individualId = 0;
			_globalFileLog = new List<string>();

			_random = new Random();
			_currentPopulation = new List<TyWeightsLearner>();

			for (int i = 0; i < PopulationSize; i++)
			{
				var learner = new TyWeightsLearner(_random, MIN_WEIGHT, MAX_WEIGHT, 0, _individualId);
				_individualId++;
				_currentPopulation.Add(learner);
			}
		}

		public void Run(int numGenerations, List<TyDeckHeroPair> myDeck, List<TyDeckHeroPair> enemyDeck, List<AbstractAgent> enemyAgents)
		{
			var myDeckName = TyDeckHeroPair.GetDeckListPrint(myDeck);
			var enemyDeckName = TyDeckHeroPair.GetDeckListPrint(enemyDeck);
			FileName = myDeckName + "Vs" + enemyDeckName +"_"+ enemyAgents[0].GetType().Name;
			TyDebug.LogInfo(FileName);
			TyDebug.LogInfo("Generations: " + numGenerations);

			for (int step = 0; step < numGenerations; step++)
			{
				var startDate = DateTime.Now;

				for (int learnerId = 0; learnerId < _currentPopulation.Count; learnerId++)
				{
					var curLearner = _currentPopulation[learnerId];
					curLearner.ResetStats();
					ComputeFitness(curLearner, myDeck, enemyDeck, enemyAgents);
					
				}

				var children = GiveBirth(SelectFittest(_currentPopulation), _random, step + 1);

				for (int childId = 0; childId < children.Count; childId++)
				{
					var childLearner = children[childId];
					childLearner.Weights.Clamp(MIN_WEIGHT, MAX_WEIGHT);

					childLearner.ResetStats();
					ComputeFitness(childLearner, myDeck, enemyDeck, enemyAgents);
				}

				_currentPopulation = MixPopulations(_currentPopulation, children);

				Log("Generation " + (step));
				LogPopulation(_currentPopulation);
				var diff = DateTime.Now.Subtract(startDate);
				Log("Generation took " + diff.Minutes + " min, " + diff.Seconds + " s");
				WriteCurrentToFile(FileName + "_" + step.ToString("0000") + ".txt");
			}
		}

		private void LogPopulation(List<TyWeightsLearner> population)
		{
			for (int i = 0; i < _currentPopulation.Count; i++)
			{
				var curLearner = _currentPopulation[i];

				Log("Id: " + curLearner.Id + " (born: " + curLearner.GenerationBorn + ", winRate: " + curLearner.WinPercent + " (min: "+ curLearner.MinWinPercent + ", max: " + curLearner.MaxWinPercent + ", avg: " + curLearner.AverageWinPercent + "))");
				Log("Weights: " + curLearner.Weights.ToString());
			}
		}

		private void Log(string s)
		{
			_globalFileLog.Add(s);
			TyDebug.LogInfo(s);
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

		private int FittestSort(TyWeightsLearner x, TyWeightsLearner y)
		{
			return y.WinPercent.CompareTo(x.WinPercent);
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

		private void ComputeFitness(TyWeightsLearner learner, List<TyDeckHeroPair> myDeck, List<TyDeckHeroPair> enemyDeck, List<AbstractAgent> enemyAgents)
		{
			TycheAgent myAgent = TycheAgent.GetLearning(learner.Weights);

			var myAgentList = new List<AbstractAgent> { myAgent };

			TyMatchSetup training = new TyMatchSetup(myAgentList, enemyAgents, false);
			training.RunRounds(myDeck, enemyDeck, Rounds, MatchesPerRound);

			learner.AddStats(training.TotalPlays, training.Agent0Wins);
			learner.RememberWinPercent();
		}
    }
}
