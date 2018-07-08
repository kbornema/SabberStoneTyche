using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using System;

namespace SabberStoneCoreAi.Tyche
{
	class TycheAgent : AbstractAgent
	{	
		private TyStateAnalyzer _analyzer;
		private Random _random;

		private bool _isTurnBegin = true;
		private bool _hasInitialized;

		private bool _heroBasedWeights;

		public bool PrintTurnTime = false;
		public bool TrackMatchTime = false;

		private DateTime _matchTimeStart;
		private DateTime _turnTimeStart;

		public TycheAgent()
			: this(TyStateWeights.GetDefault(), true)
		{		
		}

		private TycheAgent(TyStateWeights weights, bool heroBasedWeights)
		{
			_analyzer = new TyStateAnalyzer(weights);
			_heroBasedWeights = heroBasedWeights;
		}

		public bool UseTree = false;

		public override PlayerTask GetMove(POGame.POGame poGame)
		{
			if (!_hasInitialized)
				CustomInit(poGame);

			if (_isTurnBegin)
				OnMyTurnBegin();

			PlayerTask choosenTask = null;

			var options = poGame.CurrentPlayer.Options();

			//if(PrintTurnTime)
			//{
			//	TyDebug.LogInfo("Options: " + options.Count);

			//	if(options.Count > 20)
			//	{
			//		for (int i = 0; i < options.Count; i++)
			//		{
			//			TyDebug.LogWarning(options[i].FullPrint());
			//		}
			//	}
			//}


			if (options.Count == 1)
				choosenTask = options[0];

			else
			{
				if (UseTree)
				{
					TySimTree t = new TySimTree(poGame, _analyzer, options);

					int numEpisodes = (int)((options.Count * options.Count) / 2);

					for (int i = 0; i < numEpisodes; i++)
						t.SimulateEpisode(_random, 999999, ref _turnTimeStart);

					choosenTask = t.GetBestNode();
				}

				else
				{
					var bestTasks = TyStateUtility.GetSimulatedBestTasks(1, poGame, options, _analyzer);
					choosenTask = bestTasks[0].task;
				}
			}

			if (choosenTask == null)
				choosenTask = options.GetUniformRandom(_random);

			if (choosenTask.PlayerTaskType == PlayerTaskType.END_TURN)
				OnMyTurnEnd();

			return choosenTask;
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

		public override void InitializeAgent() { }
		public override void FinalizeAgent() { }

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
	}
}
