﻿using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.POGame;
using System;
using System.Collections.Generic;

namespace SabberStoneCoreAi.Tyche
{
	class TycheAgent : AbstractAgent
	{
		public enum Algorithm { Greedy, SearchTree }

		private TyStateAnalyzer _analyzer;
		private TySimTree _simTree;
		private Random _random;

		private bool _isTurnBegin = true;
		private bool _hasInitialized;

		private bool _heroBasedWeights;

		private DateTime _matchTimeStart;

		private DateTime _turnTimeStart;
		public double TimeSinceTurnStart { get { return DateTime.Now.Subtract(_turnTimeStart).TotalSeconds; } }

		public bool PrintTurnTime = false;
		public bool TrackMatchTime = false;
		public Algorithm UsedAlgorithm = Algorithm.SearchTree;

		public TycheAgent()
			: this(TyStateWeights.GetDefault(), true)
		{		
		}

		private TycheAgent(TyStateWeights weights, bool heroBasedWeights)
		{
			_analyzer = new TyStateAnalyzer(weights);
			_heroBasedWeights = heroBasedWeights;
			_simTree = new TySimTree();
		}

		public override PlayerTask GetMove(POGame.POGame poGame)
		{
			if (!_hasInitialized)
				CustomInit(poGame);

			if (_isTurnBegin)
				OnMyTurnBegin();

			var options = poGame.CurrentPlayer.Options();

			PlayerTask choosenTask = ChooseTask(poGame, options);

			//should not happen, but if, just return anything:
			if (choosenTask == null)
				choosenTask = options.GetUniformRandom(_random);

			if (choosenTask.PlayerTaskType == PlayerTaskType.END_TURN)
				OnMyTurnEnd();

			return choosenTask;
		}

		private PlayerTask ChooseTask(POGame.POGame poGame, List<PlayerTask> options)
		{
			if (options.Count == 1)
				return options[0];

			else if (UsedAlgorithm == Algorithm.SearchTree)
				return GetSimulationTreeTask(poGame, options);

			else if(UsedAlgorithm == Algorithm.Greedy)
				return GetGreedyBestTask(poGame, options);

			else
				return null;
		}

		private PlayerTask GetSimulationTreeTask(POGame.POGame poGame, List<PlayerTask> options)
		{
			if (!IsAllowedToSimulate())
				return GetGreedyBestTask(poGame, options);

			_simTree.InitTree(_analyzer, poGame, options);

			//-1 because TurnEnd won't be looked at:
			int numEpisodes = (int)((options.Count - 1) * 100);

			for (int i = 0; i < numEpisodes; i++)
			{
				if (!IsAllowedToSimulate())
					break;

				_simTree.SimulateEpisode(_random, i, numEpisodes);
			}

			return _simTree.GetBestTask();
		}

		private PlayerTask GetGreedyBestTask(POGame.POGame poGame, List<PlayerTask> options)
		{
			var bestTasks = TyStateUtility.GetSimulatedBestTasks(1, poGame, options, _analyzer);
			return bestTasks[0].task;
		}

		/// <summary> False if there is not enough time left to do simulations. </summary>
		private bool IsAllowedToSimulate()
		{
			double t = TimeSinceTurnStart;

			if (t >= TyConst.MAX_SIMULATION_TIME)
			{
				if (TyConst.LOG_SIMULATION_TIME_BREAKS)
					TyDebug.LogWarning("Stopped simulations after " + t + " seconds");

				return false;
			}

			return true;
		}

		private void OnMyTurnBegin()
		{
			_isTurnBegin = false;
			_turnTimeStart = DateTime.Now;
		}

		private void OnMyTurnEnd()
		{
			_isTurnBegin = true;

			if (PrintTurnTime)
			{
				var diff = DateTime.Now.Subtract(_turnTimeStart);
				TyDebug.LogInfo("Turn took: " + diff.TotalSeconds);
			}
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

			if (TrackMatchTime)
				_matchTimeStart = DateTime.Now;
		}

		public override void FinalizeGame()
		{
			if (TrackMatchTime)
			{
				var diff = DateTime.Now.Subtract(_matchTimeStart);
				TyDebug.LogInfo("Match took: " + diff.TotalSeconds);
			}
		}

		/// <summary> Returns an agent that won't change its strategy based on the current game. Used for learning given weights. </summary>
		public static TycheAgent GetLearning(TyStateWeights weights)
		{
			return GetCustom(weights, false);
		}

		public static TycheAgent GetCustom(TyStateWeights weights, bool changeWeights)
		{
			return new TycheAgent(weights, changeWeights);
		}

		/// <summary> Use default weights and change weights as given. </summary>
		public static TycheAgent GetCustom(bool changeWeights)
		{
			return GetCustom(TyStateWeights.GetDefault(), changeWeights);
		}

		public override void InitializeAgent() { }
		public override void FinalizeAgent() { }
	}
}
