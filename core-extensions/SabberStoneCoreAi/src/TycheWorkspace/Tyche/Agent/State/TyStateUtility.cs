using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SabberStoneCoreAi.Tyche
{
	static class TyStateUtility
    {
		/// <summary> Returns N sorted simulated TySimResults for the given start state. </summary>
		public static List<TySimResult> GetSimulatedBestTasks(int numTasks, POGame.POGame game, TyStateAnalyzer analyzer)
		{
			return GetSimulatedBestTasks(numTasks, game, game.CurrentPlayer.Options(), analyzer);
		}

		/// <summary> Returns N sorted simulated TySimResults for the given start state. </summary>
		public static List<TySimResult> GetSimulatedBestTasks(int numTasks, POGame.POGame game, List<PlayerTask> options, TyStateAnalyzer analyzer)
		{
			return GetSortedBestTasks(numTasks, GetSimulatedGames(game, options, analyzer));
		}

		/// <summary> Returns the best 'numTasks' tasks. Note: will sort the given List of tasks! </summary>
		public static List<TySimResult> GetSortedBestTasks(int numTasks, List<TySimResult> taskStructs)
		{
			//take at least one task:
			if (numTasks <= 0)
				numTasks = 1;

			taskStructs.Sort((x, y) => y.value.CompareTo(x.value));
			return taskStructs.Take(numTasks).ToList();
		}

		public static TySimResult GetSimulatedGame(POGame.POGame parent, PlayerTask task, TyStateAnalyzer analyzer)
		{
			var simulatedState = parent.Simulate(new List<PlayerTask>() { task })[task];
			var stateValue = GetStateValue(parent, simulatedState, task, analyzer);
			return new TySimResult(simulatedState, task, stateValue);
		}

		/// <summary> Returns a list of simulated games with the given parameters. </summary>
		public static List<TySimResult> GetSimulatedGames(POGame.POGame parent, List<PlayerTask> options, TyStateAnalyzer analyzer)
		{
			List<TySimResult> stateTaskStructs = new List<TySimResult>();

			for (int i = 0; i < options.Count; i++)
				stateTaskStructs.Add(GetSimulatedGame(parent, options[i], analyzer));	

			return stateTaskStructs;
		}

		/// <summary> Estimates how good the given child state is. </summary>
		public static float GetStateValue(POGame.POGame parent, POGame.POGame child, PlayerTask task, TyStateAnalyzer analyzer)
		{
			float valueFactor = 1.0f;

			TyState myState = null;
			TyState enemyState = null;

			Controller player = null;
			Controller opponent = null;

			//it's a buggy state, mostly related to equipping/using weapons on heroes etc.
			//in this case use the old state and estimate the new state manually:
			if (child == null)
			{
				player = parent.CurrentPlayer;
				opponent = parent.CurrentOpponent;

				myState = TyState.FromSimulatedGame(parent, player, task);
				enemyState = TyState.FromSimulatedGame(parent, opponent, null);

				//if the correction failes, assume the task is x% better/worse:
				if (!TyState.CorrectBuggySimulation(myState, enemyState, parent, task))
					valueFactor = 1.25f;
			}

			else
			{
				player = child.CurrentPlayer;
				opponent = child.CurrentOpponent;

				//happens sometimes even with/without TURN_END, idk
				if (!analyzer.IsMyPlayer(player))
				{
					player = child.CurrentOpponent;
					opponent = child.CurrentPlayer;
				}
				
				myState = TyState.FromSimulatedGame(child, player, task);
				enemyState = TyState.FromSimulatedGame(child, opponent, null);
			}

			TyDebug.Assert(analyzer.IsMyPlayer(player));
			TyDebug.Assert(!analyzer.IsMyPlayer(opponent));
			return analyzer.GetStateValue(myState, enemyState, player, opponent, task) * valueFactor;
		}
	}
}
