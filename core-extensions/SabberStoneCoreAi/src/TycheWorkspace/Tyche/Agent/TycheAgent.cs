using SabberStoneCore.Model;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.POGame;
using System;
using System.Collections.Generic;

namespace SabberStoneCoreAi.Tyche
{
	class TycheAgent : AbstractAgent
	{
		private const int MAX_EPISODES_FACTOR = 10;
		private const double EXPLORE_TRESHOLD = 0.75;

		private const int DEFAULT_NUM_EPISODES_MULTIPLIER = 100;
		private const int LEARNING_NUM_EPISODES_MULTIPLIER = 10;

		//TODO: create / choose a deck to play
		public static List<Card> GetUserCreatedDeck() { return null; }

		public enum Algorithm { Greedy, SearchTree }

		private TyStateAnalyzer _analyzer;
		private TySimTree _simTree;
		private Random _random;

		private bool _isTurnBegin = true;
		private bool _hasInitialized;

		private double _turnTimeStart;
		private bool _heroBasedWeights;
		private int _curEpisodeMultiplier;
		private int _defaultEpisodeMultiplier;
		
		public Algorithm UsedAlgorithm = Algorithm.SearchTree;
		public bool AdjustEpisodeMultiplier = false;
		public bool PrintTurnTime = false;

		public TycheAgent()
			: this(TyStateWeights.GetDefault(), true, DEFAULT_NUM_EPISODES_MULTIPLIER, true)
		{		
		}

		private TycheAgent(TyStateWeights weights, bool heroBasedWeights, int episodeMultiplier, bool adjustEpisodeMultiplier)
		{
			_defaultEpisodeMultiplier = episodeMultiplier;
			_curEpisodeMultiplier = episodeMultiplier;
			_heroBasedWeights = heroBasedWeights;

			_analyzer = new TyStateAnalyzer(weights);
			_simTree = new TySimTree();
			_random = new Random();

			AdjustEpisodeMultiplier = adjustEpisodeMultiplier;
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
			{
				if(TyConst.LOG_UNKNOWN_CORRECTIONS)
					TyDebug.LogError("Choosen task was null!");

				choosenTask = options.GetUniformRandom(_random);
			}

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
			double time = TyUtility.GetSecondsSinceStart() - _turnTimeStart;

			if (time >= TyConst.MAX_TURN_TIME)
			{
				if (TyConst.LOG_SIMULATION_TIME_BREAKS)
					TyDebug.LogError("Turn takes too long, fall back to greedy.");

				return GetGreedyBestTask(poGame, options);
			}

			_simTree.InitTree(_analyzer, poGame, options);

			//-1 because TurnEnd won't be looked at:
			int numEpisodes = (int)((options.Count - 1) * _curEpisodeMultiplier);

			double simStart = TyUtility.GetSecondsSinceStart();

			for (int i = 0; i < numEpisodes; i++)
			{
				if (!IsAllowedToSimulate(simStart, i, numEpisodes))
					break;

				bool shouldExploit = ((double)i / (double)numEpisodes) > EXPLORE_TRESHOLD;
				_simTree.SimulateEpisode(_random, i, shouldExploit);
			}

			return _simTree.GetBestTask();
		}

		private PlayerTask GetGreedyBestTask(POGame.POGame poGame, List<PlayerTask> options)
		{
			var bestTasks = TyStateUtility.GetSimulatedBestTasks(1, poGame, options, _analyzer);
			return bestTasks[0].task;
		}

		/// <summary> False if there is not enough time left to do simulations. </summary>
		private bool IsAllowedToSimulate(double startTime, int curEpisode, int maxEpisode)
		{
			double time = TyUtility.GetSecondsSinceStart() - startTime;

			if (time >= TyConst.MAX_SIMULATION_TIME)
			{	
				if (TyConst.LOG_SIMULATION_TIME_BREAKS)
					TyDebug.LogWarning("Stopped simulations after " + time.ToString("0.000") + "s and " + curEpisode + " of " + maxEpisode + " episodes.");

				return false;
			}

			return true;
		}

		private void OnMyTurnBegin()
		{
			_isTurnBegin = false;
			_turnTimeStart = TyUtility.GetSecondsSinceStart();
		}

		private void OnMyTurnEnd()
		{
			_isTurnBegin = true;

			var timeNeeded = TyUtility.GetSecondsSinceStart() - _turnTimeStart;

			if (AdjustEpisodeMultiplier && UsedAlgorithm == Algorithm.SearchTree)
			{
				double diff = Math.Min(TyConst.DECREASE_SIMULATION_TIME - timeNeeded, 5.0);
				double factor = 0.05;

				//reduce more if above the time limit:
				if(diff <= 0.0f)
					factor = 0.2;

				//increase/decrease depending on the time limit, also should be in interval [_defaultEpisodeMulitplier, -||- * 10]
				_curEpisodeMultiplier = Math.Clamp(_curEpisodeMultiplier + (int)(factor * diff * _defaultEpisodeMultiplier), _defaultEpisodeMultiplier, _defaultEpisodeMultiplier * 10);
			}

			if (PrintTurnTime)
				TyDebug.LogInfo("Turn took " + timeNeeded.ToString("0.000") + "s");
		}

		/// <summary> Called the first round (might be second round game wise) this agents is able to see the game and his opponent. </summary>
		private void CustomInit(POGame.POGame initialState)
		{
			_hasInitialized = true;

			if (_heroBasedWeights)
				_analyzer.Weights = TyStateWeights.GetHeroBased(initialState.CurrentPlayer.HeroClass, initialState.CurrentOpponent.HeroClass);
		}

		public override void InitializeGame()
		{
			_hasInitialized = false;
		}
		
		public static TycheAgent GetLearningAgent(TyStateWeights weights)
		{	
			return new TycheAgent(weights, false, LEARNING_NUM_EPISODES_MULTIPLIER, false);
		}

		public static TycheAgent GetTrainingAgent()
		{
			const bool ADJUST_EPISODES = false;
			const bool HERO_BASED_WEIGHTS = false;
			var agent =  new TycheAgent(TyStateWeights.GetDefault(), HERO_BASED_WEIGHTS, 0, ADJUST_EPISODES);
			agent.UsedAlgorithm = Algorithm.Greedy;
			return agent;
		}

		public override void InitializeAgent() { }
		public override void FinalizeAgent() { }
		public override void FinalizeGame() { } 
	}
}
