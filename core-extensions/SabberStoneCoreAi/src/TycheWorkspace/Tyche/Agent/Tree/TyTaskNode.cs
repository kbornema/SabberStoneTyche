using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
	/// <summary> A unique node for a given PlayerTask. </summary>
    class TyTaskNode
    {
		private TyStateAnalyzer _analyzer;

		private PlayerTask _task;
		public PlayerTask Task { get { return _task; } }

		private float _totalValue;
		public float TotalValue { get { return _totalValue; } }

		private int _visits;
		public int Visits { get { return _visits; } }

		public TyTaskNode(TyStateAnalyzer analyzer, PlayerTask task, float totalValue)
		{
			_analyzer = analyzer;
			_task = task;
			_totalValue = totalValue;
			_visits = 1;
		}

		public void Explore(TySimResult simResult, System.Random random, int maxDepth, ref DateTime turnStartTime)
		{
			Explore(simResult, random, 0, maxDepth, ref turnStartTime);
		}

		private void Explore(TySimResult simResult, System.Random random, int depth, int maxDepth, ref DateTime turnStartTime)
		{
			bool stop = false;

			if (depth >= maxDepth)
			{
				stop = true;
			}

			if (simResult.state == null)
			{
				stop = true;
			}

			var timeSinceTurnStart = DateTime.Now.Subtract(turnStartTime);

			if (timeSinceTurnStart.TotalSeconds >= TySimTree.MAX_SIMULATION_TIME)
			{
				stop = true;
			}

			if(stop)
			{
				AddValue(simResult.value);
				return;
			}
			
			var game = simResult.state;
			var options = game.CurrentPlayer.Options();
			var task = options.GetUniformRandom(random);
			var childState = TyStateUtility.GetSimulatedGame(game, task, _analyzer);

			if (childState.task.PlayerTaskType != PlayerTaskType.END_TURN)
				Explore(childState, random, depth + 1, maxDepth, ref turnStartTime);
			
			else
				AddValue(simResult.value);
		}

		private void AddValue(float value)
		{
			_totalValue += value;
			_visits++;
		}

		public float GetAverage()
		{
			return _totalValue / _visits;
		}

	}
}
