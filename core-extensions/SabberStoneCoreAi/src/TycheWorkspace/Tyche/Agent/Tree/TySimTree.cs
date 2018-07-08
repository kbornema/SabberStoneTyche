using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
    class TySimTree
    {
		public const float MAX_SIMULATION_TIME = 30.0f;

		private TyStateAnalyzer _analyzer;
		private POGame.POGame _rootGame;

		private Dictionary<PlayerTask, TyTaskNode> _nodesToEstimate = new Dictionary<PlayerTask, TyTaskNode>();

		//all nodes except EndTurn (no need to explore)
		private List<TyTaskNode> _explorableNodes = new List<TyTaskNode>();

		public TySimTree(POGame.POGame root, TyStateAnalyzer analyzer, List<PlayerTask> options)
		{
			_analyzer = analyzer;
			_rootGame = root;
	
			var initialResults = TyStateUtility.GetSimulatedGames(root, options, _analyzer);

			for (int i = 0; i < initialResults.Count; i++)
			{	
				var tmpResult = initialResults[i];
				var task = tmpResult.task;

				var node = new TyTaskNode(_analyzer, task, tmpResult.value);

				if (task.PlayerTaskType != PlayerTaskType.END_TURN)
					_explorableNodes.Add(node);

				_nodesToEstimate.Add(task, node);
			}
		}

		public void Print()
		{
			List<TyTaskNode> nodes = new List<TyTaskNode>(_nodesToEstimate.Values);

			for (int i = 0; i < nodes.Count; i++)
			{
				TyDebug.LogInfo(i + ": "+ nodes[i].GetAverage());
			}
		}

		
		/*
		private TyTaskNode GetSoftMaxNode(System.Random random)
		{
			float totalWeight = 0.0f;

			float min = float.PositiveInfinity;

			for (int i = 0; i < _explorableNodes.Count; i++)
			{
				float avg = _explorableNodes[i].GetAverage();

				if(avg < min)
					min = avg;
			}

			for (int i = 0; i < _explorableNodes.Count; i++)
				totalWeight += (_explorableNodes[i].GetAverage() + min);

			float rand = random.RandFloat();
			float lastChance = 0.0f;

			for (int i = 0; i < _explorableNodes.Count; i++)
			{
				float chance = (_explorableNodes[i].GetAverage() + min) / totalWeight;

				float accumChance = lastChance + chance;

				if (rand <= accumChance)
					return _explorableNodes[i];

				lastChance = accumChance;
			}

			TyDebug.LogError("SoftMAX WAS NULL with rand: " + rand);
			return _explorableNodes[_explorableNodes.Count - 1];
		}
		*/
		public void SimulateEpisode(System.Random random, int maxDepth, ref DateTime turnStartTime)
		{
			// TODO: balance exploraton and exploitation:
			var nodeToExlore = _explorableNodes.GetUniformRandom(random);

			//should not be possible:
			if (nodeToExlore == null)
				return;

			var task = nodeToExlore.Task;

			var result = TyStateUtility.GetSimulatedGame(_rootGame, task, _analyzer);
			nodeToExlore.Explore(result, random, maxDepth, ref turnStartTime);
		}

		public PlayerTask GetBestNode()
		{	
			List<TyTaskNode> nodes = new List<TyTaskNode>(_nodesToEstimate.Values);
			nodes.Sort((x, y) => y.GetAverage().CompareTo(x.GetAverage()));
			return nodes[0].Task;
		}
	}
}
