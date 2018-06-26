using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.Agent;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.BotA
{
    class BotA : AbstractAgent
    {
		public override void FinalizeAgent()
		{

		}

		public override void FinalizeGame()
		{

		}

		public override PlayerTask GetMove(POGame.POGame poGame)
		{
			//Console.WriteLine("STARTING GET MOVE");
			List<PlayerTask> options = poGame.CurrentPlayer.Options();

			PlayerTask bestTask = null;
			foreach (PlayerTask task in options)
			{
				//Console.Write("---->POSSIBLE ");
				printTask(task);

				/*if (task.HasSource)
				{
					Console.WriteLine("HAS SOURCE. SOURCE IS -->" + task.Source);
				}
				if (task.PlayerTaskType == PlayerTaskType.MINION_ATTACK && task.Target == poGame.CurrentOpponent.Hero)
				{
					Console.WriteLine("MINION ATTACKING OPONENT HERO");
				}
				if (task.HasTarget)
				{
					Console.WriteLine("HAS TARGET. TARGET IS -->"+task.Target);
					Console.WriteLine("HAS TARGET. CARD IS----->" + task.Target.Card.ToString());
				}
				else {
					Console.WriteLine("NOT TARGET");
				}
				
				if (task.PlayerTaskType == PlayerTaskType.PLAY_CARD) {
					Console.WriteLine("PLAYING CARD");
				}*/

				if (task.PlayerTaskType == PlayerTaskType.MINION_ATTACK && task.Target == poGame.CurrentOpponent.Hero)
				{
					//&&Console.Write("¡¡¡¡¡¡¡ATTACKING ENEMY HERO!!!!!!!!!");
					//printTask(task);
					return task;
				}


				if (task.PlayerTaskType == PlayerTaskType.MINION_ATTACK && task.Target is Minion)
				{
					//Console.Write("¡¡¡¡¡¡¡¡ATTACKING MINION!!!!!!!!!");
					//printTask(task);
					return task;
				}


				bestTask = task;


			}

			int myManaUsed = poGame.CurrentPlayer.TotalManaSpentThisGame;
			int myHeroPowerInGame = poGame.CurrentPlayer.NumTimesHeroPowerUsedThisGame;
			int myCurrentCardsToDraw = poGame.CurrentPlayer.NumCardsToDraw;
			int enemyMinionsKilled = poGame.CurrentOpponent.NumFriendlyMinionsThatDiedThisTurn;
			int currentTurn = poGame.Turn;

			/*
			Console.WriteLine("TURN: " + currentTurn + " " + myManaUsed + " " + myHeroPowerInGame + " " + myCurrentCardsToDraw + " " + enemyMinionsKilled);
			Console.WriteLine("SELECTED TASK TO EXECUTE ");
			printTask(bestTask);

			Console.ReadKey();
			*/
			return bestTask;
		}

		//Mejor hacer esto con todas las posibles en cada movimiento
		public int scoreTask(PlayerTask task, POGame.POGame poGame)
		{
			int score = 0;
			List<PlayerTask> tasks = new List<PlayerTask>();
			tasks.Add(task);

			Dictionary<PlayerTask, POGame.POGame> dict = poGame.Simulate(tasks);
			POGame.POGame afterSim = null;


			int myDiffHealth = poGame.CurrentPlayer.Hero.Health - afterSim.CurrentPlayer.Hero.Health;
			int enemyDiffHealth = poGame.CurrentOpponent.Hero.Health - afterSim.CurrentOpponent.Hero.Health;

			List<Minion> myMinions = new List<Minion>();
			List<Minion> enemyMinions = new List<Minion>();

			foreach (Minion m in poGame.Minions)
			{
				//Add to both lists
			}

			List<Minion> afterMyMinions = new List<Minion>();
			List<Minion> afterEnemyMinions = new List<Minion>();
			foreach (Minion m in afterSim.Minions)
			{
				//Add to both lists
			}

			//Get the minions dead/added

			//Get the minions damaged/healed

			return score;
		}

		public override void InitializeAgent()
		{

		}

		public override void InitializeGame()
		{

		}

		private void printTask(PlayerTask task)
		{
			/*
			Console.Write("TASK: " + task.PlayerTaskType + " " + task.Source + "----->" + task.Target + " (" + ")");
			if (task.Target != null)
				Console.Write(task.Target.Controller);
			else
				Console.Write("No target");
			Console.Write("\n");
			*/
		}
	}
}
