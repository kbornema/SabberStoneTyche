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

			//it's a buggy state, mostly related to equipping/using weapons on heroes etc.
			//in this case use the old state and estimate the new state manually:
			if (child == null)
			{
				myState = TyState.FromSimulatedGame(parent, parent.CurrentPlayer, task);
				enemyState = TyState.FromSimulatedGame(parent, parent.CurrentOpponent, null);

				//if the correction failes, assume the task is x% better/worse:
				if (!TyState.CorrectBuggySimulation(myState, enemyState, parent, task))
					valueFactor = 1.25f;
			}

			else
			{
				myState = TyState.FromSimulatedGame(child, child.CurrentPlayer, task);
				enemyState = TyState.FromSimulatedGame(child, child.CurrentOpponent, null);

				//after END_TURN the players will be swapped for the simlated resultState:
				if (task.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					TyState tmpState = myState;
					myState = enemyState;
					enemyState = tmpState;
				}
			}

			return analyzer.GetStateValue(myState, enemyState) * valueFactor;
		}
	}
}
