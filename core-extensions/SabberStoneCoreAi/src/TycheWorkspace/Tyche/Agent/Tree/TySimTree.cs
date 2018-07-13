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

		private Dictionary<PlayerTask, TyTaskNode> _nodesToEstimate = new Dictionary<PlayerTask, TyTaskNode>();
		private List<TyTaskNode> _explorableNodes = new List<TyTaskNode>();
		private List<TyTaskNode> _sortedNodes = new List<TyTaskNode>();

		public void InitTree(TyStateAnalyzer analyzer, POGame.POGame root, List<PlayerTask> options)
		{
			_sortedNodes.Clear();
			_explorableNodes.Clear();
			_nodesToEstimate.Clear();

			_analyzer = analyzer;
			_rootGame = root;

			//var initialResults = TyStateUtility.GetSimulatedGames(root, options, _analyzer);

			for (int i = 0; i < options.Count; i++)
			{
				var task = options[i];

				var node = new TyTaskNode(this, _analyzer, task, 0.0f);

				//end turn is pretty straight forward, should not really be looked at later in the simulations, just simulate once and keep the value:
				if (task.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					var sim = TyStateUtility.GetSimulatedGame(root, task, _analyzer);
					node.AddValue(sim.value);
				}
				else
				{
					_explorableNodes.Add(node);
					_sortedNodes.Add(node);
				}

				_nodesToEstimate.Add(task, node);
			}
		}

		public void SimulateEpisode(System.Random random, int curEpisode, bool shouldExploit)
		{
			TyTaskNode nodeToExlore = null;

			//exploiting:
			if (shouldExploit)
			{	
				_sortedNodes.Sort((x, y) => y.TotalValue.CompareTo(x.TotalValue));
				//exploit only 50% best nodes:
				int count = ((int)(_sortedNodes.Count * 0.5 + 0.5));
				nodeToExlore = _sortedNodes.GetUniformRandom(random, count);
			}

			//explore:
			else
				nodeToExlore = _explorableNodes[curEpisode % _explorableNodes.Count];

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
