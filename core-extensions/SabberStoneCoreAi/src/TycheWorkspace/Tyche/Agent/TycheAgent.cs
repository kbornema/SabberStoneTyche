﻿using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using System;
using System.Collections.Generic;

namespace SabberStoneCoreAi.Tyche
{
	/// <summary> An <see cref="AbstractAgent"/> that simulates each possible <see cref="PlayerTask"/> (only one step deep), and chooses the best <see cref="PlayerTask"/> according to <see cref="TyStateAnalyzer"/>, </summary>
	class TycheAgent : AbstractAgent
	{
		private Random _random;

		private TyStateAnalyzer _analyzer;
		public TyStateAnalyzer Analyzer { get { return _analyzer; } }

		private bool _hasInitialized;
		private POGame.POGame _initialState;
		private bool _heroBasedWeights;

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
					TyState.EstimateBuggySimulation(myState, enemyState, poGame, choosenOption);
					//no need to swap states here (like below), since the swap did NOT already occur in the old poGame:
				}

				else
				{
					myState = TyState.FromSimulatedGame(resultState, resultState.CurrentPlayer);
					enemyState = TyState.FromSimulatedGame(resultState, resultState.CurrentOpponent);

					//after END_TURN the players will be swapped for the sumlated resultState:
					if (choosenOption.PlayerTaskType == PlayerTaskType.END_TURN)
					{
						TyState tmpState = myState;
						myState = enemyState;
						enemyState = tmpState;
					}
				}

				float stateValue = _analyzer.GetStateValue(myState, enemyState);
				
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


			//could not find a best state, either all of them are buggy or they are losing states:
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

		private PlayerTask GetRandomTaskNonTurnEnd(POGame.POGame poGame, List<PlayerTask> options)
		{
			if (options.Count == 1)
				return options[0];

			int curId = _random.Next(options.Count);
			PlayerTask curTask = options[curId];

			if (curTask.PlayerTaskType == PlayerTaskType.END_TURN)
				options.RemoveAt(curId);
			else
				return curTask;

			return GetRandomTaskNonTurnEnd(poGame, options);
		}

		public override PlayerTask GetMove(POGame.POGame poGame)
		{
			if (!_hasInitialized)
				CustomInit(poGame);

			return GetGreedyBestTask(poGame);
		}

		private void CustomInit(POGame.POGame initialState)
		{
			_hasInitialized = true;
			_initialState = initialState;
			_random = new Random();

			if (_heroBasedWeights)
				_analyzer.Weights = TyStateWeights.GetHeroBased(_initialState.CurrentPlayer.HeroClass, _initialState.CurrentOpponent.HeroClass);
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
	}
}