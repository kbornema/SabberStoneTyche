using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SabberStoneCoreAi.BotB
{
	enum Strategy { Exploitation, Exploration };

	class BotB : AbstractAgent
	{
		protected Strategy Strategy = Strategy.Exploitation;
		private SabberStoneCoreAi.POGame.POGame CurrentPoGame;

		public override void FinalizeAgent()
		{
		}

		public override void FinalizeGame()
		{
		}

		public override PlayerTask GetMove(SabberStoneCoreAi.POGame.POGame poGame)
		{
			CurrentPoGame = poGame;
			List<PlayerTask> actions = poGame.CurrentPlayer.Options();
			Dictionary<PlayerTask, POGame.POGame> resultedictionary = poGame.Simulate(actions);
			//LinkedList<PlayerTask> minionAttacks = new LinkedList<PlayerTask>();

			/*foreach (PlayerTask task in options)
			{
				if (task.PlayerTaskType == PlayerTaskType.MINION_ATTACK && task.Target == poGame.CurrentOpponent.Hero)
				{
					minionAttacks.AddLast(task);
				}
			}
			return options[1];
			*/
			List<int> rewards = GetActionsRewards(actions, resultedictionary);
			return actions[pickAction(rewards)];
		}

		/// <summary>
		/// calculates the reward for an action.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		private int ActionReward(POGame.POGame resultedState)
		{
			ActionResults results = new ActionResults(this.CurrentPoGame, resultedState);
			int reward = results.DamageDealt * 3 + results.MonstersKilled * 1 + results.MonstersPlaced * 3;
			return reward;
		}
		/// <summary>
		/// returns the rewards for every available action
		/// </summary>
		/// <param name="actions"></param>
		/// <returns> a list with rewards </returns>
		private List<int> GetActionsRewards(List<PlayerTask> actions, Dictionary<PlayerTask, POGame.POGame> taskResults)
		{
			List<int> rewards = new List<int>();

			foreach (PlayerTask action in actions)
			{
				int reward = 0;
				if (action.PlayerTaskType == PlayerTaskType.END_TURN)
				{
					reward = -1;
				}
				else
				{
					try
					{
						reward = ActionReward(taskResults[action]);

					}
					catch (NullReferenceException)
					{
						reward = 0;
					}

				}
				rewards.Add(reward);

			}
			return rewards;
		}

		/// <summary>
		/// returns an action based on the calculated rewards 
		/// </summary>
		/// <param name="rewards"></param>
		/// <returns> action number </returns>
		private int pickAction(List<int> rewards)
		{
			return rewards.IndexOf(rewards.Max());
		}

		public override void InitializeAgent()
		{
		}

		public override void InitializeGame()
		{
		}
	}
}
