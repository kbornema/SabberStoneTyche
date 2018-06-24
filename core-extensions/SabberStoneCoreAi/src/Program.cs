using System;
using SabberStoneCoreAi.Agent.ExampleAgents;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.Tyche;
using SabberStoneCoreAi.Tyche.Testing;
using SabberStoneCoreAi.Tyche.Learning;
using System.Collections.Generic;

namespace SabberStoneCoreAi
{
	internal class Program
	{
		private static void Main(string[] args)
		{

			var allDecks = ExamDecks.GetAll();

			var warriorDeck = ExamDecks.GetWarriorAsList();
			var mageDeck = ExamDecks.GetMageAsList();
			var shamanDeck = ExamDecks.GetShamanAsList();

			var allEnemyAgents = new List<AbstractAgent> { GetAgent(Agent.RandomLate), GetAgent(Agent.Random), GetAgent(Agent.FaceHunter) };

			var botB = new List<AbstractAgent> { new BotB.BotB() };
			var randomLateAgent = new List<AbstractAgent> { GetAgent(Agent.RandomLate) };
			var randomAgent = new List<AbstractAgent> { GetAgent(Agent.Random) };
			var faceHunterAgent = new List<AbstractAgent> { GetAgent(Agent.FaceHunter) };

			const int GENERATIONS = 5;

			LearnSetup learnSetup = new LearnSetup();

			//learnSetup.Clear();
			//learnSetup.Run(GENERATIONS, mageDeck, mageDeck, botB);

			//learnSetup.Clear();
			//learnSetup.Run(GENERATIONS, warriorDeck, warriorDeck, botB);

			learnSetup.Clear();
			learnSetup.Run(GENERATIONS, shamanDeck, shamanDeck, botB);
			

			/*
			const int ROUNDS = 100;
			const int MATCHES_PER_ROUND = 10;

			Debug.LogInfo("Total matches: " + (ROUNDS * MATCHES_PER_ROUND));
			
			TycheAgent myAgent = new TycheAgent();

			var enemyAgent = new BotB.BotB();

			MatchSetup training = new MatchSetup(myAgent, enemyAgent, false);
			training.RunRounds(allDecks, allDecks, ROUNDS, MATCHES_PER_ROUND);
			training.PrintFinalResults();

			Debug.LogInfo("Press a key to close.");
			Console.ReadLine();
			*/
		}

		enum Agent
		{
			Random,
			RandomLate,
			FaceHunter,

			Count
		}

		private static AbstractAgent GetAgent(Agent i)
		{
			if (i == Agent.Random)
				return new RandomAgent();

			else if (i == Agent.RandomLate)
				return new RandomAgentLateEnd();

			else if (i == Agent.FaceHunter)
				return new FaceHunter();

			return null;
		}
	}
}
