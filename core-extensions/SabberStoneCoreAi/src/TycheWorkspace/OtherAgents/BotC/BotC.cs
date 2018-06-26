using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.BotC
{
    class BotC : AbstractAgent
    {
		private Random Rnd = new Random();
		public override void FinalizeAgent()
		{
		}
		public override void FinalizeGame()
		{
		}
		public override PlayerTask GetMove(SabberStoneCoreAi.POGame.POGame poGame)
		{
			List<PlayerTask> turnList = new List<PlayerTask>();
			PlayerTask option;
			Console.WriteLine("Current Player " + poGame.CurrentPlayer + " has " + poGame.CurrentPlayer.Options().Count + " options available ");
			foreach (PlayerTask option1 in poGame.CurrentPlayer.Options())
			{
				Console.WriteLine(option1.FullPrint() + " Source " + option1.Source + " Target " + option1.Target);
				if (option1.HasTarget && option1.Target.GetType() != null)
					Console.WriteLine("Target type" + option1.Target.GetType());
			}
			Console.WriteLine("Printing All Minions ");
			foreach (Minion minion in poGame.Minions)
				Console.WriteLine("Minion health is " + minion.Health);
			/*do
			{
				List<PlayerTask> options = poGame.CurrentPlayer.Options();
				option = options[Rnd.Next(options.Count)];
				turnList.Add(option);
			} while (option.PlayerTaskType != PlayerTaskType.END_TURN);*/
			//Split turnlist based on PlayerTaskType
			//List<PlayerTask> turnlist1 = new List<PlayerTask>();
			//Console.WriteLine("before splitting");
			//foreach (PlayerTask pt in turnList)
			//Console.WriteLine(pt);

			//Split the turnlist based on PlayerTaskType
			//Select PlayerTaskType=MinionAttack
			List<PlayerTask> MinionAttackTasks = new List<PlayerTask>();
			List<PlayerTask> PlayCardTasks = new List<PlayerTask>();
			List<PlayerTask> HeroAttackTasks = new List<PlayerTask>();
			List<PlayerTask> EndTurnTasks = new List<PlayerTask>();
			List<PlayerTask> OtherTasks = new List<PlayerTask>();
			foreach (PlayerTask pt in turnList)
			{
				if (pt.PlayerTaskType == PlayerTaskType.MINION_ATTACK)
					MinionAttackTasks.Add(pt);
				else if (pt.PlayerTaskType == PlayerTaskType.PLAY_CARD)
					PlayCardTasks.Add(pt);
				else if (pt.PlayerTaskType == PlayerTaskType.HERO_ATTACK)
					HeroAttackTasks.Add(pt);
				else if (pt.PlayerTaskType == PlayerTaskType.END_TURN)
					EndTurnTasks.Add(pt);
				else
					OtherTasks.Add(pt);
			}
			//Check if OpponentHero Can be killed
			if (TotalAttack(poGame) >= TotalOpponentHealth(poGame))
			{
				//if taunt exists, attackminion with taunt until all opponent minions with taunt are killed
				//attack opponent hero
			}

			//Evaluate opponent minion strength as Attack/Health and sort
			Minion[] oppMinions = poGame.CurrentOpponent.BoardZone.GetAll();
			Array.Sort(oppMinions, delegate (Minion m1, Minion m2)
			{
				float m1Quotient = (float)m1.AttackDamage / (float)m1.Health;
				float m2Quotient = (float)m2.AttackDamage / (float)m2.Health;
				return -(m1Quotient.CompareTo(m2Quotient));

			});
			Console.WriteLine("Opponent Minons are ");
			foreach (Minion m in oppMinions)
			{
				Console.WriteLine(m);
			}
			//Evaluate my minion strength as Attack/Health and sort
			Minion[] MyMinions = poGame.CurrentPlayer.BoardZone.GetAll();
			Array.Sort(MyMinions, delegate (Minion m1, Minion m2)
			{
				float m1Quotient = (float)m1.AttackDamage / (float)m1.Health;
				float m2Quotient = (float)m2.AttackDamage / (float)m2.Health;
				return (m1Quotient.CompareTo(m2Quotient));

			});
			if (oppMinions.Length > 0)
			{
				Minion MiniontoBeAttacked = oppMinions[0];
				//Find the minion good enough to kill the MinionToBeAttacked
				PlayerTask TaskThatKillsStrongestOpponentMinion = new PlayerTask();
				foreach (Minion m in MyMinions)
				{
					if (m.AttackDamage >= MiniontoBeAttacked.Health)
					{

						TaskThatKillsStrongestOpponentMinion = findOption(MinionAttackTasks, m, MiniontoBeAttacked);
						break;
						//foreach (PlayerTask pt in MinionAttackTasks)
						//{
						//	if (pt.Source == m && pt.Target == MiniontoBeAttacked)
						//		return pt;//option that
						//}
					}
				}
				if (TaskThatKillsStrongestOpponentMinion != null)
				{
					return TaskThatKillsStrongestOpponentMinion;
				}
			}
			return turnList[0];

			//return poGame.CurrentPlayer.Options()[0];     For Random move
		}
		//Find the option given Source and target of the attack.
		public PlayerTask findOption(List<PlayerTask> options, Minion source, Minion target)
		{
			return options.Find(delegate (PlayerTask p)
			{
				bool ma = p.PlayerTaskType == PlayerTaskType.MINION_ATTACK;
				bool src = p.HasSource && p.Source == source;
				bool trg = p.HasTarget && p.Target == target;
				return ma && src && trg;
			});
		}
		public int TotalAttack(SabberStoneCoreAi.POGame.POGame poGame)
		{
			int TotalAttack = 0;
			foreach (Minion m in poGame.CurrentPlayer.BoardZone.GetAll())
			{
				if (m.CanAttack == true)
					TotalAttack += m.AttackDamage;
			}
			if (poGame.CurrentPlayer.Hero.CanAttack)
			{
				TotalAttack++;
			}
			return TotalAttack;
		}
		public int TotalOpponentHealth(SabberStoneCoreAi.POGame.POGame poGame)
		{
			int TotalOppHealth = poGame.CurrentOpponent.Hero.Health;
			foreach (Minion m in poGame.CurrentOpponent.BoardZone.GetAll())
			{
				if (m.HasTaunt)
				{
					TotalOppHealth += m.Health;
				}
			}
			return TotalOppHealth;

		}

		public override void InitializeAgent()
		{
			Rnd = new Random();
		}
		public override void InitializeGame()
		{
		}
		public void FindNextMove(SabberStoneCoreAi.POGame.POGame poGame)
		{
			Controller opponent = poGame.CurrentOpponent;
			MCTSTree tree = new MCTSTree();
			MCTSTree rootNode = tree.GetRootNode();
			//rootNode.poGame.State = poGame.State;
			DateTime startTime = DateTime.UtcNow;
			while ((DateTime.UtcNow - startTime).TotalSeconds < 1000)
			{
				MCTSTree promisingNode = tree.GetPromisingNode(rootNode);
				if (tree.poGame.State != State.COMPLETE && tree.poGame.State != State.INVALID)
				{
					tree.ExpandNode(promisingNode);
				}
				MCTSTree nodeToExplore = promisingNode;
				if (nodeToExplore.childnodesList.Count > 0)
				{
					nodeToExplore = promisingNode.GetRandomChild();
				}
				PlayState playoutResult = SimRanPlayout(nodeToExplore);
				//poGame.State playoutResult = simRanPlayout(nodeToExplore, opponent)
				nodeToExplore.Backpropagation(nodeToExplore, playoutResult);
			}
			MCTSTree winnerNode = rootNode.GetChildWithMaxScore();
			List<PlayerTask> options = poGame.CurrentPlayer.Options();
			//return winnerNode.state.board;
		}
		public PlayState SimRanPlayout(MCTSTree nodetoexplore)
		{

			while (nodetoexplore.poGame.CurrentPlayer.PlayState == PlayState.PLAYING)
			{

			}
			PlayState playoutresult = PlayState.WON;
			return playoutresult;
		}
	}
}
