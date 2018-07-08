using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
    class TySimTree
    {	
		private TyStateAnalyzer _analyzer;
		private POGame.POGame _rootGame;

		private Dictionary<PlayerTask, TyTaskNode> _nodesToEstimate;
		private List<TyTaskNode> _explorableNodes;

		private DateTime _episodeStart;
		public double TimeSinceEpisodeStart { get { return DateTime.Now.Subtract(_episodeStart).TotalSeconds; } }

		public void InitTree(TyStateAnalyzer analyzer, POGame.POGame root, List<PlayerTask> options)
		{
			_analyzer = analyzer;
			_nodesToEstimate = new Dictionary<PlayerTask, TyTaskNode>();
			_explorableNodes = new List<TyTaskNode>();
			_rootGame = root;

			var initialResults = TyStateUtility.GetSimulatedGames(root, options, _analyzer);

			for (int i = 0; i < initialResults.Count; i++)
			{
				var tmpResult = initialResults[i];
				var task = tmpResult.task;

				var node = new TyTaskNode(this, _analyzer, task, tmpResult.value);

				if (task.PlayerTaskType != PlayerTaskType.END_TURN)
					_explorableNodes.Add(node);

				_nodesToEstimate.Add(task, node);
			}
		}

		public void SimulateEpisode(System.Random random, int curEpisode, int totalEpisodes)
		{
			_episodeStart = DateTime.Now;

			//TODO: no need to estimate VERY bad nodes:
			TyTaskNode nodeToExlore = _explorableNodes[curEpisode % _explorableNodes.Count];

			//should not be possible:
			if (nodeToExlore == null)
				return;

			var task = nodeToExlore.Task;
			var result = TyStateUtility.GetSimulatedGame(_rootGame, task, _analyzer);
			nodeToExlore.Explore(result, random);
		}

		public TyTaskNode GetBestNode()
		{	
			List<TyTaskNode> nodes = new List<TyTaskNode>(_nodesToEstimate.Values);
			nodes.Sort((x, y) => y.GetAverage().CompareTo(x.GetAverage()));
			return nodes[0];
		}

		public PlayerTask GetBestTask()
		{
			return GetBestNode().Task;
		}
	}
}
