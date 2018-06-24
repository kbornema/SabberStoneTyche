using SabberStoneCore.Enums;
using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.BotC
{
	class MCTSTree
	{
		public int visits;
		public int wins;
		//public SabberStoneCore.Model.Game game;
		float ucb;
		public POGame.POGame poGame;
		public List<MCTSTree> childnodesList = new List<MCTSTree>();
		public MCTSTree parentnode = new MCTSTree();
		private Random Rnd = new Random();
		public MCTSTree()
		{
		}
		public MCTSTree(POGame.POGame getPOGame)
		{
			this.poGame = getPOGame;
		}
		public MCTSTree(int visits, int wins)
		{
			this.visits = visits;
			this.wins = wins;
			this.ucb = 0;

		}
		public MCTSTree GetRootNode()
		{
			MCTSTree node = this;
			while (node.parentnode != null) ;
			node = node.parentnode;
			return node;
		}
		public void Backpropagation(MCTSTree nodeToExplore, PlayState playoutResult)
		{
			while (nodeToExplore.parentnode != null)
			{
				nodeToExplore.visits++;
				nodeToExplore.wins++;
				nodeToExplore = nodeToExplore.parentnode;
			}
		}
		public MCTSTree GetPromisingNode(MCTSTree rootnode)
		{
			MCTSTree node = this;
			return node;
		}
		public void ExpandNode(MCTSTree nodetoexpand)
		{
			List<PlayerTask> options = nodetoexpand.poGame.CurrentPlayer.Options();
			foreach (PlayerTask option in options)
			{
				MCTSTree newchild = new MCTSTree();
				nodetoexpand.childnodesList.Add(newchild);
				newchild.parentnode = nodetoexpand;
			}
		}
		public MCTSTree GetRandomChild()
		{
			return this.childnodesList[Rnd.Next(childnodesList.Count)];
		}
		public MCTSTree GetChildWithMaxScore()
		{
			MCTSTree node = new MCTSTree();
			return node;
		}

	}
}
