using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using System;
using System.Collections.Generic;

namespace SabberStoneCoreAi.Tyche
{	
	class TycheAgent : AbstractAgent
	{
		private TyStateAnalyzer _analyzer;
		private Random _random;

		private bool _heroBasedWeights;
		private bool _hasInitialized;

		public TycheAgent()
			: this(TyStateWeights.GetDefault(), true)
		{		
		}

		private TycheAgent(TyStateWeights weights, bool heroBasedWeights)
		{
			_analyzer = new TyStateAnalyzer(weights);
			_heroBasedWeights = heroBasedWeights;
		}

		private PlayerTask GetGreedyBestTask(POGame.POGame poGame)
		{
			var options = poGame.CurrentPlayer.Options();

			//if there is only one option, then there is nothing to choose:
			if (options.Count == 1)
				return options[0];

			var simulationResults = poGame.Simulate(options);

			float bestStateValue = Single.NegativeInfinity;

			float taskFactor = 1.0f;
			List<PlayerTask> bestTasks = new List<PlayerTask>();
			HashSet<PlayerTask> isLosingTask = new HashSet<PlayerTask>();

			for (int i = 0; i < options.Count; i++)
			{
				var resultState = simulationResults[options[i]];
				var choosenOption = options[i];

				TyState myState = null; 
				TyState enemyState  = null;

				//it's a buggy state, mostly related to equipping/using weapons on heroes etc.
				//in this case use the old state and estimate the new state manually:
				if (resultState == null)
				{	
					myState = TyState.FromSimulatedGame(poGame, poGame.CurrentPlayer);
					enemyState = TyState.FromSimulatedGame(poGame, poGame.CurrentOpponent);

					//if the correction failes, assume the task is 25% better/worse:
					if (!TyState.CorrectBuggySimulation(myState, enemyState, poGame, choosenOption))
						taskFactor = 1.25f;
				}

				else
				{
					myState = TyState.FromSimulatedGame(resultState, resultState.CurrentPlayer);
					enemyState = TyState.FromSimulatedGame(resultState, resultState.CurrentOpponent);

					//after END_TURN the players will be swapped for the simlated resultState:
					if (choosenOption.PlayerTaskType == PlayerTaskType.END_TURN)
					{
						TyState tmpState = myState;
						myState = enemyState;
						enemyState = tmpState;
					}
				}

				float stateValue = _analyzer.GetStateValue(myState, enemyState) * taskFactor;
				
				//if the player wins, just choose this Task immediately:
				if (Single.IsPositiveInfinity(stateValue))
					return choosenOption;

				if (Single.IsNegativeInfinity(stateValue))
				{
					isLosingTask.Add(choosenOption);
					continue;
				}

				if (bestTasks.Count == 0 || stateValue >= bestStateValue)
				{
					if(stateValue > bestStateValue)
					{
						//if the new task is better, remove all the old best tasks:
						bestTasks.Clear();
						bestStateValue = stateValue;
					}

					bestTasks.Add(choosenOption);
				}
			}


			//TODO: is this case relevant anymore after correcting buggy tasks?
			if (bestTasks.Count == 0)
			{
				var tasksToChoose = new List<PlayerTask>(options);

				//remove all tasks where the player loses:
				tasksToChoose.RemoveAll(x => isLosingTask.Contains(x));

				//if there is no non-losing task left, choose any task
				if(tasksToChoose.Count == 0)
					return options.GetUniformRandom(_random);

				//prefer the tasks where the player doesn't lose:
				return tasksToChoose.GetUniformRandom(_random);
			}

			return bestTasks.GetUniformRandom(_random);
		}

		public override PlayerTask GetMove(POGame.POGame poGame)
		{
			if (!_hasInitialized)
				CustomInit(poGame);

			return GetGreedyBestTask(poGame);
		}

		/// <summary> Called the first round (might be second round game wise) this agents is able to see the game and his opponent. </summary>
		private void CustomInit(POGame.POGame initialState)
		{
			_hasInitialized = true;
			_random = new Random();

			if (_heroBasedWeights)
				_analyzer.Weights = TyStateWeights.GetHeroBased(initialState.CurrentPlayer.HeroClass, initialState.CurrentOpponent.HeroClass);
		}

		public override void InitializeGame()
		{
			_hasInitialized = false;
		}

		public override void InitializeAgent() { }
		public override void FinalizeAgent() { }
		public override void FinalizeGame() { }

		/// <summary> Returns an agent that won't change its strategy based on the current game. Used for learning given weights. </summary>
		public static TycheAgent GetLearning(TyStateWeights weights)
		{
			return GetCustom(weights, false);
		}

		public static TycheAgent GetCustom(TyStateWeights weights, bool changeWeights)
		{
			return new TycheAgent(weights, changeWeights);
		}

		public static TycheAgent GetCustom(bool changeWeights)
		{
			return GetCustom(TyStateWeights.GetDefault(), changeWeights);
		}
	}
}
