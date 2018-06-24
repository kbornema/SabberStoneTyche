using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.Tyche.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Learning
{
    class LearnSetup
    {
		private const float MIN_WEIGHT = 0.0f;
		private const float MAX_WEIGHT = 10.0f;

		private const float MUTATION_DEVIATION = 2.0f;

		private const int POPULATION_SIZE = 8;
		private const int MATING_POOL_SIZE = 4;
		private const int CHILD_POOL_SIZE = 4;

		private const int ROUNDS = 100;
		private const int MATCHES_PER_ROUND = 1;

		private int _rounds;
		private int _matchesPerRound;

		private List<ParamLearner> _learners;

		private System.Random _random;

		private List<string> _generationFileLog;
		private List<string> _globalFileLog;

		public string FileName = "result";

		public LearnSetup()
		{
			Clear();
		}

		public void Clear()
		{
			_generationFileLog = new List<string>();
			_globalFileLog = new List<string>();

			_rounds = ROUNDS;
			_matchesPerRound = MATCHES_PER_ROUND;

			_random = new Random();

			_learners = new List<ParamLearner>();

			for (int i = 0; i < POPULATION_SIZE; i++)
			{
				var learner = new ParamLearner(_random, MIN_WEIGHT, MAX_WEIGHT);
				_learners.Add(learner);
			}
		}

		public void Run(int numSteps, List<DeckHeroPair> myDeck, List<DeckHeroPair> enemyDeck, List<AbstractAgent> enemyAgents)
		{
			var myDeckName = DeckHeroPair.GetDeckListPrint(myDeck);
			var enemyDeckName = DeckHeroPair.GetDeckListPrint(enemyDeck);

			FileName = myDeckName + "Vs" + enemyDeckName +"_"+ enemyAgents[0].GetType().Name;

			for (int step = 0; step < numSteps; step++)
			{
				var startDate = DateTime.Now;

				for (int learnerId = 0; learnerId < _learners.Count; learnerId++)
				{
					var curLearner = _learners[learnerId];
					curLearner.ResetStats();
					ComputeFitness(curLearner, myDeck, enemyDeck, enemyAgents);
				}

				var children = GiveBirth(SelectFittest(_learners), _random);

				for (int childId = 0; childId < children.Count; childId++)
				{
					var childLearner = children[childId];
					childLearner.Parameter.Clamp(MIN_WEIGHT, MAX_WEIGHT);

					childLearner.ResetStats();
					ComputeFitness(childLearner, myDeck, enemyDeck, enemyAgents);
				}

				_learners = MixPopulations(_learners, children);

				Log("Population " + (step));
				LogPopulation(_learners);
				var diff = DateTime.Now.Subtract(startDate);
				Log("Generation took " + diff.Minutes + " min, " + diff.Seconds + " s");
				WriteCurrentToFile(FileName + "_" + step.ToString("0000") + ".txt");
				_generationFileLog.Clear();
			}

			WriteGlobalToFile();
		}

		private void LogPopulation(List<ParamLearner> population)
		{
			for (int i = 0; i < _learners.Count; i++)
			{
				Log(i + " ( winRate: " + population[i].WinPercent + ")");
				Log("Params: " + population[i].Parameter.ToString());
			}
		}

		private void Log(string s)
		{
			_globalFileLog.Add(s);
			_generationFileLog.Add(s);
			Debug.LogInfo(s);
		}

		public void WriteGlobalToFile()
		{
			File.WriteAllLines(FileName + ".txt", _globalFileLog);
		}

		private void WriteCurrentToFile(string fileName)
		{
			File.WriteAllLines(fileName, _generationFileLog);
		}

		private List<ParamLearner> MixPopulations(List<ParamLearner> oldPopulation, List<ParamLearner> offspring)
		{
			List<ParamLearner> tmpPopulation = new List<ParamLearner>();
			tmpPopulation.AddRange(oldPopulation);
			tmpPopulation.AddRange(offspring);
			tmpPopulation.Sort(FittestSort);

			List<ParamLearner> newPopulaton = new List<ParamLearner>();

			for (int i = 0; i < POPULATION_SIZE; i++)
				newPopulaton.Add(tmpPopulation[i]);

			return newPopulaton;
		}

		private int FittestSort(ParamLearner x, ParamLearner y)
		{
			return y.WinPercent.CompareTo(x.WinPercent);
		}

		private List<ParamLearner> GiveBirth(List<ParamLearner> choosen, System.Random random)
		{
			List<ParamLearner> children = new List<ParamLearner>();

			for (int i = 0; i < CHILD_POOL_SIZE; i++)
			{
				List<ParamLearner> parentsToChoose = new List<ParamLearner>(choosen);

				int firstId = random.Next(parentsToChoose.Count);
				var first = parentsToChoose[firstId];
				parentsToChoose.RemoveAt(firstId);

				int secondId = random.Next(parentsToChoose.Count);
				var second = parentsToChoose[secondId];

				var childParams = ParamLearner.GetCrossedParams(first, second, random);
				childParams = ParamLearner.GetMutatedParams(childParams, random, MUTATION_DEVIATION);

				children.Add(new ParamLearner(childParams));
			}

			return children;
		}

		private List<ParamLearner> SelectFittest(List<ParamLearner> learners)
		{
			List<ParamLearner> copyLearners = new List<ParamLearner>(learners);

			//sort by fitness (aka win percent)
			copyLearners.Sort(FittestSort);

			List<ParamLearner> fittest = new List<ParamLearner>();

			for (int i = 0; i < MATING_POOL_SIZE; i++)
				fittest.Add(copyLearners[i]);

			return fittest;
		}

		private void ComputeFitness(ParamLearner learner, List<DeckHeroPair> myDeck, List<DeckHeroPair> enemyDeck, List<AbstractAgent> enemyAgents)
		{
			TycheAgent myAgent = new TycheAgent();
			myAgent.Analyzer.Parameter = learner.Parameter;

			var myAgentList = new List<AbstractAgent> { myAgent };

			MatchSetup training = new MatchSetup(myAgentList, enemyAgents, false);
			training.RunRounds(myDeck, enemyDeck, _rounds, _matchesPerRound);

			learner.AddStats(training.TotalPlays, training.Agent0Wins);
		}
    }
}
